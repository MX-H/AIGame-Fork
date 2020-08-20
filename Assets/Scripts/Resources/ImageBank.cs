using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New ImageBank", menuName = "ImageBank", order = 51)]
public class ImageBank : ScriptableObject
{
    [SerializeField]
    private List<Texture2D> textures;

    ImageBank()
    {
        textures = new List<Texture2D>();
    }

    public int GetSize()
    {
        return textures.Count;
    }
    public Texture2D GetTexture(int i)
    {
        return textures[i];
    }

}
