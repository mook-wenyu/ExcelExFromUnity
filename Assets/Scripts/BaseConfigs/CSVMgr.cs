using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class CSVMgr
{
    private static Dictionary<string, Dictionary<string, BaseConfig>> _csvData = new();

    public static void Init()
    {
        GeneratedConfigLoader.LoadAllConfigs(_csvData);

        Debug.Log($"成功加载配置: {_csvData.Count} 个配置文件");
    }

    /// <summary>
    /// 获取指定类型的所有配置数据
    /// </summary>
    /// <typeparam name="T">配置类型</typeparam>
    /// <returns>配置数据列表</returns>
    public static List<T> GetAll<T>() where T : BaseConfig
    {
        string configName = typeof(T).Name;

        if (_csvData.TryGetValue(configName, out var dict))
        {
            return dict.Values.Cast<T>().ToList();
        }

        Debug.LogWarning($"未找到类型 {configName} 的配置数据");
        return new List<T>();
    }

    /// <summary>
    /// 获取指定类型和ID的配置数据
    /// </summary>
    /// <typeparam name="T">配置类型</typeparam>
    /// <param name="id">配置ID</param>
    /// <returns>配置数据</returns>
    public static T Get<T>(string id) where T : BaseConfig
    {
        string configName = typeof(T).Name;

        if (_csvData.TryGetValue(configName, out var dict) && dict.TryGetValue(id, out var config))
        {
            return config as T;
        }

        Debug.LogWarning($"未找到类型 {configName} ID为 {id} 的配置数据");
        return null;
    }

    /// <summary>
    /// 加载CSV文件并转换为配置对象列表
    /// </summary>
    /// <typeparam name="T">配置类型</typeparam>
    /// <param name="csvFile">CSV文件</param>
    /// <returns>配置对象列表</returns>
    public static List<T> Load<T>(TextAsset csvFile) where T : BaseConfig
    {
        var lines = csvFile.text.Split('\n');
        if (lines.Length <= 3)
        {
            Debug.LogWarning($"CSV文件格式不正确或为空: {csvFile.name}");
            return new List<T>();
        }

        var dataLines = lines.Skip(3).Where(line => !string.IsNullOrWhiteSpace(line)).ToArray();
        var fields = lines[1].Split(',');
        var types = lines[2].Split(',');

        // 创建结果数组
        var results = new T[dataLines.Length];

        // 并行处理每一行
        Parallel.For(0, dataLines.Length, i =>
        {
            results[i] = ParseLine<T>(dataLines[i], fields, types);
        });

        // 收集结果
        var datas = new List<T>(results.Length);
        foreach (var obj in results)
        {
            if (obj != null)
            {
                if (string.IsNullOrEmpty(obj.id))
                {
                    Debug.LogWarning($"配置对象ID为空: {typeof(T).Name}");
                }
                datas.Add(obj);
            }
        }

        return datas;
    }

    /// <summary>
    /// 解析单行CSV数据
    /// </summary>
    /// <typeparam name="T">配置类型</typeparam>
    /// <param name="line">CSV行</param>
    /// <param name="fields">字段名数组</param>
    /// <param name="types">类型名数组</param>
    /// <returns>解析后的配置对象</returns>
    private static T ParseLine<T>(string line, string[] fields, string[] types) where T : BaseConfig
    {
        try
        {
            var rawValues = SplitCSVLine(line);
            var obj = Activator.CreateInstance<T>();

            for (int i = 0; i < fields.Length && i < rawValues.Count; i++)
            {
                string val = rawValues[i];
                string type = types[i];
                string field = fields[i];
                SetValue(obj, field, val, type);
            }

            return obj;
        }
        catch (Exception ex)
        {
            Debug.LogError($"解析CSV行时发生错误: {ex}\n行内容: {line}");
            return null;
        }
    }

    /// <summary>
    /// 分割 CSV 行，正确处理引号
    /// </summary>
    /// <param name="line">CSV行</param>
    /// <returns>分割后的字符串列表</returns>
    private static List<string> SplitCSVLine(string line)
    {
        var result = new List<string>();
        var sb = new StringBuilder();
        bool inQuote = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            // 处理双引号（转义的引号）
            if (c == '"')
            {
                if (inQuote)
                {
                    // 检查是否是转义的双引号 ("")
                    if (i + 1 < line.Length && line[i + 1] == '"')
                    {
                        sb.Append('"');
                        i++; // 跳过下一个引号
                    }
                    else
                    {
                        // 引号结束
                        inQuote = false;
                    }
                }
                else
                {
                    // 引号开始
                    inQuote = true;
                }
                continue;
            }

            // 处理逗号分隔符
            if (c == ',' && !inQuote)
            {
                // 添加当前字段并重置StringBuilder
                result.Add(sb.ToString());
                sb.Clear();
                continue;
            }

            // 正常字符，直接添加
            sb.Append(c);
        }

        // 添加最后一个字段
        result.Add(sb.ToString());

        return result;
    }

    /// <summary>
    /// 设置字段值
    /// </summary>
    /// <typeparam name="T">配置类型</typeparam>
    /// <param name="obj">配置对象</param>
    /// <param name="field">字段名</param>
    private static void SetValue<T>(T obj, string field, string val, string type)
    {
        string fieldName = field.Trim();
        var fi = typeof(T).GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);
        if (fi == null)
        {
            Debug.LogWarning($"字段未找到: '{field}'（Trim后: '{fieldName}'） in {typeof(T)}");
            return;
        }

        val = val.Trim().Trim('"');
        type = type.Trim().ToLower();

        // 检查是否是数组类型
        bool isArray = type.EndsWith("[]");
        if (isArray)
        {
            // 获取数组元素类型
            string elementType = type.Substring(0, type.Length - 2).Trim();

            // 如果值为空，则设置为空数组
            if (string.IsNullOrWhiteSpace(val))
            {
                // 根据元素类型创建空数组
                object emptyArray = CreateEmptyArray(elementType);
                fi.SetValue(obj, emptyArray);
                return;
            }

            // 分割值字符串，处理可能的中文逗号
            string[] elements = val.Split(',');

            // 根据元素类型解析数组
            object arrayValue = ParseArray(elements, elementType);
            fi.SetValue(obj, arrayValue);
            return;
        }

        // 非数组类型的处理
        object parsed = type switch
        {
            "int" => int.TryParse(val, out var i) ? i : 0,
            "long" => long.TryParse(val, out var l) ? l : 0L,
            "float" => float.TryParse(val, out var f) ? f : 0f,
            "double" => double.TryParse(val, out var d) ? d : 0d,
            "bool" => bool.TryParse(val, out var b) ? b : false,
            "vector2" => ParseVector2(val),
            "vector3" => ParseVector3(val),
            _ => val
        };
        fi.SetValue(obj, parsed);
    }

    /// <summary>
    /// 创建指定元素类型的空数组
    /// </summary>
    /// <param name="elementType">元素类型</param>
    /// <returns>空数组</returns>
    private static object CreateEmptyArray(string elementType)
    {
        return elementType switch
        {
            "int" => new int[0],
            "long" => new long[0],
            "float" => new float[0],
            "double" => new double[0],
            "bool" => new bool[0],
            "string" => new string[0],
            _ => new string[0] // 默认为字符串数组
        };
    }

    /// <summary>
    /// 解析字符串数组为指定类型的数组
    /// </summary>
    /// <param name="elements">字符串元素</param>
    /// <param name="elementType">元素类型</param>
    /// <returns>解析后的数组</returns>
    private static object ParseArray(string[] elements, string elementType)
    {
        switch (elementType)
        {
            case "int":
                return elements.Select(e => int.TryParse(e.Trim(), out var i) ? i : 0).ToArray();

            case "long":
                return elements.Select(e => long.TryParse(e.Trim(), out var l) ? l : 0L).ToArray();

            case "float":
                return elements.Select(e => float.TryParse(e.Trim(), out var f) ? f : 0f).ToArray();

            case "double":
                return elements.Select(e => double.TryParse(e.Trim(), out var d) ? d : 0d).ToArray();

            case "bool":
                return elements.Select(e => bool.TryParse(e.Trim(), out var b) ? b : false).ToArray();

            case "string":
            default:
                return elements.Select(e => e.Trim()).ToArray();
        }
    }

    /// <summary>
    /// 解析字符串为Vector3
    /// </summary>
    /// <param name="s">字符串</param>
    /// <returns>解析后的Vector3</returns>
    private static Vector3 ParseVector3(string s)
    {
        s = s.Trim().Trim('"'); // 去除空格和引号
        // 支持中文逗号和英文逗号
        var parts = s.Split(',');

        if (parts.Length != 3) return Vector3.zero;

        bool xValid = float.TryParse(parts[0], out float x);
        bool yValid = float.TryParse(parts[1], out float y);
        bool zValid = float.TryParse(parts[2], out float z);

        return (xValid && yValid && zValid) ? new Vector3(x, y, z) : Vector3.zero;
    }

    /// <summary>
    /// 解析字符串为Vector2
    /// </summary>
    /// <param name="s">字符串</param>
    /// <returns>解析后的Vector2</returns>
    private static Vector2 ParseVector2(string s)
    {
        s = s.Trim().Trim('"'); // 去除空格和引号
        // 支持中文逗号和英文逗号
        var parts = s.Split(',');

        if (parts.Length != 2) return Vector2.zero;

        bool xValid = float.TryParse(parts[0], out float x);
        bool yValid = float.TryParse(parts[1], out float y);
        return (xValid && yValid) ? new Vector2(x, y) : Vector2.zero;
    }
}
