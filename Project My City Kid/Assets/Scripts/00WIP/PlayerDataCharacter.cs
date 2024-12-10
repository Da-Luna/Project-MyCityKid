using UnityEngine;
using System;

#region PLAYER DATA CLASSES

[Serializable]
public class PlayerTemporaryStorage
{
    public int SkinColorIndex = 1;
    public int GenderIndex = 0;

    public int HeadIndex = 0;
    public int EyesTextureIndex = 0;

    public int HairsIndex = 0;
    public int HairsTexIndex = 2;

    public int TopIndex = 0;
    public int TopTextureIndex = 4;

    public int PantsIndex = 0;
    public int PantsTextureIndex = 1;

    public int ShoesIndex = 0;
    public int ShoeTextureIndex = 2;
}

[Serializable]
public enum Gender { Female, Male }

[Serializable]
public enum ClothingCategory { Pants, PantsShorts, Shoes, Tops, TopSleeveless }

[Serializable]
public class PlayerHead
{
    public string characterName;
    public string iD;
    public int objIndexNo;
    public Sprite headSprite;
    public Gender gender;
    public Mesh headMesh;
    public Material headMaterial;
    public Mesh eyesMesh;
    public int eyesTexIndexNo;
    public Material eyesMaterial;
    public Mesh eyelashesMesh;
    public Material eyelashesMaterial;
}
[Serializable]
public class PlayerSlot
{
    public string slotName;
    public string iD;
    public int objIndexNo;
    public Mesh slotMesh;
}
[Serializable]
public class PlayerHairs
{
    public string hairName;
    public string iD;
    public int objIndexNo;
    public Sprite hairSprite;
    public Mesh hairMesh;
    public int hairTexIndexNo;
    public Material hairMaterial;
}
[Serializable]
public class PlayerBodyPart
{
    public string bodyPartName;
    public string iD;
    public int objIndexNo;
    public Mesh bodyPartMesh;
    public Material bodyPartMaterial;
}
[Serializable]
public class PlayerClothing
{
    public string clothName;
    public string iD;
    public int objIndexNo;
    public Sprite clotSprite;
    public ClothingCategory clothCategory;
    public Mesh clothMesh;
    public int clothTexIndexNo;
    public Material clothMaterial;
}
[Serializable]
public class PlayerMaterial
{
    public string matName;
    public string iD;
    public int objIndexNo;
    public string matTargetKey;
    public Material matObject;
}
[Serializable]
public class PlayerTextures
{
    public string texName;
    public string iD;
    public int objIndexNo;
    public string matTargetKey;
    public Texture objTexture;
}
[Serializable]
public class PlayerSkinColor
{
    public string iD;
    public int objIndexNo;
    public Color skinColor;
}
[Serializable]
public class PlayerAttatchSlots
{
    public SkinnedMeshRenderer headRenderer;
    public SkinnedMeshRenderer eyesRenderer;
    public SkinnedMeshRenderer eyelashesRenderer;
    public SkinnedMeshRenderer hairRenderer;
    public SkinnedMeshRenderer topRenderer;
    public SkinnedMeshRenderer pantsRenderer;
    public SkinnedMeshRenderer shoesRenderer;

    public SkinnedMeshRenderer baseArmsRenderer;
    public SkinnedMeshRenderer baseHandsRenderer;
    public SkinnedMeshRenderer baseLegsRenderer;
    public SkinnedMeshRenderer baseTorsoRenderer;
}
[Serializable]
public class PlayerTargetIDs
{
    public string targetSkinColorID;

    public string targetHeadID;
    public string targetEyesTexID;

    public string targetHairsID;
    public string targetHairsTexID;

    public string targetTopID;
    public string targetTopTexID;

    public string targetPantsID;
    public string targetPantsTexID;

    public string targetShoesID;
    public string targetShoesTexID;
}
[Serializable]
public class SOPlayerDatabases
{
    public SODatabaseHeads sODatabaseHeads;
    public SODatabaseBodyParts sODatabaseBodyParts;
    public SODatabaseHairs sODatabaseHairs;
    public SODatabaseClothTops sODatabaseClothTops;
    public SODatabaseClothPants sODatabaseClothPants;
    public SODatabaseClothShoes sODatabaseClothShoes;
    public SODatabaseMaterials sODatabaseMaterials;
}

