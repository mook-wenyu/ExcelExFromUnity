using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

public static class CSVMgr
{
    private static Dictionary<string, Dictionary<string, BaseConfig>> _csvData = new();

    public static void Init()
    {
        var csvFiles = Resources.LoadAll<TextAsset>("Configs");
        foreach (var csvFile in csvFiles)
        {
            // 通过反射获取类型
            Type configType = GetConfigTypeByFileName(csvFile.name);
            if (configType == null)
            {
                Debug.LogError($"找不到对应的配置类: {csvFile.name}");
                continue;
            }

            try
            {
                // 使用反射调用泛型方法
                var method = typeof(CSVMgr).GetMethod("Load", BindingFlags.Public | BindingFlags.Static);
                var genericMethod = method.MakeGenericMethod(configType);

                // 获取返回值并转换为对应的Dictionary<string, BaseConfig>
                var result = genericMethod.Invoke(null, new object[] { csvFile });

                if (result == null)
                {
                    Debug.LogError($"加载配置失败，返回值为null: {csvFile.name}");
                    continue;
                }

                // 创建目标字典
                var datas = new Dictionary<string, BaseConfig>();

                // 获取泛型字典类型的GetEnumerator方法
                var resultType = result.GetType();
                var enumerator = resultType.GetMethod("GetEnumerator").Invoke(result, null);
                var enumeratorType = enumerator.GetType();
                var moveNextMethod = enumeratorType.GetMethod("MoveNext");
                var currentProperty = enumeratorType.GetProperty("Current");

                // 遍历源字典的所有键值对
                while ((bool)moveNextMethod.Invoke(enumerator, null))
                {
                    var current = currentProperty.GetValue(enumerator);
                    var keyProperty = current.GetType().GetProperty("Key");
                    var valueProperty = current.GetType().GetProperty("Value");

                    string key = (string)keyProperty.GetValue(current);
                    BaseConfig value = (BaseConfig)valueProperty.GetValue(current);

                    datas[key] = value;
                }

                _csvData[csvFile.name] = datas;
            }
            catch (Exception ex)
            {
                Debug.LogError($"加载配置时发生异常: {csvFile.name}, 错误: {ex.Message}\n{ex.StackTrace}");
            }
        }

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
    /// 根据文件名获取对应的配置类型
    /// </summary>
    /// <param name="fileName">文件名（不含扩展名）</param>
    /// <returns>对应的配置类型</returns>
    public static Type GetConfigTypeByFileName(string fileName)
    {
        // 获取所有程序集
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (var assembly in assemblies)
        {
            // 查找与文件名匹配的类型
            Type type = assembly.GetTypes()
                .FirstOrDefault(t => t.IsClass &&
                                    !t.IsAbstract &&
                                    t.IsSubclassOf(typeof(BaseConfig)) &&
                                    string.Equals(t.Name, fileName, StringComparison.OrdinalIgnoreCase));

            if (type != null)
            {
                return type;
            }
        }

        Debug.LogWarning($"未找到与 {fileName} 对应的配置类");
        return null;
    }

    /// <summary>
    /// 加载CSV文件并转换为配置对象字典
    /// </summary>
    /// <typeparam name="T">配置类型</typeparam>
    /// <param name="csvFile">CSV文件</param>
    /// <returns>以ID为键的配置对象字典</returns>
    public static Dictionary<string, T> Load<T>(TextAsset csvFile) where T : BaseConfig
    {
        var lines = csvFile.text.Split('\n').Skip(3);
        var fields = csvFile.text.Split('\n')[1].Split(',');
        var types = csvFile.text.Split('\n')[2].Split(',');
        var datas = new Dictionary<string, T>();

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            var rawValues = SplitCSVLine(line);
            var obj = Activator.CreateInstance<T>();

            for (int i = 0; i < fields.Length && i < rawValues.Length; i++)
            {
                string val = rawValues[i];
                string type = types[i];
                string field = fields[i];
                SetValue(obj, field, val, type);
            }

            if (!string.IsNullOrEmpty(obj.id))
            {
                datas[obj.id] = obj;
            }
            else
            {
                Debug.LogWarning($"配置对象ID为空，无法添加到字典中: {typeof(T).Name}");
            }
        }

        return datas;
    }

    /// <summary>
    /// 分割CSV行
    /// </summary>
    /// <param name="line">CSV行</param>
    /// <returns>分割后的字符串数组</returns>
    private static string[] SplitCSVLine(string line)
    {
        var list = new List<string>();
        var sb = new StringBuilder();
        bool inQuote = false;

        foreach (char c in line)
        {
            if (c == '"')
            {
                inQuote = !inQuote;
            }
            else if (c == ',' && !inQuote)
            {
                list.Add(sb.ToString());
                sb.Clear();
            }
            else
            {
                sb.Append(c);
            }
        }

        list.Add(sb.ToString());
        return list.ToArray();
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
            string[] elements = val.Replace('，', ',').Split(',');

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
        var parts = s.Replace('，', ',').Split(',');

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
        var parts = s.Replace('，', ',').Split(',');

        if (parts.Length != 2) return Vector2.zero;

        bool xValid = float.TryParse(parts[0], out float x);
        bool yValid = float.TryParse(parts[1], out float y);
        return (xValid && yValid) ? new Vector2(x, y) : Vector2.zero;
    }
}
