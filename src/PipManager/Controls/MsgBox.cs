﻿using PipManager.Languages;

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

    public static async Task Warning(string message)
    {
        var messageBox = new MessageBox
        {
            Title = Lang.MsgBox_Title_Warning,
            Content = message,
            MinWidth = 300,
            IsPrimaryButtonEnabled = false,
            CloseButtonText = Lang.MsgBox_CloseButton_Cancel
        };
        await messageBox.ShowDialogAsync();
    }

    public static async Task<MessageBoxResult> Error(string message, string primaryButtonText)
    {
        var messageBox = new MessageBox
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

    public static async Task<MessageBoxResult> Warning(string message, string primaryButtonText, string? title = null)
    {
        title ??= Lang.MsgBox_Title_Warning;
        var messageBox = new MessageBox
        {
            Title = title,
            Content = message,
            MinWidth = 300,
            PrimaryButtonText = primaryButtonText,
            CloseButtonText = Lang.MsgBox_CloseButton_Cancel
        };
        var result = await messageBox.ShowDialogAsync();
        return result;
    }
}