#endregion // #region PLAYER DATA CLASSES

public class PlayerDataCharacter : MonoBehaviour
{

    [Header("Database References")]

    [SerializeField]
    SOPlayerDatabases sOPlayerDatabases;
    
    [SerializeField]
    PlayerTemporaryStorage playerTmpStorage;

    [Header("Target IDs")]

    [SerializeField]
    PlayerTargetIDs targetIDList;

    #region STATIC ELEMENTS

    [Header("STATIC ELEMENTS")]

    [SerializeField]
    int characterSkinColor;

    [Space]
    [SerializeField]
    PlayerHead characterHead;

    [SerializeField]
    PlayerHairs characterHairs;

    [SerializeField]
    PlayerAttatchSlots playerAttatchSlots;

    #endregion // STATIC ELEMENTS

    #region MODULAR ELEMENTS

    [Header("MODULAR ELEMENTS")]

    [SerializeField]
    PlayerClothing playerTop;

    [SerializeField]
    PlayerClothing playerPants;

    [SerializeField]
    PlayerClothing PlayerShoes;

    #endregion // MODULAR ELEMENTS

    protected Color skinMatColor;

    void Start()
    {
        CharacterAssemble();
    }

    public void DatabaseSubmit()
    {
        playerTmpStorage.SkinColorIndex = characterSkinColor;

        if (characterHead.gender == Gender.Female)
            playerTmpStorage.GenderIndex = 0;
        else if (characterHead.gender == Gender.Male)
            playerTmpStorage.GenderIndex = 1;

        playerTmpStorage.HeadIndex = characterHead.objIndexNo;
        playerTmpStorage.EyesTextureIndex = characterHead.eyesTexIndexNo;

        playerTmpStorage.HairsIndex = characterHairs.objIndexNo;
        playerTmpStorage.HairsTexIndex = characterHairs.hairTexIndexNo;

        playerTmpStorage.TopIndex = playerTop.objIndexNo;
        playerTmpStorage.TopTextureIndex = playerTop.clothTexIndexNo;

        playerTmpStorage.PantsIndex = playerTop.objIndexNo;
        playerTmpStorage.PantsTextureIndex = playerTop.clothTexIndexNo;

        playerTmpStorage.ShoesIndex = playerTop.objIndexNo;
        playerTmpStorage.ShoeTextureIndex = playerTop.clothTexIndexNo;
    }

    public int CharacterGender()
    {
        if (characterHead.gender == Gender.Female)
            return 0;
        else if (characterHead.gender == Gender.Male)
            return 1;

        return -1;
    }

    public void CharacterAssemble()
    {
        AssignCharacter();
    }

    void AssignCharacter()
    {
        AssignBaseSkin();
        AssignPlayerHead();
        AssignPlayerHairs();
        AssignPlayerTop();
        AssignPlayerPants();
        AssignPlayerShoes();
    }

    #region Apply Single Character Parts

    public void AssignBaseSkin()
    {
        int skinIndex = playerTmpStorage.SkinColorIndex;
        characterSkinColor = skinIndex;
        var skinColor = sOPlayerDatabases.sODatabaseBodyParts;
        skinMatColor = skinColor.characterSkinColor[skinIndex].skinColor;

        int fHandIndex = 0;
        skinColor.femaleBodyParts[fHandIndex].bodyPartMaterial.color = skinColor.characterSkinColor[skinIndex].skinColor;
        int fArmsIndex = 1;
        skinColor.femaleBodyParts[fArmsIndex].bodyPartMaterial.color = skinColor.characterSkinColor[skinIndex].skinColor;
        int fLegsIndex = 2;
        skinColor.femaleBodyParts[fLegsIndex].bodyPartMaterial.color = skinColor.characterSkinColor[skinIndex].skinColor;
        int fTorsoIndex = 3;
        skinColor.femaleBodyParts[fTorsoIndex].bodyPartMaterial.color = skinColor.characterSkinColor[skinIndex].skinColor;

        int mHandIndex = 0;
        skinColor.maleBodyParts[mHandIndex].bodyPartMaterial.color = skinColor.characterSkinColor[skinIndex].skinColor;
        int mArmsIndex = 1;
        skinColor.maleBodyParts[mArmsIndex].bodyPartMaterial.color = skinColor.characterSkinColor[skinIndex].skinColor;
        int mLegsIndex = 2;
        skinColor.maleBodyParts[mLegsIndex].bodyPartMaterial.color = skinColor.characterSkinColor[skinIndex].skinColor;
        int mTorsoIndex = 3;
        skinColor.maleBodyParts[mTorsoIndex].bodyPartMaterial.color = skinColor.characterSkinColor[skinIndex].skinColor;
    }

