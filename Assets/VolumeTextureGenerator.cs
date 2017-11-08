using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class VolumeTextureGenerator
{
    public Texture2D[] Textures;

    public bool Clamp;

    public Texture3D GetAssembledTexture()
    {
        int TextureWidth = Textures.Length;
        int TextureDepth = Textures[0].width;
        int TextureHeight = Textures[0].height;
        Color[] colors = new Color[TextureWidth * TextureHeight * TextureDepth];
        Texture3D ret = new Texture3D(TextureWidth, TextureHeight, TextureDepth, TextureFormat.RGB24, false);
        for (int i = 0; i < TextureWidth; i++)
        {
            for (int j = 0; j < TextureDepth; j++)
            {
                for (int k = 0; k < TextureHeight; k++)
                {
                    int index = i + (j * TextureWidth) + (k * TextureDepth * TextureWidth);
                    colors[index] = Textures[i].GetPixel(j, k);
                }
            }
        }
        ret.SetPixels(colors); //TODO: Be a cool kid and use SetPixels32
        ret.Apply();
        ret.wrapMode = Clamp ? TextureWrapMode.Clamp : TextureWrapMode.Repeat; //Some Prototypes need this
        return ret;
    }

    public Color GetPixelColor(Vector3 cubePoint)
    {
        int textureIndex = Mathf.FloorToInt(cubePoint.x * Textures.Length);
        Texture2D texture = Textures[textureIndex];
        int x = Mathf.FloorToInt(cubePoint.y * texture.width);
        int y = Mathf.FloorToInt(cubePoint.z * texture.height);
        return texture.GetPixel(x, y);
    }
}
