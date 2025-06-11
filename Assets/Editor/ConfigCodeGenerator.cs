using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class ConfigCodeGenerator
{
    /// <summary>
    /// 生成配置加载器
    /// </summary>
    public static void GenerateConfigLoader()
    {
        // 扫描所有配置类
        var configTypes = ReflectionHelper.GetAllConfigTypes();

        StringBuilder sb = new StringBuilder();
        // 生成文件头
        sb.AppendLine("// 自动生成的代码，请勿手动修改");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using UnityEngine;");
        sb.AppendLine();
        sb.AppendLine("public static class GeneratedConfigLoader");
        sb.AppendLine("{");

        // 生成加载方法
        sb.AppendLine("    public static void LoadAllConfigs(Dictionary<string, Dictionary<string, BaseConfig>> csvData)");
        sb.AppendLine("    {");

        foreach (var type in configTypes)
        {
            string typeName = type.Name;
            sb.AppendLine($"        LoadConfig<{typeName}>(csvData, \"{typeName}\");");
        }

        sb.AppendLine("    }");
        sb.AppendLine();

        // 生成泛型加载方法
        sb.AppendLine("    private static void LoadConfig<T>(Dictionary<string, Dictionary<string, BaseConfig>> csvData, string configName) where T : BaseConfig");
        sb.AppendLine("    {");
        sb.AppendLine("        var textAsset = Resources.Load<TextAsset>($\"Configs/{configName}\");");
        sb.AppendLine("        if (textAsset == null)");
        sb.AppendLine("        {");
        sb.AppendLine("            Debug.LogWarning($\"找不到配置文件: {configName}\");");
        sb.AppendLine("            return;");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        var configs = CSVMgr.Load<T>(textAsset);");
        sb.AppendLine("        var dict = new Dictionary<string, BaseConfig>();");
        sb.AppendLine();
        sb.AppendLine("        foreach (var config in configs)");
        sb.AppendLine("        {");
        sb.AppendLine("            if (!string.IsNullOrEmpty(config.id))");
        sb.AppendLine("            {");
        sb.AppendLine("                dict[config.id] = config;");
        sb.AppendLine("            }");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        csvData[configName] = dict;");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        // 写入文件
        string filePath = Path.Combine(Application.dataPath, "Scripts", "BaseConfigs", "GeneratedConfigLoader.cs");
        File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
    }
}

public static class ReflectionHelper
{
    /// <summary>
    /// 获取所有配置类
    /// </summary>
    /// <returns>配置类列表</returns>
    public static List<Type> GetAllConfigTypes()
    {
        var types = new List<Type>();
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (var assembly in assemblies)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(BaseConfig)))
                {
                    types.Add(type);
                }
            }
        }

        return types;
    }
}
