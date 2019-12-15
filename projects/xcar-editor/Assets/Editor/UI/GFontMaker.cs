using UnityEngine;
using UnityEditor;

public class GFontMaker
{
    static Shader s_DefaultShader = Shader.Find("GUI/Text Shader");
    public static void Create(string assetPath)
    {
        var assets = AssetDatabase.LoadAllAssetRepresentationsAtPath(assetPath);
        if (assets == null || assets.Length <= 0)
            return;

        var count = assets.Length;
        var demo = assets[3] as Sprite;
        var texture = demo.texture;
        var characters = new CharacterInfo[count];

        var corner = Vector2.zero;
        var size = Vector2Int.zero;
        for (int i = 0; i < count; i++)
        {
            var sprite = assets[i] as Sprite;
            var symbol = new CharacterInfo();

            var uv_minx = sprite.rect.xMin / texture.width;
            var uv_miny = sprite.rect.yMin / texture.height;
            var uv_maxx = sprite.rect.xMax / texture.width;
            var uv_maxy = sprite.rect.yMax / texture.height;

            size.Set(Mathf.RoundToInt(sprite.rect.width), Mathf.RoundToInt(sprite.rect.height));

            symbol.index = int.Parse(sprite.name);
            symbol.advance = size.x;
            symbol.bearing = size.y;
            symbol.glyphWidth = size.x;
            symbol.glyphHeight = size.y;

            corner.Set(uv_minx, uv_miny);
            symbol.uvBottomLeft = corner;
            corner.Set(uv_maxx, uv_miny);
            symbol.uvBottomRight = corner;
            corner.Set(uv_minx, uv_maxy);
            symbol.uvTopLeft = corner;
            corner.Set(uv_maxx, uv_maxy);
            symbol.uvTopRight = corner;

            symbol.minX = 0;
            symbol.minY = -size.y;
            symbol.maxX = size.x;
            symbol.maxY = 0;

            characters[i] = symbol;
        }

        var path = assetPath.Replace("Res/Atlas", "Res/Font").Replace(".png", ".mat");
        var material = new Material(s_DefaultShader);
        material.SetTexture("_MainTex", texture);
        AssetDatabase.CreateAsset(material, path);

        var fontPath = path.Replace(".mat", ".fontsettings");
        var font = new Font();
        font.material = material;
        font.characterInfo = characters;
        AssetDatabase.CreateAsset(font, fontPath);
    }
}