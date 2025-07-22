# ExcelExFromUnity

## 项目简介

ExcelExFromUnity 是一个 Unity 插件，用于将 Excel 配置文件导出为 JSON 格式，并自动生成对应的 C# 类文件，实现配置数据的快速加载和使用。

## 功能特性

- 支持 Excel (.xlsx/.xls) 文件转换为 JSON 格式
- 自动生成配置类和加载器代码
- 支持多种数据类型：int, long, float, double, bool, string 及其数组
- 支持字符串数组中包含逗号的复杂格式
- 支持 Excel 文件变更的自动监控和重新导出

## 添加到项目

### 安装依赖

1. 打开 Unity 的 Package Manager (菜单: Window > Package Manager)
2. 点击左上角的 "+" 按钮，选择 "Add package by name..."
3. 输入 `com.unity.nuget.newtonsoft-json` 并点击 "Add" 按钮

### 使用包管理器安装

1. 打开 Unity 的 Package Manager (菜单: Window > Package Manager)
2. 点击左上角的 "+" 按钮，选择 "Add package from git URL..."
3. 输入以下URL: `https://github.com/mook-wenyu/ExcelExFromUnity.git?path=Assets/ExcelEx`
4. 点击 "Add" 按钮完成安装

## 使用方法

### 配置导出设置

1. 在 Unity 编辑器中选择菜单 `Tools > Excel Exporter Settings`
2. 设置以下路径：
   - Excel Input Path: Excel 文件夹路径（默认为 "ExcelConfigs"）
   - CS Output Path: 生成 C# 文件的路径（默认为 "Scripts/Configs"）
   - JSON Output Path: 生成 JSON 文件的路径（默认为 "Resources/JsonConfigs"）
3. 点击 "Save" 保存设置

### 配置文件格式要求

Excel 文件需按以下格式组织：

- 第1行：字段注释（用于生成代码注释）
- 第2行：字段名称（第一列必须是 id）
- 第3行：字段类型（支持 int, long, float, double, bool, string 及其数组类型）
- 第4行及以后：具体数据

### 导出配置

1. 将 Excel 配置文件放入项目根目录的 `ExcelConfigs` 文件夹中（或自定义设置的路径）
2. 在 Unity 编辑器中选择菜单 `Tools > Excel To Json`
3. 插件会自动将 Excel 文件转换为 JSON 并生成对应的 C# 类

### 自动监控

插件会自动监控 Excel 文件的变更，当文件发生变化时会自动重新导出配置。

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
