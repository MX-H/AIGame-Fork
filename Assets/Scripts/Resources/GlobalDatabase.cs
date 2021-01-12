using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New GlobalDatabase", menuName = "Database", order = 51)]

public class GlobalDatabase : ScriptableObject
{
    [SerializeField]
    private ImageBank avatarSprites;

    [SerializeField]
    private DeckModelBank deckModels;

    public Texture2D GetAvatarTexture(int i)
    {
        return avatarSprites.GetTexture(i);
    }

    public IHistogram GetDeckModel(int i)
    {
        return deckModels.GetDeckModel(i);
    }
}
