using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class AtlasSplitter : EditorWindow
{
    private Texture2D atlasTexture;
    private TextAsset spriteDataFile; // 可选：如果有精灵数据文件
    private string outputPath = "Assets/SplitSprites";
    
    [MenuItem("Tools/Atlas Splitter")]
    public static void ShowWindow()
    {
        GetWindow<AtlasSplitter>("Atlas Splitter");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Atlas Splitting Tool", EditorStyles.boldLabel);
        
        atlasTexture = (Texture2D)EditorGUILayout.ObjectField("Atlas Texture", atlasTexture, typeof(Texture2D), false);
        spriteDataFile = (TextAsset)EditorGUILayout.ObjectField("Sprite Data (Optional)", spriteDataFile, typeof(TextAsset), false);
        
        outputPath = EditorGUILayout.TextField("Output Path", outputPath);
        
        if (GUILayout.Button("Split Atlas"))
        {
            if (atlasTexture == null)
            {
                EditorUtility.DisplayDialog("Error", "Please select an atlas texture first!", "OK");
                return;
            }
            
            SplitAtlas();
        }
    }
    
    private void SplitAtlas()
    {
        // 确保输出目录存在
        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }
        
        string atlasPath = AssetDatabase.GetAssetPath(atlasTexture);
        TextureImporter textureImporter = AssetImporter.GetAtPath(atlasPath) as TextureImporter;
        
        // 检查纹理是否设置为Sprite模式
        if (textureImporter.textureType != TextureImporterType.Sprite && 
            textureImporter.textureType != TextureImporterType.Default)
        {
            EditorUtility.DisplayDialog("Error", 
                "Texture type should be set to 'Sprite' or 'Default'. Please change it in Import Settings.", 
                "OK");
            return;
        }
        
        // 如果是多张精灵模式，可以直接获取精灵
        if (textureImporter.spriteImportMode == SpriteImportMode.Multiple)
        {
            SplitMultipleSprites(atlasPath);
        }
        else
        {
            // 如果没有精灵数据，尝试手动分割（需要提供分割信息）
            EditorUtility.DisplayDialog("Info", 
                "Texture is not in multiple sprite mode. You need to provide sprite data or set it up as multiple sprites first.", 
                "OK");
        }
    }
    
    private void SplitMultipleSprites(string atlasPath)
    {
        // 加载所有精灵
        Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(atlasPath).OfType<Sprite>().ToArray();
        
        if (sprites.Length == 0)
        {
            EditorUtility.DisplayDialog("Error", "No sprites found in the atlas!", "OK");
            return;
        }
        
        // 获取原始纹理
        Texture2D sourceTexture = atlasTexture;
        
        // 确保纹理是可读的
        if (!sourceTexture.isReadable)
        {
            // 尝试临时设置为可读
            TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(atlasPath);
            importer.isReadable = true;
            AssetDatabase.ImportAsset(atlasPath);
            sourceTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(atlasPath);
        }
        
        // 处理每个精灵
        foreach (Sprite sprite in sprites)
        {
            // 创建新纹理
            Texture2D newTexture = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
            
            // 复制像素
            Color[] pixels = sourceTexture.GetPixels(
                (int)sprite.rect.x, 
                (int)sprite.rect.y, 
                (int)sprite.rect.width, 
                (int)sprite.rect.height);
            
            newTexture.SetPixels(pixels);
            newTexture.Apply();
            
            // 保存为PNG
            byte[] pngData = newTexture.EncodeToPNG();
            string spritePath = Path.Combine(outputPath, sprite.name + ".png");
            File.WriteAllBytes(spritePath, pngData);
            
            // 销毁临时纹理
            DestroyImmediate(newTexture);
        }
        
        // 刷新资源数据库
        AssetDatabase.Refresh();
        
        EditorUtility.DisplayDialog("Success", 
            $"Successfully split {sprites.Length} sprites to {outputPath}", 
            "OK");
    }
}