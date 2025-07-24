using LocationShareApp.Services;
using LocationShareApp.Views;

﻿namespace LocationShareApp;

public partial class App : Application
{
    public App()
    {
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
            System.Diagnostics.Debug.WriteLine($"应用初始化失败: {ex.Message}");
        }
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"未处理的异常: {ex.Message}");
            // 这里可以添加日志记录或错误报告
        }
    }

    private void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"未观察到的任务异常: {e.Exception.Message}");
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
            return Current?.Handler?.MauiContext?.Services?.GetService<T>();
        }
        catch
        {
            return null;
        }
    }
}