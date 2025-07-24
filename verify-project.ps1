# 项目完整性验证脚本
Write-Host "=== 位置共享应用项目验证 ===" -ForegroundColor Green

$errors = @()
$warnings = @()
$success = @()

# 检查项目结构
Write-Host "检查项目结构..." -ForegroundColor Yellow

$requiredFiles = @{
    "LocationShareApp.csproj" = "MAUI项目文件"
    "LocationShareApp.sln" = "解决方案文件"
    "MauiProgram.cs" = "MAUI程序入口"
    "App.xaml" = "应用程序定义"
    "AppShell.xaml" = "应用程序Shell"
    "API/LocationShareApp.API.csproj" = "API项目文件"
    "API/Program.cs" = "API程序入口"
    "API/appsettings.json" = "API配置文件"
    "docker-compose.yml" = "Docker编排文件"
    "README.md" = "项目说明文档"
}

foreach ($file in $requiredFiles.Keys) {
    if (Test-Path $file) {
        $success += "✓ $($requiredFiles[$file]): $file"
    } else {
        $errors += "✗ 缺少$($requiredFiles[$file]): $file"
    }
}

# 检查关键目录
$requiredDirs = @{
    "Views" = "页面视图目录"
    "ViewModels" = "视图模型目录"
    "Services" = "服务目录"
    "Models" = "数据模型目录"
    "API/Controllers" = "API控制器目录"
    "API/Services" = "API服务目录"
    "API/Models" = "API模型目录"
    "Platforms/Android" = "Android平台目录"
    "Platforms/iOS" = "iOS平台目录"
}

foreach ($dir in $requiredDirs.Keys) {
    if (Test-Path $dir) {
        $success += "✓ $($requiredDirs[$dir]): $dir"
    } else {
        $errors += "✗ 缺少$($requiredDirs[$dir]): $dir"
    }
}

# 检查核心功能文件
Write-Host "检查核心功能文件..." -ForegroundColor Yellow

$coreFiles = @(
    "Views/LoginPage.xaml",
    "Views/RegisterPage.xaml", 
    "Views/MainPage.xaml",
    "Views/UserDetailPage.xaml",
    "Views/UserManagementPage.xaml",
    "ViewModels/LoginViewModel.cs",
    "ViewModels/MainViewModel.cs",
    "Services/ApiService.cs",
    "Services/LocationService.cs",
    "Services/BatteryService.cs",
    "Services/SignalRService.cs",
    "API/Controllers/AuthController.cs",
    "API/Controllers/UserController.cs",
    "API/Controllers/LocationController.cs",
    "API/Controllers/BatteryController.cs",
    "API/Hubs/LocationHub.cs"
)

foreach ($file in $coreFiles) {
    if (Test-Path $file) {
        $success += "✓ 核心文件: $file"
    } else {
        $errors += "✗ 缺少核心文件: $file"
    }
}

# 检查配置文件
Write-Host "检查配置文件..." -ForegroundColor Yellow

try {
    $mauiConfig = Get-Content "appsettings.json" | ConvertFrom-Json
    if ($mauiConfig.ApiSettings.BaseUrl) {
        $success += "✓ MAUI应用配置正确"
    } else {
        $warnings += "⚠ MAUI应用配置可能不完整"
    }
} catch {
    $errors += "✗ MAUI应用配置文件格式错误"
}

try {
    $apiConfig = Get-Content "API/appsettings.json" | ConvertFrom-Json
    if ($apiConfig.ConnectionStrings.DefaultConnection) {
        $success += "✓ API配置正确"
    } else {
        $warnings += "⚠ API配置可能不完整"
    }
} catch {
    $errors += "✗ API配置文件格式错误"
}

# 检查NuGet包配置
Write-Host "检查NuGet包配置..." -ForegroundColor Yellow

try {
    $mauiProject = [xml](Get-Content "LocationShareApp.csproj")
    $packages = $mauiProject.Project.ItemGroup.PackageReference
    
    $requiredPackages = @(
        "Microsoft.Maui.Controls",
        "Microsoft.Maui.Controls.Maps",
        "Microsoft.AspNetCore.SignalR.Client"
    )
    
    foreach ($pkg in $requiredPackages) {
        if ($packages | Where-Object { $_.Include -eq $pkg }) {
            $success += "✓ MAUI包: $pkg"
        } else {
            $warnings += "⚠ 可能缺少MAUI包: $pkg"
        }
    }
} catch {
    $warnings += "⚠ 无法检查MAUI项目包配置"
}

try {
    $apiProject = [xml](Get-Content "API/LocationShareApp.API.csproj")
    $packages = $apiProject.Project.ItemGroup.PackageReference
    
    $requiredApiPackages = @(
        "Microsoft.EntityFrameworkCore",
        "Npgsql.EntityFrameworkCore.PostgreSQL"
    )
    
    foreach ($pkg in $requiredApiPackages) {
        if ($packages | Where-Object { $_.Include -eq $pkg }) {
            $success += "✓ API包: $pkg"
        } else {
            $warnings += "⚠ 可能缺少API包: $pkg"
        }
    }
} catch {
    $warnings += "⚠ 无法检查API项目包配置"
}

# 检查部署文件
Write-Host "检查部署文件..." -ForegroundColor Yellow

$deployFiles = @(
    "API/Dockerfile",
    "docker-compose.yml",
    "nginx.conf",
    "build.ps1",
    "deploy.ps1",
    "test-mobile.ps1"
)

foreach ($file in $deployFiles) {
    if (Test-Path $file) {
        $success += "✓ 部署文件: $file"
    } else {
        $warnings += "⚠ 缺少部署文件: $file"
    }
}

# 生成验证报告
Write-Host "`n=== 验证报告 ===" -ForegroundColor Green

Write-Host "`n成功项目 ($($success.Count)):" -ForegroundColor Green
foreach ($item in $success) {
    Write-Host $item -ForegroundColor Green
}

if ($warnings.Count -gt 0) {
    Write-Host "`n警告项目 ($($warnings.Count)):" -ForegroundColor Yellow
    foreach ($item in $warnings) {
        Write-Host $item -ForegroundColor Yellow
    }
}

if ($errors.Count -gt 0) {
    Write-Host "`n错误项目 ($($errors.Count)):" -ForegroundColor Red
    foreach ($item in $errors) {
        Write-Host $item -ForegroundColor Red
    }
}

# 总结
Write-Host "`n=== 验证总结 ===" -ForegroundColor Green
Write-Host "成功: $($success.Count)" -ForegroundColor Green
Write-Host "警告: $($warnings.Count)" -ForegroundColor Yellow
Write-Host "错误: $($errors.Count)" -ForegroundColor Red

if ($errors.Count -eq 0) {
    Write-Host "`n🎉 项目验证通过！应用已准备就绪。" -ForegroundColor Green
    Write-Host "可以使用以下命令启动应用:" -ForegroundColor Cyan
    Write-Host "  后端API: .\deploy.ps1 -All" -ForegroundColor Cyan
    Write-Host "  移动应用: .\test-mobile.ps1 -All" -ForegroundColor Cyan
} else {
    Write-Host "`n❌ 项目验证失败，请修复上述错误后重试。" -ForegroundColor Red
}

Write-Host "`n=== 验证完成 ===" -ForegroundColor Green