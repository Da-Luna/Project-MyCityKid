using UnityEngine;

[CreateAssetMenu(fileName = "DatabaseClothShoes", menuName = "TLSDigital/Player/DatabaseClothShoes", order = 6)]
public class SODatabaseClothShoes : ScriptableObject
{
    [Header("Female Shoes")]
    public PlayerClothing[] femaleShoes;
    [Space]
    [Header("Female Pants Shoes")]
    public PlayerTextures[] femaleShoesTextures;
    [Space]
    [Header("Male Shoes")]
    public PlayerClothing[] maleShoes;
    [Space]
    [Header("Male Pants Shoes")]
    public PlayerTextures[] maleShoesTextures;
}