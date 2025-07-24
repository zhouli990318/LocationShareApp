# 使用官方的.NET 9.0运行时镜像作为基础镜像
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# 使用.NET 9.0 SDK镜像进行构建
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["API/LocationShareApp.API.csproj", "API/"]
RUN dotnet restore "API/LocationShareApp.API.csproj"
COPY . .
WORKDIR "/src/API"
RUN dotnet build "LocationShareApp.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LocationShareApp.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# 创建非root用户
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

ENTRYPOINT ["dotnet", "LocationShareApp.API.dll"]