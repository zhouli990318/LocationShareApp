# 移动应用测试脚本
param(
    [string]$Platform = "Android",
    [string]$Configuration = "Debug",
    [switch]$Clean,
    [switch]$Build,
    [switch]$Deploy,
    [switch]$Test,
    [switch]$All
)

Write-Host "=== 移动应用测试脚本 ===" -ForegroundColor Green

# 如果指定了All参数，则执行所有步骤
if ($All) {
    $Clean = $true
    $Build = $true
    $Deploy = $true
    $Test = $true
}

# 检查.NET MAUI工作负载
function Test-MauiWorkload {
    try {
        $workloads = dotnet workload list
        if ($workloads -match "maui") {
            Write-Host "✓ .NET MAUI工作负载已安装" -ForegroundColor Green
            return $true
        } else {
            Write-Host "✗ .NET MAUI工作负载未安装" -ForegroundColor Red
            Write-Host "请运行: dotnet workload install maui" -ForegroundColor Yellow
            return $false
        }
    }
    catch {
        Write-Host "✗ 无法检查.NET MAUI工作负载" -ForegroundColor Red
        return $false
    }
}

# 检查Android SDK
function Test-AndroidSDK {
    try {
        if ($env:ANDROID_HOME -and (Test-Path $env:ANDROID_HOME)) {
            Write-Host "✓ Android SDK已配置: $env:ANDROID_HOME" -ForegroundColor Green
            return $true
        } else {
            Write-Host "✗ Android SDK未配置" -ForegroundColor Red
            Write-Host "请设置ANDROID_HOME环境变量" -ForegroundColor Yellow
            return $false
        }
    }
    catch {
        Write-Host "✗ 无法检查Android SDK" -ForegroundColor Red
        return $false
    }
}

# 检查iOS开发环境（仅macOS）
function Test-iOSEnvironment {
    if ($Platform -eq "iOS") {
        if ($IsMacOS) {
            try {
                xcode-select --version | Out-Null
                Write-Host "✓ Xcode已安装" -ForegroundColor Green
                return $true
            }
            catch {
                Write-Host "✗ Xcode未安装或未配置" -ForegroundColor Red
                return $false
            }
        } else {
            Write-Host "✗ iOS开发需要macOS环境" -ForegroundColor Red
            return $false
        }
    }
    return $true
}

# 环境检查
Write-Host "正在检查开发环境..." -ForegroundColor Yellow

if (-not (Test-MauiWorkload)) {
    exit 1
}

if ($Platform -eq "Android" -and -not (Test-AndroidSDK)) {
    Write-Host "警告: Android SDK未正确配置，可能影响构建" -ForegroundColor Yellow
}

if (-not (Test-iOSEnvironment)) {
    if ($Platform -eq "iOS") {
        exit 1
    }
}

# 清理项目
if ($Clean) {
    Write-Host "正在清理项目..." -ForegroundColor Yellow
    dotnet clean LocationShareApp.csproj --configuration $Configuration
    
    # 清理平台特定的输出目录
    $cleanPaths = @("bin", "obj")
    foreach ($path in $cleanPaths) {
        if (Test-Path $path) {
            Remove-Item -Recurse -Force $path
            Write-Host "已清理: $path" -ForegroundColor Cyan
        }
    }
    
    Write-Host "项目清理完成" -ForegroundColor Green
}

