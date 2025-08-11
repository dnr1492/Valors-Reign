using UnityEditor;
using UnityEngine;

public static class SetSpriteBorderTool
{
    //L=5, T=5, R=5, B=4 (TextureImporter는 L,B,R,T 순서)
    private static readonly Vector4 Border = new Vector4(5, 4, 5, 5);

    [MenuItem("정재욱/Sprite/[스프라이트를 선택한 상태에서 클릭] Set Border L5 T5 R5 B4")]
    private static void SetBorder()
    {
        foreach (var obj in Selection.objects)
        {
            var path = AssetDatabase.GetAssetPath(obj);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null || importer.textureType != TextureImporterType.Sprite)
                continue;

            //단일 스프라이트
            if (importer.spriteImportMode == SpriteImportMode.Single)
            {
                importer.spriteBorder = Border;
                ////Pivot도 같이 강제하고 싶으면 주석 해제
                //importer.spriteAlignment = (int)SpriteAlignment.Center;
                //importer.spritePivot = new Vector2(0.5f, 0.5f);
            }
            //멀티(스프라이트 시트)
            else if (importer.spriteImportMode == SpriteImportMode.Multiple)
            {
                var metas = importer.spritesheet;
                for (int i = 0; i < metas.Length; i++)
                {
                    metas[i].border = Border;
                    ////Pivot도 같이 강제하고 싶으면:
                    //metas[i].alignment = (int)SpriteAlignment.Center;
                    //metas[i].pivot = new Vector2(0.5f, 0.5f);
                }
                importer.spritesheet = metas;
            }

            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();
        }
        Debug.Log("SetSpriteBorder: 완료 (L=5, T=5, R=5, B=4)");
    }
}
