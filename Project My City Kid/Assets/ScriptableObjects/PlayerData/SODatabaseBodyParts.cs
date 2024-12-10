using UnityEngine;

[CreateAssetMenu(fileName = "DatabaseBodyPart", menuName = "TLSDigital/Player/DatabaseBodyPart", order = 2)]
public class SODatabaseBodyParts : ScriptableObject
{
    [Header("Character Skin")] // Wird in der Creation definiert
    public PlayerSkinColor[] characterSkinColor;
    [Space]
    [Header("Female Base Body")] // Wird in der Creation definiert
    public PlayerBodyPart[] femaleBodyParts;
    [Space]
    [Header("Male Character")] // Wird in der Creation definiert
    public PlayerBodyPart[] maleBodyParts;
}