    public void AssignPlayerHead()
    {
        int headIndex = playerTmpStorage.HeadIndex;
        int eyesTexIndex = playerTmpStorage.EyesTextureIndex;
        var charHead = sOPlayerDatabases.sODatabaseHeads.femaleHeadList;
        var charEyesTex = sOPlayerDatabases.sODatabaseHeads.femaleHeadTextureList;
        
        if (playerTmpStorage.GenderIndex == 0)
        {
            charHead = sOPlayerDatabases.sODatabaseHeads.femaleHeadList;
            characterHead.gender = Gender.Female;
        }
        else if (playerTmpStorage.GenderIndex == 1)
        {
            charHead = sOPlayerDatabases.sODatabaseHeads.maleHeadList;
            characterHead.gender = Gender.Male;
        }

        //headIndex = Array.FindIndex(charHead, head => head.iD == targetHeadID);
        //eyesTexIndex = Array.FindIndex(charEyesTex, eyes => eyes.iD == targetIDList.targetEyesTexID);

        if (headIndex != -1)
        {
            playerAttatchSlots.headRenderer.sharedMesh = charHead[headIndex].headMesh;
            playerAttatchSlots.headRenderer.material = charHead[headIndex].headMaterial;
            
            playerAttatchSlots.eyesRenderer.sharedMesh = charHead[headIndex].eyesMesh;
            playerAttatchSlots.eyesRenderer.material = charHead[headIndex].eyesMaterial;
            playerAttatchSlots.eyesRenderer.material.mainTexture = charEyesTex[eyesTexIndex].objTexture;

            playerAttatchSlots.eyelashesRenderer.sharedMesh = charHead[headIndex].eyelashesMesh;
            playerAttatchSlots.eyelashesRenderer.material = charHead[headIndex].eyelashesMaterial;

            playerAttatchSlots.headRenderer.material.color = skinMatColor;

            characterHead.headMesh = charHead[headIndex].headMesh;
            characterHead.eyesMesh = charHead[headIndex].eyesMesh;
            characterHead.eyelashesMesh = charHead[headIndex].eyelashesMesh;
            
            characterHead.headMaterial = charHead[headIndex].headMaterial;
            characterHead.eyesMaterial = charHead[headIndex].eyesMaterial;
            characterHead.eyelashesMaterial = charHead[headIndex].eyelashesMaterial;

            characterHead.characterName = charHead[headIndex].characterName;
            characterHead.iD = charHead[headIndex].iD;
            characterHead.objIndexNo = headIndex;
            characterHead.gender = charHead[headIndex].gender;
            characterHead.eyesTexIndexNo = charHead[headIndex].eyesTexIndexNo;
        }
    }

