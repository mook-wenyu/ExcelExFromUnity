using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Scripting;

namespace ExcelEx
{
    public static class ConfigMgr
    {
        private static Dictionary<string, Dictionary<string, BaseConfig>> _jsonData = new();

        private static readonly JsonSerializerSettings _jsonSerializerSettings = new()
        {
            TypeNameHandling = TypeNameHandling.Auto
        };

        public static void Init()
        {
            var jsonConfigs = Resources.LoadAll<TextAsset>("JsonConfigs");
            foreach (var jsonConfig in jsonConfigs)
            {
                var config = JsonConvert.DeserializeObject<Dictionary<string, BaseConfig>>(jsonConfig.text, _jsonSerializerSettings);
                _jsonData[jsonConfig.name] = config;
            }
        }

        /// <summary>
        /// 获取指定类型和ID的配置数据
        /// </summary>
        /// <typeparam name="T">配置类型</typeparam>
        /// <param name="id">配置ID</param>
        /// <returns>配置数据</returns>
        [Preserve]
        public static T Get<T>(string id) where T : BaseConfig
        {
            var configName = typeof(T).Name;

            if (_jsonData.TryGetValue(configName, out var dict) && dict.TryGetValue(id, out var config))
            {
                return config as T;
            }

            Debug.LogWarning($"未找到类型 {configName} ID为 {id} 的配置数据");
            return null;
        }

        /// <summary>
        /// 获取指定类型的所有配置数据
        /// </summary>
        /// <typeparam name="T">配置类型</typeparam>
        /// <returns>配置数据列表</returns>
        [Preserve]
        public static ReadOnlySpan<T> GetAll<T>() where T : BaseConfig
        {
            var configName = typeof(T).Name;

            if (_jsonData.TryGetValue(configName, out var dict))
            {
                return dict.Values.Cast<T>().ToArray();
            }

            Debug.LogWarning($"未找到类型 {configName} 的配置数据");
            return new ReadOnlySpan<T>();
        }

    }
}
