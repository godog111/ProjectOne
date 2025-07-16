#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text;
using ExcelDataReader;
using Newtonsoft.Json;
using System.Data;
using System.Collections.Generic;

public class ExcelToJSONConverter : EditorWindow
{
    private string excelPath = "";
    private string outputPath = "Assets/Resources/GameData";
    private bool prettyPrint = true;

    [MenuItem("Tools/Excel to JSON Converter")]
    public static void ShowWindow()
    {
        GetWindow<ExcelToJSONConverter>("Excel Converter");
    }

    void OnGUI()
    {
        GUILayout.Label("Excel to JSON Converter", EditorStyles.boldLabel);

        // 文件选择区域
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Excel File:", GUILayout.Width(70));
        excelPath = EditorGUILayout.TextField(excelPath);
        if (GUILayout.Button("Browse...", GUILayout.Width(80)))
        {
            excelPath = EditorUtility.OpenFilePanel("Select Excel File", "", "xlsx,xls");
            Repaint();
        }
        EditorGUILayout.EndHorizontal();

        // 输出路径选择
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Output Path:", GUILayout.Width(70));
        outputPath = EditorGUILayout.TextField(outputPath);
        if (GUILayout.Button("Select Folder...", GUILayout.Width(80)))
        {
            outputPath = EditorUtility.SaveFolderPanel("Select Output Folder", outputPath, "");
            Repaint();
        }
        EditorGUILayout.EndHorizontal();

        // 格式选项
        prettyPrint = EditorGUILayout.Toggle("Pretty Format", prettyPrint);

        // 转换按钮
        if (GUILayout.Button("Convert to JSON"))
        {
            if (File.Exists(excelPath))
            {
                ConvertExcelToJson();
            }
            else
            {
                Debug.LogError("Excel file not found!");
            }
        }
    }

    private void ConvertExcelToJson()
    {
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        
        using (var stream = File.Open(excelPath, FileMode.Open, FileAccess.Read))
        {
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                {
                    ConfigureDataTable = _ => new ExcelDataTableConfiguration()
                    {
                        UseHeaderRow = true // 使用第一行作为列名
                    }
                });

                foreach (DataTable table in result.Tables)
                {
                    ProcessDataTable(table);
                }
            }
        }

        AssetDatabase.Refresh();
    }

     private void ProcessDataTable(DataTable table)
    {
        List<Dictionary<string, object>> dataList = new List<Dictionary<string, object>>();

        foreach (DataRow row in table.Rows)
        {
            Dictionary<string, object> entry = new Dictionary<string, object>();
            
            foreach (DataColumn column in table.Columns)
            {
                object value = row[column];
                string fieldName = column.ColumnName;

                // 特殊处理数组字段
                if (fieldName == "requiredItemID" && value is string)
                {
                    // 将字符串转换为float数组
                    var strValues = ((string)value).Split('|');
                    List<float> floatList = new List<float>();
                    foreach (var str in strValues)
                    {
                        if (float.TryParse(str, out float f))
                        {
                            floatList.Add(f);
                        }
                    }
                    value = floatList.ToArray();
                }

                // 处理布尔值
                if (fieldName == "requireSpecialItem")
                {
                    value = value.ToString().ToLower() == "true";
                }

                if ((fieldName == "spawnPosition" || fieldName.EndsWith("Position")) && value is string)
                    {
                        string strValue = (string)value;
            
                     // 情况1：处理"x:50,y:60"格式
                         if (strValue.Contains("x:") && strValue.Contains("y:"))
                            {
                                 strValue = strValue.Replace("x:", "").Replace("y:", "");
                                 var parts = strValue.Split(',');
                                if (parts.Length == 2 && float.TryParse(parts[0], out float x) && float.TryParse(parts[1], out float y))
                                 {
                                    value = new Dictionary<string, float> { {"x", x}, {"y", y} };
                                    }
                            }
                    }

                entry[fieldName] = value;
            }

            dataList.Add(entry);
        }

        // 构建目标结构
        var wrapper = new Dictionary<string, object>
        {
            { table.TableName, dataList } // 直接使用表明作为键
        };

        string json = JsonConvert.SerializeObject(
            wrapper,
            prettyPrint ? Formatting.Indented : Formatting.None,
            new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore // 忽略空值
            }
        );

        string outputFile = Path.Combine(outputPath, $"{table.TableName}.json");
        File.WriteAllText(outputFile, json);
    }
}
#endif