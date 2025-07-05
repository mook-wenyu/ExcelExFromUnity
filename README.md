# ExcelExFromUnity

## 项目简介

ExcelExFromUnity 是一个 Unity 插件，用于将 Excel 配置文件导出为 JSON 格式，并自动生成对应的 C# 类文件，实现配置数据的快速加载和使用。

## 功能特性

- 支持 Excel (.xlsx/.xls) 文件转换为 JSON 格式
- 自动生成配置类和加载器代码
- 支持多种数据类型：int, long, float, double, bool, string 及其数组
- 支持字符串数组中包含逗号的复杂格式

## 添加到项目

### 手动添加源代码

1. 将以下文件夹和文件添加到您的 Unity 项目中：
   - Assets/Editor
   - Assets/Editor/ExcelExEditor.cs
   - Assets/Editor/ExcelDataReader (依赖文件夹)
   - Assets/Editor/ExcelDataReader/ExcelDataReader
   - Assets/Editor/ExcelDataReader/ExcelDataReader.DataSet

   - Assets/Scripts/BaseConfigs
   - Assets/Scripts/BaseConfigs/BaseConfig.cs
   - Assets/Scripts/BaseConfigs/ConfigMgr.cs

   - Unity包管理器，按名称添加包：com.unity.nuget.newtonsoft-json

## 使用方法

### 配置文件格式要求

Excel 文件需按以下格式组织：

- 第1行：字段注释（用于生成代码注释）
- 第2行：字段名称（第一列必须是 id）
- 第3行：字段类型（支持 int, long, float, double, bool, string 及其数组类型）
- 第4行及以后：具体数据

### 导出配置

1. 将 Excel 配置文件放入项目根目录的 `ExcelConfigs` 文件夹中
2. 在 Unity 编辑器中选择菜单 `Tools > ExcelToJson`
3. 插件会自动将 Excel 文件转换为 JSON 并生成对应的 C# 类

### 在代码中使用

1. 在游戏初始化时调用：

```csharp
ConfigMgr.Init();
```

2. 获取配置数据：

```csharp
// 获取指定 ID 的配置
var config = ConfigMgr.Get<RoleConfig>("10001");

// 获取所有配置
var allConfigs = ConfigMgr.GetAll<RoleConfig>();
```

## 依赖库

- ExcelDataReader 3.7.0
- ExcelDataReader.DataSet 3.7.0
- Newtonsoft.Json
