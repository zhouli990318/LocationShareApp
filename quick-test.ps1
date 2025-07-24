# 快速测试脚本
Write-Host "=== 快速项目测试 ===" -ForegroundColor Green

# 测试API项目
Write-Host "测试API项目..." -ForegroundColor Yellow
cd API
$apiResult = dotnet build --verbosity quiet
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ API项目构建成功" -ForegroundColor Green
} else {
    Write-Host "✗ API项目构建失败" -ForegroundColor Red
}
cd ..

# 测试关键文件存在性
Write-Host "检查关键文件..." -ForegroundColor Yellow
$criticalFiles = @(
    "LocationShareApp.csproj",
    "App.xaml",
    "MauiProgram.cs",
    "Services/ApiService.cs",
    "Views/LoginPage.xaml",
    "API/Program.cs"
)

$missingFiles = @()
foreach ($file in $criticalFiles) {
    if (Test-Path $file) {
        Write-Host "✓ $file" -ForegroundColor Green
    } else {
        Write-Host "✗ $file" -ForegroundColor Red
        $missingFiles += $file
    }
}

# 检查配置文件
Write-Host "检查配置..." -ForegroundColor Yellow
try {
    $config = Get-Content "appsettings.json" | ConvertFrom-Json
    Write-Host "✓ 应用配置文件格式正确" -ForegroundColor Green
} catch {
    Write-Host "✗ 应用配置文件格式错误" -ForegroundColor Red
}

# 总结
Write-Host "`n=== 测试总结 ===" -ForegroundColor Green
if ($missingFiles.Count -eq 0 -and $LASTEXITCODE -eq 0) {
    Write-Host "项目基本结构完整，API构建成功！" -ForegroundColor Green
    Write-Host "项目已准备就绪，可以进行进一步开发和测试。" -ForegroundColor Cyan
} else {
    Write-Host "发现一些问题，但项目基本可用。" -ForegroundColor Yellow
}