using UnityEngine;
using System.Collections;
using System;

public class ComputeShaderTest : MonoBehaviour
{
    public VolumeTextureGenerator TextureGenerator;

    public ComputeShader ParticleUpdater;
    public Material ParticleMaterial;
    public int ParticleCount;
    [Range(0, .1f)]
	public float ParticleSize;
	public float VelocityScale;
    
    public float ParticleLifetime = 1;
    public float ParticleSpeed = .1f;

    public const int GroupSize = 128;
    private int updateParticlesKernel;

    private ComputeBuffer particlesBuffer;
    private const int particleStride = sizeof(float) * 3 //Original Position 
        + sizeof(float) * 3 // Current Position 
        + sizeof(float); //Time
    private const int particlePositionsStride = sizeof(float) * 3 //Position 
        + sizeof(float); //Speed
    private ComputeBuffer quadPoints;
    private ComputeBuffer particlePositionsBuffer;
    private const int quadStride = 12;

    struct WindData
    {
        public Vector3 originalPosition;
        public Vector3 currentPosition;
        public float time;
    };

    Vector3 GetRandomPointInCube()
    {
        return new Vector3
            (
                UnityEngine.Random.value,
                UnityEngine.Random.value,
                UnityEngine.Random.value
            );
    }

    void Start()
    {
        updateParticlesKernel = ParticleUpdater.FindKernel("UpdateParticles");

        WindData[] data = new WindData[ParticleCount];
        Vector4[] positionData = new Vector4[ParticleCount];

        particlesBuffer = new ComputeBuffer(ParticleCount, particleStride);
        particlePositionsBuffer = new ComputeBuffer(ParticleCount, particlePositionsStride);

        for (int i = 0; i < ParticleCount; i++)
        {
            data[i].originalPosition = GetRandomPointInCube();
            data[i].currentPosition = data[i].originalPosition;
            data[i].time = UnityEngine.Random.value * ParticleLifetime;
        }

        particlesBuffer.SetData(data);

        quadPoints = new ComputeBuffer(6, quadStride);
        quadPoints.SetData(new[]
        {
            new Vector3(-.5f, .5f),
            new Vector3(.5f, .5f),
            new Vector3(.5f, -.5f),
            new Vector3(.5f, -.5f),
            new Vector3(-.5f, -.5f),
            new Vector3(-.5f, .5f)
        });

        Texture3D assembledTexture = TextureGenerator.GetAssembledTexture();
        ParticleUpdater.SetTexture(updateParticlesKernel, "MapTexture", assembledTexture);
        ParticleMaterial.SetTexture("MapTexture", assembledTexture);
    }

    void Update()
    {
        // Setting these every frame so that I can noodle with the shader without having to restart the project
        ParticleUpdater.SetBuffer(updateParticlesKernel, "particles", particlesBuffer);
        ParticleUpdater.SetBuffer(updateParticlesKernel, "particlePositions", particlePositionsBuffer);
     
        ParticleUpdater.SetFloat("deltaTime", Time.deltaTime);
        ParticleUpdater.SetFloat("particleLifetime", ParticleLifetime);
        ParticleUpdater.SetFloat("particleSpeed", ParticleSpeed);

        int numberofGroups = Mathf.CeilToInt((float)ParticleCount / GroupSize);
        ParticleUpdater.Dispatch(updateParticlesKernel, numberofGroups, 1, 1);
    }
	
	void OnDestroy()
	{
		quadPoints.Dispose();
		particlesBuffer.Dispose();
		particlePositionsBuffer.Dispose();
	}
     
    void OnRenderObject()
    {
        ParticleMaterial.SetMatrix("masterTransform", transform.localToWorldMatrix);
        ParticleMaterial.SetBuffer("particles", particlePositionsBuffer);
        ParticleMaterial.SetBuffer("quadPoints", quadPoints);
		ParticleMaterial.SetFloat("_CardSize", ParticleSize);
		ParticleMaterial.SetFloat("_VelocityScale", VelocityScale);
        ParticleMaterial.SetPass(0);
        Graphics.DrawProceduralNow(MeshTopology.Triangles, 6, ParticleCount);
    }
}
