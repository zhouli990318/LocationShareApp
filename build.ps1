# 位置共享应用构建脚本
param(
    [string]$Configuration = "Release",
    [string]$Platform = "Any CPU",
    [switch]$Clean,
    [switch]$Restore,
    [switch]$Build,
    [switch]$Test,
    [switch]$Package,
    [switch]$All
)

Write-Host "=== 位置共享应用构建脚本 ===" -ForegroundColor Green

# 如果指定了All参数，则执行所有步骤
if ($All) {
    $Clean = $true
    $Restore = $true
    $Build = $true
    $Test = $true
    $Package = $true
}

# 清理项目
if ($Clean) {
    Write-Host "正在清理项目..." -ForegroundColor Yellow
    dotnet clean LocationShareApp.sln --configuration $Configuration
    dotnet clean API/LocationShareApp.API.csproj --configuration $Configuration
    
    # 清理输出目录
    if (Test-Path "bin") { Remove-Item -Recurse -Force "bin" }
    if (Test-Path "obj") { Remove-Item -Recurse -Force "obj" }
    if (Test-Path "API/bin") { Remove-Item -Recurse -Force "API/bin" }
    if (Test-Path "API/obj") { Remove-Item -Recurse -Force "API/obj" }
    
    Write-Host "项目清理完成" -ForegroundColor Green
}

# 还原NuGet包
if ($Restore) {
    Write-Host "正在还原NuGet包..." -ForegroundColor Yellow
    dotnet restore LocationShareApp.sln
    if ($LASTEXITCODE -ne 0) {
        Write-Host "NuGet包还原失败" -ForegroundColor Red
        exit 1
    }
    Write-Host "NuGet包还原完成" -ForegroundColor Green
}

# 构建项目
if ($Build) {
    Write-Host "正在构建项目..." -ForegroundColor Yellow
    
    # 构建API项目
    Write-Host "构建API项目..." -ForegroundColor Cyan
    dotnet build API/LocationShareApp.API.csproj --configuration $Configuration --no-restore
    if ($LASTEXITCODE -ne 0) {
        Write-Host "API项目构建失败" -ForegroundColor Red
        exit 1
    }
    
    # 构建MAUI项目
    Write-Host "构建MAUI项目..." -ForegroundColor Cyan
    dotnet build LocationShareApp.csproj --configuration $Configuration --no-restore
    if ($LASTEXITCODE -ne 0) {
        Write-Host "MAUI项目构建失败" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "项目构建完成" -ForegroundColor Green
}

# 运行测试
if ($Test) {
    Write-Host "正在运行测试..." -ForegroundColor Yellow
    
    # 这里可以添加单元测试项目的测试命令
    # dotnet test Tests/LocationShareApp.Tests.csproj --configuration $Configuration --no-build
    
    Write-Host "测试完成" -ForegroundColor Green
}

# 打包应用
if ($Package) {
    Write-Host "正在打包应用..." -ForegroundColor Yellow
    
    # 发布API项目
    Write-Host "发布API项目..." -ForegroundColor Cyan
    dotnet publish API/LocationShareApp.API.csproj --configuration $Configuration --output "publish/api" --no-build
    if ($LASTEXITCODE -ne 0) {
        Write-Host "API项目发布失败" -ForegroundColor Red
        exit 1
    }
    
    # 打包Android应用
    Write-Host "打包Android应用..." -ForegroundColor Cyan
    dotnet publish LocationShareApp.csproj -f net9.0-android --configuration $Configuration --output "publish/android"
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Android应用打包失败" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "应用打包完成" -ForegroundColor Green
    Write-Host "输出目录: publish/" -ForegroundColor Cyan
}

Write-Host "=== 构建脚本执行完成 ===" -ForegroundColor Green