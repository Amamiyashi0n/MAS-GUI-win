# MAS GUI

一个基于 WPF 的 Microsoft Activation Scripts (MAS) 图形化界面工具，提供简洁美观的 Material Design 风格界面，用于 Windows 和 Office 的激活管理。

## 应用截图

<img width="513" height="883" alt="image" src="https://github.com/user-attachments/assets/15d342cf-2164-4d3a-9fa7-83c5c73f42de" />

## 功能特性

- **一键激活**：自动检测 Windows 和已安装 Office 套件并激活
- **多种激活方式**：
  - Windows: HWID、TSforge、TSforge ESU、KMS
  - Office: Ohook
- **卸载功能**：支持卸载已应用的激活
- **Material Design UI**：现代化的用户界面设计
- **实时日志**：记录所有操作日志
- **进度显示**：清晰的操作进度和完成状态反馈

## 系统要求

- Windows 操作系统
- .NET Framework 3.5 或更高版本

## 项目结构

```
MAS_GUI/
├── Controls/              # 自定义控件
│   ├── MaterialButton.cs
│   ├── MaterialCard.cs
│   ├── MaterialIcon.cs
│   ├── MaterialProgressBar.cs
│   ├── MaterialScrollBar.cs
│   ├── MaterialTextBox.cs
│   ├── MaterialToggle.cs
│   ├── SayingWidget.xaml
│   └── SayingWidget.xaml.cs
├── Util/                  # 工具类
│   ├── ScriptRunner.cs    # 脚本运行器
│   └── MAS_AIO.cmd        # 嵌入的激活脚本
├── Res/                   # 资源文件
│   └── miku.jpg
├── Properties/            # 项目属性
├── App.xaml               # 应用程序入口
├── MainWindow.xaml        # 主窗口界面
└── MainWindow.xaml.cs     # 主窗口逻辑
```

## 构建说明

### 前置要求

- Visual Studio 2017 或更高版本
- .NET Framework 3.5 SDK

### 构建步骤

1. 克隆或下载此仓库
2. 使用 Visual Studio 打开 `MAS_gui.sln` 解决方案文件
3. 选择 Release 或 Debug 配置
4. 点击"生成" -> "生成解决方案"或按 `Ctrl+Shift+B`

构建完成后，可执行文件将位于：
- Debug: `bin\Debug\MAS_GUI.exe`
- Release: `bin\Release\MAS_GUI.exe`

## 使用说明

### 基本操作

1. **一键激活**：点击主界面的"一键激活"按钮，程序将自动检测并激活 Windows 和 Office
2. **高级功能**：点击"高级"按钮进入高级模式，可选择特定的激活方式
3. **Windows 激活**：
   - HWID：数字权利激活
   - TSforge 激活：使用 TSforge 方法激活
   - TSforge ESU：针对扩展安全更新的激活
   - 卸载：移除 Windows 激活
4. **Office 激活**：
   - Ohook：使用 Ohook 方法激活
   - 卸载：移除 Office 激活
5. **在线 KMS**：
   - KMS 激活：使用在线 KMS 服务器激活
   - 卸载：清除 KMS 缓存

### 日志文件

操作日志将保存在应用程序目录下的 `mas_gui.log` 文件中，便于排查问题。

## 技术栈

- **框架**: WPF (Windows Presentation Foundation)
- **语言**: C#
- **目标框架**: .NET Framework 3.5
- **UI 设计**: Material Design 风格

## 自定义控件

项目包含一系列 Material Design 风格的自定义控件：

- `MaterialButton`: 按钮控件（包含多种样式）
- `MaterialCard`: 卡片容器
- `MaterialIcon`: 图标控件
- `MaterialProgressBar`: 进度条
- `MaterialScrollBar`: 滚动条
- `MaterialTextBox`: 文本输入框
- `MaterialToggle`: 开关控件
- `SayingWidget`: 动态语录组件

## 注意事项

- 本工具仅供学习和研究使用
- 请遵守相关法律法规和软件许可协议
- 使用前请备份重要数据
- 激活过程可能需要管理员权限

## 项目地址

https://github.com/Amamiyashi0n/MAS-GUI

## 许可证

本项目遵循原 MAS 项目的许可证。

## 致谢

- 基于 [Microsoft Activation Scripts](https://github.com/massgravel/Microsoft-Activation-Scripts) 项目
- Material Design 设计规范
