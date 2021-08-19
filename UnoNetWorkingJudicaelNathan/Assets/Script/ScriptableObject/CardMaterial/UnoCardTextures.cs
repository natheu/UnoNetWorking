using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "CardTextures", menuName = "ScriptableObjects/UnoCardTextures")]
public class UnoCardTextures : ScriptableObject
{
    [SerializeField]
    private string FolderPath = "";

    [System.Serializable]
    public struct MatCard
    {
        public List<Texture> mats;
    }
    [SerializeField]
    private List<MatCard> CardTextures = new List<MatCard>();

    [SerializeField]
    private List<Texture> ANYColorText = new List<Texture>();
    [SerializeField]
    private GameObject PrefabCards;

    public GameObject GetPrefab() { return PrefabCards; }

    public Texture GetSprite(int i, int j)
    {
        if (j >= PlayerGameData.PLUS_FOUR)
            j -= PlayerGameData.PLUS_FOUR;
        return CardTextures[i].mats[j];
    }

    public Texture GetANYColorSprite(int i)
    {
        if (i < ANYColorText.Count)
            return ANYColorText[i];
        return null;
    }

    public void CreateTexturesList(string folderPath)
    {
        FolderPath = folderPath;
    }
}
