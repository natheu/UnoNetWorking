using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardTextures", menuName = "ScriptableObjects/UnoCardTextures")]
public class UnoCardTextures : ScriptableObject
{
    [SerializeField]
    private string FolderPath = "";

    private List<List<Texture>> CardTextures = new List<List<Texture>>();

    public void CreateTexturesList(string folderPath)
    {
        FolderPath = folderPath;
    }
}
