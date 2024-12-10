using UnityEngine;

[CreateAssetMenu(fileName = "DatabaseClothPants", menuName = "TLSDigital/Player/DatabaseClothPants", order = 5)]
public class SODatabaseClothPants : ScriptableObject
{
    [Header("Female Pants")]
    public PlayerClothing[] femalePants;
    [Space]
    [Header("Female Pants Texture")]
    public PlayerTextures[] femalePantsTextures;
    [Space]
    [Header("Male Pants")]
    public PlayerClothing[] malePants;
    [Space]
    [Header("Male Pants Texture")]
    public PlayerTextures[] malePantsTextures;
}