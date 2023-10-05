﻿using PipManager.Languages;
using Serilog;

namespace PipManager.Controls;

public static class MsgBox
{
    public static async Task Success(string message)
    {
        var messageBox = new Wpf.Ui.Controls.MessageBox
        {
            Title = Lang.MsgBox_Title_Success,
            Content = message,
            MinWidth = 300,
            IsPrimaryButtonEnabled = false,
            CloseButtonText = Lang.MsgBox_CloseButton_Cancel
        };
        await messageBox.ShowDialogAsync();
    }
    public static async Task Error(string message)
    {
        var messageBox = new Wpf.Ui.Controls.MessageBox
        {
            Title = Lang.MsgBox_Title_Error,
            Content = message,
            MinWidth = 300,
            IsPrimaryButtonEnabled = false,
            CloseButtonText = Lang.MsgBox_CloseButton_Cancel
        };
        await messageBox.ShowDialogAsync();
    }

    public static async Task<Wpf.Ui.Controls.MessageBoxResult> Error(string message, string primaryButtonText)
    {
        var messageBox = new Wpf.Ui.Controls.MessageBox
        {
            Title = Lang.MsgBox_Title_Error,
            Content = message,
            MinWidth = 300,
            PrimaryButtonText = primaryButtonText,
            CloseButtonText = Lang.MsgBox_CloseButton_Cancel
        };
        var result = await messageBox.ShowDialogAsync();
        return result;
    }

    public static async Task<Wpf.Ui.Controls.MessageBoxResult> Warning(string message, string primaryButtonText)
    {
        var messageBox = new Wpf.Ui.Controls.MessageBox
        {
            Title = Lang.MsgBox_Title_Error,
            Content = message,
            MinWidth = 300,
            PrimaryButtonText = primaryButtonText,
            CloseButtonText = Lang.MsgBox_CloseButton_Cancel
        };
        var result = await messageBox.ShowDialogAsync();
        return result;
    }
}