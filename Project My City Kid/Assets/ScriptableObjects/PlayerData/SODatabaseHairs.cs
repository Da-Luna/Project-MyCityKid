using UnityEngine;

[CreateAssetMenu(fileName = "DatabaseHairs", menuName = "TLSDigital/Player/DatabaseHairs", order = 3)]
public class SODatabaseHairs : ScriptableObject
{
    [Header("Female Character Hairs")]
    public PlayerHairs[] femaleHairs;
    [Space]
    [Header("Female Hairs Textures")]
    public PlayerTextures[] femaleHairsTextures;
    [Space]
    [Header("Male Character Hairs")]
    public PlayerHairs[] maleHairs;
    [Space]
    [Header("Male Hairs Textures")]
    public PlayerTextures[] maleHairsTextures;
}