# 位置共享应用部署脚本
param(
    [string]$Environment = "Production",
    [switch]$Build,
    [switch]$Deploy,
    [switch]$Start,
    [switch]$Stop,
    [switch]$Restart,
    [switch]$Logs,
    [switch]$Status,
    [switch]$All
)

Write-Host "=== 位置共享应用部署脚本 ===" -ForegroundColor Green

# 如果指定了All参数，则执行构建和部署
if ($All) {
    $Build = $true
    $Deploy = $true
    $Start = $true
}

# 检查Docker是否安装
function Test-Docker {
    try {
        docker --version | Out-Null
        return $true
    }
    catch {
        Write-Host "错误: Docker未安装或未运行" -ForegroundColor Red
        return $false
    }
}

# 检查Docker Compose是否安装
function Test-DockerCompose {
    try {
        docker-compose --version | Out-Null
        return $true
    }
    catch {
        Write-Host "错误: Docker Compose未安装" -ForegroundColor Red
        return $false
    }
}

# 构建Docker镜像
if ($Build) {
    Write-Host "正在构建Docker镜像..." -ForegroundColor Yellow
    
    if (-not (Test-Docker)) {
        exit 1
    }
    
    # 构建API镜像
    Write-Host "构建API镜像..." -ForegroundColor Cyan
    docker build -f API/Dockerfile -t locationshare-api:latest .
    if ($LASTEXITCODE -ne 0) {
        Write-Host "API镜像构建失败" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "Docker镜像构建完成" -ForegroundColor Green
}

# 部署服务
if ($Deploy) {
    Write-Host "正在部署服务..." -ForegroundColor Yellow
    
    if (-not (Test-DockerCompose)) {
        exit 1
    }
    
    # 创建必要的目录
    if (-not (Test-Path "ssl")) {
        New-Item -ItemType Directory -Path "ssl" -Force
        Write-Host "已创建SSL证书目录，请将证书文件放入ssl目录" -ForegroundColor Yellow
    }
    
    # 使用Docker Compose部署
    Write-Host "启动服务容器..." -ForegroundColor Cyan
    docker-compose up -d
    if ($LASTEXITCODE -ne 0) {
        Write-Host "服务部署失败" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "服务部署完成" -ForegroundColor Green
    Write-Host "API服务地址: http://localhost:8080" -ForegroundColor Cyan
    Write-Host "数据库地址: localhost:5432" -ForegroundColor Cyan
}

# 启动服务
if ($Start) {
    Write-Host "正在启动服务..." -ForegroundColor Yellow
    docker-compose start
    if ($LASTEXITCODE -eq 0) {
        Write-Host "服务启动成功" -ForegroundColor Green
    } else {
        Write-Host "服务启动失败" -ForegroundColor Red
    }
}

# 停止服务
if ($Stop) {
    Write-Host "正在停止服务..." -ForegroundColor Yellow
    docker-compose stop
    if ($LASTEXITCODE -eq 0) {
        Write-Host "服务停止成功" -ForegroundColor Green
    } else {
        Write-Host "服务停止失败" -ForegroundColor Red
    }
}

# 重启服务
if ($Restart) {
    Write-Host "正在重启服务..." -ForegroundColor Yellow
    docker-compose restart
    if ($LASTEXITCODE -eq 0) {
        Write-Host "服务重启成功" -ForegroundColor Green
    } else {
        Write-Host "服务重启失败" -ForegroundColor Red
    }
}

# 查看日志
if ($Logs) {
    Write-Host "显示服务日志..." -ForegroundColor Yellow
    docker-compose logs -f
}

# 查看服务状态
if ($Status) {
    Write-Host "服务状态:" -ForegroundColor Yellow
    docker-compose ps
    
    Write-Host "`n容器资源使用情况:" -ForegroundColor Yellow
    docker stats --no-stream --format "table {{.Container}}\t{{.CPUPerc}}\t{{.MemUsage}}\t{{.NetIO}}"
}

Write-Host "=== 部署脚本执行完成 ===" -ForegroundColor Green