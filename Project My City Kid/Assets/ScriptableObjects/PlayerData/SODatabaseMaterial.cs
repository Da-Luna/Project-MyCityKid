using UnityEngine;

[CreateAssetMenu(fileName = "DatabaseMaterials", menuName = "TLSDigital/Player/DatabaseMaterials", order = 7)]
public class SODatabaseMaterials : ScriptableObject
{
    [Header("Female Character Materials")]
    [Space]
    [Space]
    public PlayerMaterial[] baseBodyMaterials;
}