# 移动应用发布脚本
param(
    [string]$Platform = "Android",
    [string]$Configuration = "Release",
    [string]$OutputPath = "publish",
    [switch]$Sign,
    [switch]$All
)

Write-Host "=== 移动应用发布脚本 ===" -ForegroundColor Green

# 创建输出目录
if (-not (Test-Path $OutputPath)) {
    New-Item -ItemType Directory -Path $OutputPath -Force
    Write-Host "已创建输出目录: $OutputPath" -ForegroundColor Cyan
}

# 发布Android应用
if ($Platform -eq "Android" -or $All) {
    Write-Host "正在发布Android应用..." -ForegroundColor Yellow
    
    $androidOutput = Join-Path $OutputPath "android"
    if (-not (Test-Path $androidOutput)) {
        New-Item -ItemType Directory -Path $androidOutput -Force
    }
    
    # 发布APK
    Write-Host "生成APK文件..." -ForegroundColor Cyan
    dotnet publish LocationShareApp.csproj -f net9.0-android -c $Configuration -o $androidOutput
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Android APK生成成功" -ForegroundColor Green
        
        # 查找生成的APK文件
        $apkFiles = Get-ChildItem -Path $androidOutput -Filter "*.apk" -Recurse
        foreach ($apk in $apkFiles) {
            Write-Host "APK文件: $($apk.FullName)" -ForegroundColor Cyan
        }
        
        if ($Sign) {
            Write-Host "注意: APK签名需要配置密钥库" -ForegroundColor Yellow
            Write-Host "请参考Android开发文档进行应用签名" -ForegroundColor Yellow
        }
    } else {
        Write-Host "✗ Android APK生成失败" -ForegroundColor Red
    }
}

# 发布iOS应用
if (($Platform -eq "iOS" -or $All) -and $IsMacOS) {
    Write-Host "正在发布iOS应用..." -ForegroundColor Yellow
    
    $iosOutput = Join-Path $OutputPath "ios"
    if (-not (Test-Path $iosOutput)) {
        New-Item -ItemType Directory -Path $iosOutput -Force
    }
    
    # 发布iOS应用
    Write-Host "生成iOS应用..." -ForegroundColor Cyan
    dotnet publish LocationShareApp.csproj -f net9.0-ios -c $Configuration -o $iosOutput
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ iOS应用生成成功" -ForegroundColor Green
        Write-Host "注意: iOS应用需要通过Xcode进行签名和发布到App Store" -ForegroundColor Yellow
    } else {
        Write-Host "✗ iOS应用生成失败" -ForegroundColor Red
    }
}

# 发布Windows应用
if (($Platform -eq "Windows" -or $All) -and $IsWindows) {
    Write-Host "正在发布Windows应用..." -ForegroundColor Yellow
    
    $windowsOutput = Join-Path $OutputPath "windows"
    if (-not (Test-Path $windowsOutput)) {
        New-Item -ItemType Directory -Path $windowsOutput -Force
    }
    
    # 发布Windows应用
    Write-Host "生成Windows应用..." -ForegroundColor Cyan
    dotnet publish LocationShareApp.csproj -f net9.0-windows10.0.19041.0 -c $Configuration -o $windowsOutput
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Windows应用生成成功" -ForegroundColor Green
    } else {
        Write-Host "✗ Windows应用生成失败" -ForegroundColor Red
    }
}

# 生成发布报告
$reportPath = Join-Path $OutputPath "publish-report.txt"
$report = @"
位置共享应用发布报告
===================
发布时间: $(Get-Date)
配置: $Configuration
平台: $Platform

发布文件:
"@

Get-ChildItem -Path $OutputPath -Recurse -File | ForEach-Object {
    $report += "`n$($_.FullName)"
}

$report | Out-File -FilePath $reportPath -Encoding UTF8
Write-Host "发布报告已保存: $reportPath" -ForegroundColor Cyan

Write-Host "`n=== 发布完成 ===" -ForegroundColor Green
Write-Host "输出目录: $OutputPath" -ForegroundColor Cyan