using UnityEditor;
using UnityEngine;

namespace Scripts.Editor.Importers
{
    public class SkinSpriteImporter : AssetPostprocessor
    {
        const string Root = "Assets/_GAME/_EXTERNAL/Sprites/Common/Pers Skins";
        const float Ppu = 640f;
        static readonly Vector2 Pivot = new(0.5f, 30f/1280f);

        void OnPreprocessTexture()
        {
            if(!assetPath.StartsWith(Root))
                return;

            var ti = (TextureImporter)assetImporter;
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.spritePixelsPerUnit = Ppu;
            ti.alphaIsTransparency = true;
            ti.mipmapEnabled = false;
            ti.isReadable = false;
            ti.filterMode = FilterMode.Bilinear;

            var s = new TextureImporterSettings();
            ti.ReadTextureSettings(s);
            s.spriteMeshType = SpriteMeshType.Tight;
            s.spriteAlignment = (int)SpriteAlignment.Custom;
            s.spritePivot = Pivot;
            ti.SetTextureSettings(s);
        }
    }
}