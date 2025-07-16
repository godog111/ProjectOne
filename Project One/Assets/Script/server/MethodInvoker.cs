using System;
using System.Reflection;
using UnityEngine;

/// <summary>
/// 通用方法调用器脚本
/// 允许通过配置参数动态调用其他类中的方法
/// </summary>
public class MethodInvoker : MonoBehaviour
{
    /// <summary>
    /// 调用指定类中的方法
    /// </summary>
    /// <param name="className">目标类的完整名称（包括命名空间）</param>
    /// <param name="methodName">要调用的方法名</param>
    /// <param name="parameters">方法参数数组</param>
    /// <returns>调用结果，如果失败返回null</returns>
    public object InvokeMethod(string className, string methodName, params object[] parameters)
    {
        try
        {
            // 1. 获取目标类型
            Type targetType = Type.GetType(className);
            
            if (targetType == null)
            {
                Debug.LogError($"找不到类: {className}");
                return null;
            }

            // 2. 查找方法
            MethodInfo methodInfo = FindMethod(targetType, methodName, parameters);
            
            if (methodInfo == null)
            {
                Debug.LogError($"在类 {className} 中找不到匹配的方法: {methodName}");
                return null;
            }

            // 3. 创建实例（如果是静态方法则不需要）
            object instance = null;
            if (!methodInfo.IsStatic)
            {
                instance = FindOrCreateInstance(targetType);
                if (instance == null)
                {
                    Debug.LogError($"无法创建或获取 {className} 的实例");
                    return null;
                }
            }

            // 4. 调用方法
            return methodInfo.Invoke(instance, parameters);
        }
        catch (Exception e)
        {
            Debug.LogError($"调用方法失败: {e}");
            return null;
        }
    }

    /// <summary>
    /// 查找匹配的方法
    /// </summary>
    private MethodInfo FindMethod(Type targetType, string methodName, object[] parameters)
    {
        // 获取所有同名方法
        MethodInfo[] methods = targetType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | 
                                                    BindingFlags.Instance | BindingFlags.Static);
        
        foreach (MethodInfo method in methods)
        {
            if (method.Name != methodName) continue;

            ParameterInfo[] paramInfos = method.GetParameters();
            
            // 检查参数数量是否匹配
            if (paramInfos.Length != parameters.Length) continue;
            
            bool isMatch = true;
            
            // 检查每个参数类型是否兼容
            for (int i = 0; i < paramInfos.Length; i++)
            {
                if (parameters[i] == null)
                {
                    // 如果参数为null，只允许可空类型或引用类型
                    if (paramInfos[i].ParameterType.IsValueType && 
                        Nullable.GetUnderlyingType(paramInfos[i].ParameterType) == null)
                    {
                        isMatch = false;
                        break;
                    }
                }
                else if (!paramInfos[i].ParameterType.IsAssignableFrom(parameters[i].GetType()))
                {
                    isMatch = false;
                    break;
                }
            }
            
            if (isMatch)
            {
                return method;
            }
        }
        
        return null;
    }

    /// <summary>
    /// 查找或创建目标类型的实例
    /// </summary>
    private object FindOrCreateInstance(Type targetType)
    {
        // 尝试查找现有的单例实例
        // 这里假设管理类可能有Instance静态属性
        PropertyInfo instanceProperty = targetType.GetProperty("Instance", 
            BindingFlags.Public | BindingFlags.Static);
        
        if (instanceProperty != null && instanceProperty.CanRead)
        {
            return instanceProperty.GetValue(null);
        }

        // 尝试通过Unity的FindObjectOfType查找
        if (typeof(UnityEngine.Object).IsAssignableFrom(targetType))
        {
            return UnityEngine.Object.FindObjectOfType(targetType);
        }

        // 最后尝试创建新实例
        try
        {
            return Activator.CreateInstance(targetType);
        }
        catch
        {
            Debug.LogError($"无法创建 {targetType.Name} 的实例，可能需要特定的构造函数参数");
            return null;
        }
    }

    // 示例使用方法,调用其他管理器通知的方法
    public void jiaohu()
    {
        // 示例1：调用静态方法
        InvokeMethod("NPCManager", "targetSpawnNpc", "prefab/npc", "OrcWarrior","-21,-8");
        
        // 示例2：调用实例方法
        // 假设有一个GameManager类，有一个公有方法AddScore(int)
        // InvokeMethod("Namespace.GameManager", "AddScore", 100);
    }
}