    public void AssignPlayerHairs()
    {
        int hairsIndex = playerTmpStorage.HairsIndex;
        int hairsTexIndex = playerTmpStorage.HairsTexIndex;

        var charHairs = sOPlayerDatabases.sODatabaseHairs.femaleHairs;
        var charHairsTexture = sOPlayerDatabases.sODatabaseHairs.femaleHairsTextures;

        if (playerTmpStorage.GenderIndex == 0)
        {
            charHairs = sOPlayerDatabases.sODatabaseHairs.femaleHairs;
            charHairsTexture = sOPlayerDatabases.sODatabaseHairs.femaleHairsTextures;
        }
        else if(playerTmpStorage.GenderIndex == 1)
        {
            charHairs = sOPlayerDatabases.sODatabaseHairs.maleHairs;
            charHairsTexture = sOPlayerDatabases.sODatabaseHairs.maleHairsTextures;
        }

        //hairsIndex = Array.FindIndex(charHairs, hairs => hairs.iD == targetHairsID);
        //hairsTexIndex = Array.FindIndex(charHairsTexture, hairsTex => hairsTex.iD == targetIDList.targetHairsTexID);

        if (hairsIndex != -1)
        {
            playerAttatchSlots.hairRenderer.sharedMesh = charHairs[hairsIndex].hairMesh;
            playerAttatchSlots.hairRenderer.material = charHairs[hairsIndex].hairMaterial;           
            playerAttatchSlots.hairRenderer.material.mainTexture = charHairsTexture[hairsTexIndex].objTexture;

            characterHairs.hairName = charHairs[hairsIndex].hairName;
            characterHairs.iD = charHairs[hairsIndex].iD;
            characterHairs.objIndexNo = hairsIndex;
            characterHairs.hairMesh = charHairs[hairsIndex].hairMesh;
            characterHairs.hairMaterial = charHairs[hairsIndex].hairMaterial;
            characterHairs.hairTexIndexNo = hairsTexIndex;
        }
    }

