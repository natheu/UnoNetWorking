using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardTextures", menuName = "ScriptableObjects/UnoCardTextures")]
public class UnoCardTextures : ScriptableObject
{
    [SerializeField]
    private string FolderPath = "";

    [SerializeField]
    private List<MatCard> CardTextures = new List<MatCard>();

    [SerializeField]
    private GameObject PrefabCards;

    public GameObject GetPrefab() { return PrefabCards; }

    struct MatCard
    {
        List<Material> mats;
    }

    public void CreateTexturesList(string folderPath)
    {
        FolderPath = folderPath;
    }
}
