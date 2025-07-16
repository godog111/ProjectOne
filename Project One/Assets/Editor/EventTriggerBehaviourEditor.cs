#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[CustomEditor(typeof(EventTriggerClip))] // 注意：这里改为针对Clip而不是Behaviour
public class EventTriggerEditor : Editor
{
    private SerializedProperty templateProp;
    private string presetName = "NewEventPreset";
    private bool includeTarget = false;

    private void OnEnable()
    {
        templateProp = serializedObject.FindProperty("template");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        // 绘制默认属性
        EditorGUILayout.PropertyField(templateProp);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("预设工具", EditorStyles.boldLabel);
        
        presetName = EditorGUILayout.TextField("预设名称", presetName);
        includeTarget = EditorGUILayout.Toggle("包含目标对象", includeTarget);
        
        if (GUILayout.Button("生成预设"))
        {
            GeneratePreset();
        }
        
        serializedObject.ApplyModifiedProperties();
    }

    private void GeneratePreset()
    {
        // 获取模板中的行为数据
        var template = templateProp.objectReferenceValue as EventTriggerClip;
        if (template == null) return;
        
        // 获取保存路径
        string path = EditorUtility.SaveFilePanelInProject(
            "保存事件触发器预设",
            presetName,
            "asset",
            "请选择保存预设的位置");
        
        if (!string.IsNullOrEmpty(path))
        {
            // 创建新预设
            var preset = ScriptableObject.CreateInstance<EventTriggerPreset>();
            
            // 从模板复制数据
            var behaviour = template.template;
            preset.triggerType = behaviour.triggerType;
            preset.methodName = behaviour.methodName;
            preset.methodParameter = behaviour.methodParameter;
            preset.useConditionSystem = behaviour.useConditionSystem;
            preset.conditionType = behaviour.conditionType;
            preset.conditionID = behaviour.conditionID;
            preset.conditionValue = behaviour.conditionValue;
            preset.overrideTarget = includeTarget;
            if (includeTarget)
            {
                preset.targetOverride = behaviour.targetObject;
            }
            
            preset.name = presetName;
            
            // 保存资产
            AssetDatabase.CreateAsset(preset, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = preset;
        }
    }
}
#endif