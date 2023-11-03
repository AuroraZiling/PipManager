﻿using PipManager.Models;
using PipManager.Services.Configuration;
using Serilog;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;

namespace PipManager;

public partial class AppStarting
{
    [LibraryImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool AllocConsole();

    public readonly AppConfig Config;
    public bool ShowConsoleWindow = false;

    public AppStarting()
    {
        Config = ConfigurationService.LoadConfiguration();
        Directory.CreateDirectory(AppInfo.CrushesDir);
        Directory.CreateDirectory(AppInfo.LogDir);
    }

    public void LoadLanguage()
    {
        var language = Config.Personalization.Language;
        if (language != "Auto")
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo(language);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(language);
        }
        Log.Information($"Language sets to {language}");
    }

    public void StartLogging()
    {
        if (ShowConsoleWindow)
        {
            AllocConsole();
            Log.Logger = new LoggerConfiguration()
                .Enrich.WithProperty("Version", AppInfo.AppVersion)
                .MinimumLevel.Debug()
                .WriteTo.Console(outputTemplate: "{Timestamp:HH:mm:ss} [{Level}] (Thread: {ThreadId}) {Message}{NewLine}{Exception}")
                .Enrich.With(new ThreadIdEnricher())
                .WriteTo.File(Path.Combine(AppInfo.LogDir, $"log_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt"), outputTemplate: "{Timestamp:HH:mm:ss} [{Level}] (Thread: {ThreadId}) {Message}{NewLine}{Exception}")
                .CreateLogger();
        }
        else
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.WithProperty("Version", AppInfo.AppVersion)
                .MinimumLevel.Debug()
                .Enrich.With(new ThreadIdEnricher())
                .WriteTo.File(Path.Combine(AppInfo.LogDir, $"log_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt"), outputTemplate: "{Timestamp:HH:mm:ss} [{Level}] (Thread: {ThreadId}) {Message}{NewLine}{Exception}")
                .CreateLogger();
        }
        Log.Information("Logging started");
    }

    public void LogDeletion()
    {
        if (!Config.Personalization.LogAutoDeletion || !Directory.Exists(AppInfo.LogDir)) return;
        var fileList = Directory.GetFileSystemEntries(AppInfo.LogDir);
        var logFileAmount = fileList.Count(file => File.Exists(file) && file.EndsWith(".txt"));
        if (logFileAmount >= Config.Personalization.LogAutoDeletionTimes)
        {
            var directoryInfo = new DirectoryInfo(AppInfo.LogDir);
            var filesInfo = directoryInfo.GetFileSystemInfos();
            foreach (var file in filesInfo)
            {
                if (file.Extension != ".txt") continue;
                try
                {
                    File.Delete(file.FullName);
                }
                catch
                {
                    continue;
                }
            }
            Log.Information($"{logFileAmount} log file(s) deleted");
        }
    }

    public void CrushesDeletion()
    {
        if (!Config.Personalization.CrushesAutoDeletion || !Directory.Exists(AppInfo.CrushesDir)) return;
        var fileList = Directory.GetFileSystemEntries(AppInfo.CrushesDir);
        var crushFileAmount = fileList.Count(file => File.Exists(file) && file.EndsWith(".txt"));
        if (crushFileAmount >= Config.Personalization.CrushesAutoDeletionTimes)
        {
            var directoryInfo = new DirectoryInfo(AppInfo.CrushesDir);
            var filesInfo = directoryInfo.GetFileSystemInfos();
            foreach (var file in filesInfo)
            {
                if (file.Extension != ".txt") continue;
                try
                {
                    File.Delete(file.FullName);
                }
                catch
                {
                    continue;
                }
            }
            Log.Information($"{crushFileAmount} crush file(s) deleted");
        }
    }
}