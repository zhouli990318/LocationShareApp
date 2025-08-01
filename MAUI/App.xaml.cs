﻿using LocationShareApp.Services;
using LocationShareApp.Views;
using Microsoft.Extensions.Logging;

namespace LocationShareApp;

public partial class App : Application
{
    private readonly ILogger<App> _logger;

    public App(ILogger<App> logger)
    {
        _logger = logger;
        InitializeComponent();
        
        // 设置全局异常处理
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
        
        MainPage = new AppShell();
    }

    protected override async void OnStart()
    {
        base.OnStart();
        
        // 应用启动时的初始化
        try
        {
            var initService = ServiceHelper.GetService<IAppInitializationService>();
            if (initService != null)
            {
                await initService.InitializeAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "应用初始化失败");
        }
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            _logger.LogError(ex, "未处理的异常");
            // 这里可以添加日志记录或错误报告
        }
    }

    private void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
    {
        _logger.LogError(e.Exception, "未观察到的任务异常");
        e.SetObserved();
    }
}

// 服务帮助类
public static class ServiceHelper
{
    public static T GetService<T>() where T : class
    {
        try
        {
        return Application.Current?.Handler?.MauiContext?.Services?.GetService<T>();
        }
        catch
        {
            return null;
        }
    }
}