    public void AssignPlayerTop()
    {
        int topIndex = playerTmpStorage.TopIndex;
        int topTexIndex = playerTmpStorage.TopTextureIndex;

        var charTop = sOPlayerDatabases.sODatabaseClothTops.femaleTop;
        var charHairsTexture = sOPlayerDatabases.sODatabaseClothTops.femaleTopTextures;

        if (playerTmpStorage.GenderIndex == 0)
        {
            charTop = sOPlayerDatabases.sODatabaseClothTops.femaleTop;
            charHairsTexture = sOPlayerDatabases.sODatabaseClothTops.femaleTopTextures;
        }
        else if (playerTmpStorage.GenderIndex == 1)
        {
            charTop = sOPlayerDatabases.sODatabaseClothTops.maleTop;
            charHairsTexture = sOPlayerDatabases.sODatabaseClothTops.maleTopTextures;
        }

        //topIndex = Array.FindIndex(charTop, top => top.iD == targetTopID);
        //topTexIndex = Array.FindIndex(charHairsTexture, topTex => topTex.iD == targetIDList.targetTopTexID);

        if (topIndex != -1)
        {
            playerAttatchSlots.topRenderer.sharedMesh = charTop[topIndex].clothMesh;
            playerAttatchSlots.topRenderer.sharedMaterial = charTop[topIndex].clothMaterial;
            playerAttatchSlots.topRenderer.material.mainTexture = charHairsTexture[topTexIndex].objTexture;

            playerTop.clothName = charTop[topIndex].clothName;
            playerTop.iD = charTop[topIndex].iD;
            playerTop.objIndexNo = topIndex;
            playerTop.clothCategory = charTop[topIndex].clothCategory;
            playerTop.clothMesh = charTop[topIndex].clothMesh;
            playerTop.clothTexIndexNo = topTexIndex;
            playerTop.clothMaterial = charTop[topIndex].clothMaterial;

            playerAttatchSlots.baseHandsRenderer.sharedMesh = null;
            playerAttatchSlots.baseHandsRenderer.material = null;
            playerAttatchSlots.baseArmsRenderer.sharedMesh = null;
            playerAttatchSlots.baseArmsRenderer.material = null;

            if (playerTop.clothCategory == ClothingCategory.Tops)
            {
                int handsIndex;
                var handsBodyPart = sOPlayerDatabases.sODatabaseBodyParts;
                
                if (characterHead.gender == Gender.Female)
                {
                    handsIndex = Array.FindIndex(handsBodyPart.femaleBodyParts, hands => hands.iD == "FHands");
                    playerAttatchSlots.baseHandsRenderer.sharedMesh = handsBodyPart.femaleBodyParts[handsIndex].bodyPartMesh;
                    playerAttatchSlots.baseHandsRenderer.material = handsBodyPart.femaleBodyParts[handsIndex].bodyPartMaterial;
                }
                else if (characterHead.gender == Gender.Male)
                {
                    handsIndex = Array.FindIndex(handsBodyPart.maleBodyParts, hands => hands.iD == "MHands");
                    playerAttatchSlots.baseHandsRenderer.sharedMesh = handsBodyPart.maleBodyParts[handsIndex].bodyPartMesh;
                    playerAttatchSlots.baseHandsRenderer.material = handsBodyPart.maleBodyParts[handsIndex].bodyPartMaterial;
                }
            }
            else if (playerTop.clothCategory == ClothingCategory.TopSleeveless)
            {
                int handsIndex;
                var handsBodyPart = sOPlayerDatabases.sODatabaseBodyParts;

                int armsIndex;
                var armsBodyPart = sOPlayerDatabases.sODatabaseBodyParts;

                if (playerTmpStorage.GenderIndex == 0)
                {
                    handsIndex = Array.FindIndex(handsBodyPart.femaleBodyParts, hands => hands.iD == "FHands");
                    playerAttatchSlots.baseHandsRenderer.sharedMesh = handsBodyPart.femaleBodyParts[handsIndex].bodyPartMesh;
                    playerAttatchSlots.baseHandsRenderer.material = handsBodyPart.femaleBodyParts[handsIndex].bodyPartMaterial;

                    armsIndex = Array.FindIndex(armsBodyPart.femaleBodyParts, arms => arms.iD == "FArms");
                    playerAttatchSlots.baseArmsRenderer.sharedMesh = armsBodyPart.femaleBodyParts[armsIndex].bodyPartMesh;
                    playerAttatchSlots.baseArmsRenderer.material = armsBodyPart.femaleBodyParts[armsIndex].bodyPartMaterial;
                }
                else if (playerTmpStorage.GenderIndex == 1)
                {
                    handsIndex = Array.FindIndex(handsBodyPart.maleBodyParts, hands => hands.iD == "MHands");
                    playerAttatchSlots.baseHandsRenderer.sharedMesh = handsBodyPart.maleBodyParts[handsIndex].bodyPartMesh;
                    playerAttatchSlots.baseHandsRenderer.material = handsBodyPart.maleBodyParts[handsIndex].bodyPartMaterial;

                    armsIndex = Array.FindIndex(handsBodyPart.maleBodyParts, hands => hands.iD == "MArms");
                    playerAttatchSlots.baseArmsRenderer.sharedMesh = armsBodyPart.maleBodyParts[armsIndex].bodyPartMesh;
                    playerAttatchSlots.baseArmsRenderer.material = armsBodyPart.maleBodyParts[armsIndex].bodyPartMaterial;
                }
            }
        }
    }

