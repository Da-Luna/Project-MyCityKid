using UnityEngine;

[CreateAssetMenu(fileName = "DatabaseHeads", menuName = "TLSDigital/Player/DatabaseHeads", order = 1)]
public class SODatabaseHeads : ScriptableObject
{
    [Header("Female Character Heads")]
    public PlayerHead[] femaleHeadList;
    [Space]
    [Header("Female Heads Textures")]
    public PlayerTextures[] femaleHeadTextureList;
    [Space]
    [Header("Male Character Heads")]
    public PlayerHead[] maleHeadList;
    [Space]
    [Header("Female Heads Textures")]
    public PlayerTextures[] maleHeadTextureList;
}