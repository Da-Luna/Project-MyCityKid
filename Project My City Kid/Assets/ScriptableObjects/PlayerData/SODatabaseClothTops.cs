using UnityEngine;

[CreateAssetMenu(fileName = "DatabaseClothTops", menuName = "TLSDigital/Player/DatabaseClothTops", order = 4)]
public class SODatabaseClothTops : ScriptableObject
{
    [Header("Female Tops")]
    public PlayerClothing[] femaleTop;
    [Space]
    [Header("Female Tops Texture")]
    public PlayerTextures[] femaleTopTextures;
    [Space]
    [Header("Male Tops")]
    public PlayerClothing[] maleTop;
    [Space]
    [Header("Male Tops Texture")]
    public PlayerTextures[] maleTopTextures;
}