    public void AssignPlayerPants()
    {
        int pantsIndex = playerTmpStorage.PantsIndex;
        int pantsTexIndex = playerTmpStorage.PantsTextureIndex;
        int legsIndex;

        var charPants = sOPlayerDatabases.sODatabaseClothPants.femalePants;
        var charPantsTexture = sOPlayerDatabases.sODatabaseClothPants.femalePantsTextures;
        var legsBodyPart = sOPlayerDatabases.sODatabaseBodyParts;

        if (playerTmpStorage.GenderIndex == 0)
        {
            charPants = sOPlayerDatabases.sODatabaseClothPants.femalePants;
            charPantsTexture = sOPlayerDatabases.sODatabaseClothPants.femalePantsTextures;

            legsIndex = Array.FindIndex(legsBodyPart.femaleBodyParts, legs => legs.iD == "FLegs");
            playerAttatchSlots.baseLegsRenderer.sharedMesh = legsBodyPart.femaleBodyParts[legsIndex].bodyPartMesh;
            playerAttatchSlots.baseLegsRenderer.material = legsBodyPart.femaleBodyParts[legsIndex].bodyPartMaterial;
        }
        else if (playerTmpStorage.GenderIndex == 1)
        {
            charPants = sOPlayerDatabases.sODatabaseClothPants.malePants;
            charPantsTexture = sOPlayerDatabases.sODatabaseClothPants.malePantsTextures;

            legsIndex = Array.FindIndex(legsBodyPart.maleBodyParts, legs => legs.iD == "MLegs");
            playerAttatchSlots.baseLegsRenderer.sharedMesh = legsBodyPart.maleBodyParts[legsIndex].bodyPartMesh;
            playerAttatchSlots.baseLegsRenderer.material = legsBodyPart.maleBodyParts[legsIndex].bodyPartMaterial;
        }

        //pantsIndex = Array.FindIndex(charPants, pants => pants.iD == targetPantsID);
        //pantsTexIndex = Array.FindIndex(charPantsTexture, pantsTex => pantsTex.iD == targetIDList.targetPantsTexID);

        if (pantsIndex != -1)
        {
            playerAttatchSlots.pantsRenderer.sharedMesh = charPants[pantsIndex].clothMesh;
            playerAttatchSlots.pantsRenderer.sharedMaterial = charPants[pantsIndex].clothMaterial;
            playerAttatchSlots.pantsRenderer.material.mainTexture = charPantsTexture[pantsTexIndex].objTexture;

            playerPants.clothName = charPants[pantsIndex].clothName;
            playerPants.iD = charPants[pantsIndex].iD;
            playerPants.objIndexNo = pantsIndex;
            playerPants.clothCategory = charPants[pantsIndex].clothCategory;
            playerPants.clothMesh = charPants[pantsIndex].clothMesh;
            playerPants.clothTexIndexNo = pantsTexIndex;
            playerPants.clothMaterial = charPants[pantsIndex].clothMaterial;

            playerAttatchSlots.baseLegsRenderer.sharedMesh = null;
            playerAttatchSlots.baseLegsRenderer.material = null;
        }
    }

    public void AssignPlayerShoes()
    {
        int shoesIndex = playerTmpStorage.ShoesIndex;
        int shoesTexIndex = playerTmpStorage.ShoeTextureIndex;

        var charShoes = sOPlayerDatabases.sODatabaseClothShoes.femaleShoes;
        var charShoesTex = sOPlayerDatabases.sODatabaseClothShoes.femaleShoesTextures;

        if (playerTmpStorage.GenderIndex == 0)
        {
            charShoes = sOPlayerDatabases.sODatabaseClothShoes.femaleShoes;
            charShoesTex = sOPlayerDatabases.sODatabaseClothShoes.femaleShoesTextures;
        }
        else if (playerTmpStorage.GenderIndex == 1)
        {
            charShoes = sOPlayerDatabases.sODatabaseClothShoes.maleShoes;
            charShoesTex = sOPlayerDatabases.sODatabaseClothShoes.maleShoesTextures;
        }

        //shoesIndex = Array.FindIndex(charShoes, shoes => shoes.iD == targetShoesID);
        //shoesTexIndex = Array.FindIndex(charShoesTex, shoesTex => shoesTex.iD == targetIDList.targetShoesTexID);

        if (shoesIndex != -1)
        {
            playerAttatchSlots.shoesRenderer.sharedMesh = charShoes[shoesIndex].clothMesh;
            playerAttatchSlots.shoesRenderer.sharedMaterial = charShoes[shoesIndex].clothMaterial;
            playerAttatchSlots.shoesRenderer.material.mainTexture = charShoesTex[shoesTexIndex].objTexture;

            PlayerShoes.clothName = charShoes[shoesIndex].clothName;
            PlayerShoes.iD = charShoes[shoesIndex].iD;
            PlayerShoes.objIndexNo = shoesIndex;
            PlayerShoes.clothCategory = charShoes[shoesIndex].clothCategory;
            PlayerShoes.clothMesh = charShoes[shoesIndex].clothMesh;
            PlayerShoes.clothTexIndexNo = shoesTexIndex;
            PlayerShoes.clothMaterial = charShoes[shoesIndex].clothMaterial;
        }
    }

    #endregion // Apply Single Character Parts

    public void PlayerInit()
    {
        CharacterAssemble();
    }
}

