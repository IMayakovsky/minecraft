using UnityEngine;

[System.Serializable]
public class BlockType {

    public string BlockName;
    public bool IsVisible;
    public bool IsTransparent;
    public int Strength = 1;
    public Sprite Icon;
    public AudioClip FootSound;

    [Header("Textures ID")]
    public TextureTypeEnum TopTexture;
    public TextureTypeEnum SideTexture;
    public TextureTypeEnum BottomTexture;

    public TextureTypeEnum GetTextureId (int face) {

        switch (face) {

            case 0:
            case 1:
            case 4:
            case 5:
                return SideTexture;
            case 2:
                return TopTexture;
            case 3:
                return BottomTexture;
            default:
                return 0;
        }

    }

}