# 构建应用
if ($Build) {
    Write-Host "正在构建$Platform应用..." -ForegroundColor Yellow
    
    $targetFramework = switch ($Platform) {
        "Android" { "net9.0-android" }
        "iOS" { "net9.0-ios" }
        "Windows" { "net9.0-windows10.0.19041.0" }
        default { "net9.0-android" }
    }
    
    Write-Host "目标框架: $targetFramework" -ForegroundColor Cyan
    
    # 还原NuGet包
    Write-Host "还原NuGet包..." -ForegroundColor Cyan
    dotnet restore LocationShareApp.csproj
    if ($LASTEXITCODE -ne 0) {
        Write-Host "NuGet包还原失败" -ForegroundColor Red
        exit 1
    }
    
    # 构建项目
    Write-Host "构建应用..." -ForegroundColor Cyan
    dotnet build LocationShareApp.csproj -f $targetFramework -c $Configuration
    if ($LASTEXITCODE -ne 0) {
        Write-Host "应用构建失败" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "$Platform应用构建完成" -ForegroundColor Green
}

# 部署应用
if ($Deploy) {
    Write-Host "正在部署$Platform应用..." -ForegroundColor Yellow
    
    $targetFramework = switch ($Platform) {
        "Android" { "net9.0-android" }
        "iOS" { "net9.0-ios" }
        "Windows" { "net9.0-windows10.0.19041.0" }
        default { "net9.0-android" }
    }
    
    if ($Platform -eq "Android") {
        # 检查Android设备或模拟器
        Write-Host "检查Android设备..." -ForegroundColor Cyan
        try {
            $devices = adb devices
            if ($devices -match "device$") {
                Write-Host "✓ 发现Android设备" -ForegroundColor Green
                
                # 部署到设备
                dotnet build LocationShareApp.csproj -f $targetFramework -c $Configuration -t:Run
                if ($LASTEXITCODE -eq 0) {
                    Write-Host "Android应用部署成功" -ForegroundColor Green
                } else {
                    Write-Host "Android应用部署失败" -ForegroundColor Red
                }
            } else {
                Write-Host "✗ 未发现Android设备或模拟器" -ForegroundColor Red
                Write-Host "请连接Android设备或启动模拟器" -ForegroundColor Yellow
            }
        }
        catch {
            Write-Host "✗ 无法检查Android设备（ADB未找到）" -ForegroundColor Red
        }
    }
    elseif ($Platform -eq "iOS") {
        if ($IsMacOS) {
            Write-Host "部署iOS应用..." -ForegroundColor Cyan
            dotnet build LocationShareApp.csproj -f $targetFramework -c $Configuration -t:Run
            if ($LASTEXITCODE -eq 0) {
                Write-Host "iOS应用部署成功" -ForegroundColor Green
            } else {
                Write-Host "iOS应用部署失败" -ForegroundColor Red
            }
        } else {
            Write-Host "✗ iOS部署需要macOS环境" -ForegroundColor Red
        }
    }
}

# 运行测试
if ($Test) {
    Write-Host "正在运行应用测试..." -ForegroundColor Yellow
    
    # 这里可以添加自动化测试
    Write-Host "执行基本功能测试..." -ForegroundColor Cyan
    
    # 检查关键文件是否存在
    $criticalFiles = @(
        "LocationShareApp.csproj",
        "MauiProgram.cs",
        "App.xaml",
        "AppShell.xaml",
        "Views/LoginPage.xaml",
        "Views/MainPage.xaml",
        "Services/ApiService.cs",
        "appsettings.json"
    )
    
    $missingFiles = @()
    foreach ($file in $criticalFiles) {
        if (-not (Test-Path $file)) {
            $missingFiles += $file
        }
    }
    
    if ($missingFiles.Count -eq 0) {
        Write-Host "✓ 所有关键文件存在" -ForegroundColor Green
    } else {
        Write-Host "✗ 缺少关键文件:" -ForegroundColor Red
        foreach ($file in $missingFiles) {
            Write-Host "  - $file" -ForegroundColor Red
        }
    }
    
    # 检查配置文件
    Write-Host "检查配置文件..." -ForegroundColor Cyan
    try {
        $config = Get-Content "appsettings.json" | ConvertFrom-Json
        if ($config.ApiSettings.BaseUrl) {
            Write-Host "✓ API配置正确" -ForegroundColor Green
        } else {
            Write-Host "✗ API配置缺失" -ForegroundColor Red
        }
    }
    catch {
        Write-Host "✗ 配置文件格式错误" -ForegroundColor Red
    }
    
    Write-Host "测试完成" -ForegroundColor Green
}

# 生成测试报告
Write-Host "`n=== 测试报告 ===" -ForegroundColor Green
Write-Host "平台: $Platform" -ForegroundColor Cyan
Write-Host "配置: $Configuration" -ForegroundColor Cyan
Write-Host "时间: $(Get-Date)" -ForegroundColor Cyan

if ($Build -and $LASTEXITCODE -eq 0) {
    Write-Host "✓ 构建成功" -ForegroundColor Green
} elseif ($Build) {
    Write-Host "✗ 构建失败" -ForegroundColor Red
}

Write-Host "=== 测试脚本执行完成 ===" -ForegroundColor Green