// 自动生成的代码，请勿手动修改
using System.Collections.Generic;
using UnityEngine;

public static class GeneratedConfigLoader
{
    public static void LoadAllConfigs(Dictionary<string, Dictionary<string, BaseConfig>> csvData)
    {
        LoadConfig<RoleConfig>(csvData, "RoleConfig");
    }

    private static void LoadConfig<T>(Dictionary<string, Dictionary<string, BaseConfig>> csvData, string configName) where T : BaseConfig
    {
        var textAsset = Resources.Load<TextAsset>($"Configs/{configName}");
        if (textAsset == null)
        {
            Debug.LogWarning($"找不到配置文件: {configName}");
            return;
        }

        var configs = CSVMgr.Load<T>(textAsset);
        var dict = new Dictionary<string, BaseConfig>();

        foreach (var config in configs)
        {
            if (!string.IsNullOrEmpty(config.id))
            {
                dict[config.id] = config;
            }
        }

        csvData[configName] = dict;
    }
}
