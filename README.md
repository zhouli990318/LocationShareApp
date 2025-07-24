# 家庭位置共享移动应用

一个基于.NET MAUI开发的跨平台移动应用，支持家庭成员之间的实时位置共享、电量监控和设备信息查看。

## 功能特性

- 📱 **跨平台支持**: 支持Android和iOS平台
- 🔐 **安全认证**: 手机号+密码注册登录，JWT身份验证
- 📍 **实时位置共享**: 基于地图的实时位置显示和历史轨迹
- 🔋 **电量监控**: 24小时电量变化图表，充电状态监控
- 👥 **用户管理**: 通过绑定码添加和管理关联用户
- 🌐 **实时通信**: 基于SignalR的实时数据推送
- 📊 **设备信息**: 网络状态、设备型号等信息展示
- 💾 **离线支持**: 数据缓存和离线同步功能

## 技术架构

### 移动端
- **.NET MAUI**: 跨平台移动应用开发框架
- **C# + XAML**: 主要开发语言和UI标记
- **Microsoft.Maui.Maps**: 地图服务集成
- **Microsoft.Maui.Essentials**: 设备功能访问
- **SignalR Client**: 实时通信客户端

### 后端服务
- **ASP.NET Core Web API**: RESTful API服务
- **Entity Framework Core**: 数据访问层
- **PostgreSQL**: 数据库
- **SignalR**: 实时数据推送
- **JWT Authentication**: 身份验证

## 项目结构

```
LocationShareApp/
├── API/                          # 后端API项目
│   ├── Controllers/              # API控制器
│   ├── Data/                     # 数据访问层
│   ├── Hubs/                     # SignalR Hub
│   ├── Models/                   # 数据模型
│   └── Services/                 # 业务服务
├── Controls/                     # 自定义控件
├── Helpers/                      # 辅助类
├── Models/                       # 客户端数据模型
├── Platforms/                    # 平台特定代码
│   ├── Android/                  # Android平台
│   └── iOS/                      # iOS平台
├── Services/                     # 客户端服务
├── ViewModels/                   # 视图模型
├── Views/                        # 页面视图
└── Resources/                    # 资源文件
```

## 开发环境要求

- **Visual Studio 2022** 或 **Visual Studio Code**
- **.NET 9.0 SDK**
- **Android SDK** (用于Android开发)
- **Xcode** (用于iOS开发，仅macOS)
- **PostgreSQL** 数据库

## 快速开始

### 1. 克隆项目
```bash
git clone <repository-url>
cd LocationShareApp
```

### 2. 配置数据库
1. 安装PostgreSQL数据库
2. 修改 `API/appsettings.json` 中的数据库连接字符串
3. 运行数据库迁移：
```bash
cd API
dotnet ef database update
```

### 3. 构建项目
使用PowerShell脚本构建：
```powershell
.\build.ps1 -All
```

或手动构建：
```bash
# 还原NuGet包
dotnet restore

# 构建API项目
dotnet build API/LocationShareApp.API.csproj

# 构建MAUI项目
dotnet build LocationShareApp.csproj
```

### 4. 运行应用

#### 启动后端API
```bash
cd API
dotnet run
```
API将在 `https://localhost:7001` 启动

#### 运行移动应用
```bash
# Android
dotnet build -t:Run -f net9.0-android

# iOS (仅macOS)
dotnet build -t:Run -f net9.0-ios
```

## 配置说明

### API配置 (API/appsettings.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=LocationShareDB;Username=postgres;Password=your_password"
  },
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "LocationShareApp",
    "Audience": "LocationShareApp",
    "ExpiryInHours": 24
  }
}
```

### 移动应用配置 (appsettings.json)
```json
{
  "ApiSettings": {
    "BaseUrl": "https://localhost:7001/api",
    "SignalRHubUrl": "https://localhost:7001/locationHub"
  }
}
```

## 权限配置

### Android权限 (Platforms/Android/AndroidManifest.xml)
- `ACCESS_FINE_LOCATION`: 精确位置访问
- `ACCESS_COARSE_LOCATION`: 粗略位置访问
- `ACCESS_BACKGROUND_LOCATION`: 后台位置访问
- `INTERNET`: 网络访问

### iOS权限 (Platforms/iOS/Info.plist)
- `NSLocationWhenInUseUsageDescription`: 使用时位置访问
- `NSLocationAlwaysAndWhenInUseUsageDescription`: 始终位置访问
- `UIBackgroundModes`: 后台模式配置

## 部署说明

### 后端API部署
1. 发布API项目：
```bash
dotnet publish API/LocationShareApp.API.csproj -c Release -o publish/api
```

2. 配置生产环境数据库连接
3. 部署到云服务器或容器

### 移动应用发布
1. Android APK：
```bash
dotnet publish -f net9.0-android -c Release
```

2. iOS应用需要通过Xcode进行签名和发布

## 故障排除

### 常见问题

1. **数据库连接失败**
   - 检查PostgreSQL服务是否运行
   - 验证连接字符串配置
   - 确认数据库用户权限

2. **位置权限被拒绝**
   - 检查应用权限设置
   - 确认位置服务已启用
   - 重新安装应用重新授权

3. **SignalR连接失败**
   - 检查网络连接
   - 验证API服务是否运行
   - 确认防火墙设置

4. **构建失败**
   - 清理项目：`dotnet clean`
   - 还原包：`dotnet restore`
   - 检查.NET SDK版本

## 贡献指南

1. Fork项目
2. 创建功能分支
3. 提交更改
4. 推送到分支
5. 创建Pull Request

## 许可证

本项目采用MIT许可证 - 查看 [LICENSE](LICENSE) 文件了解详情。

## 联系方式

如有问题或建议，请通过以下方式联系：
- 邮箱: your-email@example.com
- 项目Issues: [GitHub Issues](https://github.com/your-repo/issues)