using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using UnityEditor;
using UnityEngine;

public class ExcelExEditor
{

    [MenuItem("Tools/生成配置CSV")]
    public static void GenerateConfigs()
    {
        DeleteAllOldFiles();

        var excelDirPath = $"{Application.dataPath}/../Configs";
        if (!Directory.Exists(excelDirPath))
        {
            Debug.LogError("配置文件夹不存在");
            return;
        }

        var excelFiles = Directory.GetFiles(excelDirPath);
        if (excelFiles.Length == 0)
        {
            Debug.LogError("配置文件夹为空");
            return;
        }

        foreach (var excelFile in excelFiles)
        {
            ConvertExcelToCSV(excelFile);
        }

        ConfigCodeGenerator.GenerateConfigLoader();

        AssetDatabase.Refresh();

        EditorApplication.delayCall += () =>
        {
            Debug.Log("完成导出！");
        };
    }

    /// <summary>
    /// 将Excel文件转换为CSV文件
    /// </summary>
    /// <param name="excelFilePath">Excel文件路径</param>
    private static void ConvertExcelToCSV(string excelFilePath)
    {
        try
        {
            string fileName = Path.GetFileNameWithoutExtension(excelFilePath);
            // 读取Excel文件 根据后缀名判断是XLSX还是XLS
            using var stream = new FileStream(excelFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            IWorkbook workbook = excelFilePath.EndsWith(".xlsx")
                ? new XSSFWorkbook(stream)
                : new HSSFWorkbook(stream);

            // 获取第一个工作表
            var sheet = workbook.GetSheetAt(0);
            if (sheet == null || sheet.LastRowNum < 2) return;

            // 构建CSV内容
            var sb = new StringBuilder();
            for (var rowIndex = 0; rowIndex <= sheet.LastRowNum; rowIndex++)
            {
                // 获取当前行
                var row = sheet.GetRow(rowIndex);
                if (row == null) continue;

                // 获取当前行所有单元格的值
                List<string> values = new List<string>();
                for (var cellIndex = 0; cellIndex < row.LastCellNum; cellIndex++)
                {
                    // 获取当前单元格的值，并替换掉逗号和换行符
                    var cell = row.GetCell(cellIndex)?.ToString() ?? "";
                    // 替换逗号和所有类型的换行符
                    cell = cell.Replace("\"", "\"\"")
                        .Replace("\r\n", " ")
                        .Replace("\n", " ")
                        .Replace("\r", " ");
                    // 如果单元格的值包含空格或逗号，则用双引号包裹
                    if (cell.Contains(" ") || cell.Contains(",") || cell.Contains("\"\""))
                    {
                        cell = $"\"{cell}\"";
                    }
                    values.Add(cell);
                }
                // 将当前行的所有单元格的值拼接成一个字符串
                var line = string.Join(",", values);
                sb.AppendLine(line);
            }

            // 将CSV内容写入文件
            var csvFilePath = Path.Combine(Application.dataPath, "Resources", "Configs", $"{fileName}Config.csv");
            File.WriteAllText(csvFilePath, sb.ToString(), Encoding.UTF8);

            // 将CSV文件转换为C#类
            CSV2CSharp(csvFilePath);
        }
        catch (Exception ex)
        {
            Debug.LogError($"转换Excel文件{excelFilePath}失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 将CSV文件转换为C#类
    /// </summary>
    /// <param name="csvFilePath">CSV文件路径</param>
    private static void CSV2CSharp(string csvFilePath)
    {
        var csvContent = File.ReadAllText(csvFilePath, Encoding.UTF8);
        var lines = csvContent.Split('\n').Where(line => !string.IsNullOrWhiteSpace(line)).ToList();
        if (lines.Count < 3) return;

        var fieldComments = lines[0].Split(',');   // 字段注释
        var fields = lines[1].Split(',');   // 字段名
        var types = lines[2].Split(',');    // 字段类型

        if (fields[0] != "id")
        {
            Debug.LogError("CSV文件第2行第1列必须是id");
            return;
        }

        var className = Path.GetFileNameWithoutExtension(csvFilePath);

        var sb = new StringBuilder();

        sb.AppendLine("using System;");

        // 判断是否需要 UnityEngine 引用
        if (types.Any(type => RequiresUnityEngineNamespace(type)))
        {
            sb.AppendLine("using UnityEngine;");
        }

        sb.AppendLine();
        sb.AppendLine($"public class {className} : BaseConfig");
        sb.AppendLine("{");

        for (var i = 0; i < types.Length; i++)
        {
            string type = types[i].Trim();
            string field = fields[i].Trim();
            if (field == "id" && i == 0)
            {
                continue;
            }
            if (i == types.Length - 1)
            {
                field = field.TrimEnd('\r', '\n');
            }
            string comment = fieldComments != null && i < fieldComments.Length ? fieldComments[i].Trim() : null;
            if (!string.IsNullOrEmpty(comment))
            {
                sb.AppendLine($"    /// <summary>");
                sb.AppendLine($"    /// {comment}");
                sb.AppendLine($"    /// </summary>");
            }
            sb.AppendLine($"    public {type} {field};");
        }

        sb.AppendLine("}");

        var classPath = Path.Combine(Application.dataPath, "Scripts", "Configs", $"{className}.cs");
        File.WriteAllText(classPath, sb.ToString(), Encoding.UTF8);
    }

    /// <summary>
    /// 判断是否需要 UnityEngine 命名空间
    /// </summary>
    /// <param name="typeStr">类型字符串</param>
    /// <returns>是否需要 UnityEngine 命名空间</returns>
    private static bool RequiresUnityEngineNamespace(string typeStr)
    {
        return typeStr.ToLower().Trim() is "vector2" or "vector3";
    }

    /// <summary>
    /// 删除所有旧文件
    /// </summary>
    private static void DeleteAllOldFiles()
    {
        var csDir = Path.Combine(Application.dataPath, "Scripts", "Configs");
        var csvDir = Path.Combine(Application.dataPath, "Resources", "Configs");

        if (Directory.Exists(csDir)) Directory.Delete(csDir, true);
        Directory.CreateDirectory(csDir);

        if (Directory.Exists(csvDir)) Directory.Delete(csvDir, true);
        Directory.CreateDirectory(csvDir);

        AssetDatabase.Refresh();
    }


}
