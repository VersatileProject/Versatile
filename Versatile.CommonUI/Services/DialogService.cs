using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Versatile.Common;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace Versatile.CommonUI.Services;

public class DialogService
{

    public FileSavePicker CreateFileSavePicker((string, string)[] choices, string filename)
    {
        var picker = new FileSavePicker();
        picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        foreach(var c in choices)
        {
            picker.FileTypeChoices.Add(c.Item1, new List<string>() { c.Item2 });
        }
        picker.SuggestedFileName = filename;
        InitializeWithWindow.Initialize(picker, System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle);
        return picker;
    }

    public FileOpenPicker CreateFileOpenPicker(string[] choices)
    {
        var picker = new FileOpenPicker();
        picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        foreach(var c in choices)
        {
            picker.FileTypeFilter.Add(c);
        }
        InitializeWithWindow.Initialize(picker, System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle);
        return picker;
    }

    public async Task<bool> TrySave((string, string)[] choices, string filename, Action<string> action)
    {
        var picker = CreateFileSavePicker(choices, filename);

        var file = await picker.PickSaveFileAsync();
        if (file == null)
        {
            return false;
        }
        
        action(file.Path);
        return true;
    }

    public async Task<bool> TryOpen(string[] choices, Action<string> action)
    {
        var picker = CreateFileOpenPicker(choices);

        var file = await picker.PickSingleFileAsync();
        if (file == null)
        {
            return false;
        }

        action(file.Path);
        return true;
    }

    public async Task<ContentDialogResult> ShowDialog(string title, string content, string close, string primary = null, string secondary = null)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = content,
            CloseButtonText = close,
            PrimaryButtonText = primary,
            SecondaryButtonText = secondary,
            XamlRoot = VersatileApp.GetService<Window>().Content.XamlRoot
        };

        var result = await dialog.ShowAsync();
        return result;
    }


    public async Task<ContentDialogResult> ShowYesNoCancel(string title, string content)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = content,
            CloseButtonText = VersatileApp.Localize("Main/Dialog_CancelButtonText"),
            PrimaryButtonText = VersatileApp.Localize("Main/Dialog_YesButtonText"),
            SecondaryButtonText = VersatileApp.Localize("Main/Dialog_NoButtonText"),
            XamlRoot = VersatileApp.GetService<Window>().Content.XamlRoot
        };

        var result = await dialog.ShowAsync();
        return result;
    }

    public async Task<ContentDialogResult> ShowOk(string title, string content)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = content,
            CloseButtonText = VersatileApp.Localize("Main/Dialog_OkButtonText"),
            XamlRoot = VersatileApp.GetService<Window>().Content.XamlRoot
        };

        var result = await dialog.ShowAsync();
        return result;
    }

    public async Task<string?> ShowInput(string title, string content)
    {
        var textbox = new TextBox()
        {
            Text = content,
            SelectionStart = 0,
            SelectionLength = content?.Length ?? 0,
        };
        var dialog = new ContentDialog
        {
            Title = title,
            Content = textbox,
            CloseButtonText = "Cancel",
            PrimaryButtonText = "OK",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = VersatileApp.GetService<Window>().Content.XamlRoot
        };

        var result = await dialog.ShowAsync();
        if(result == ContentDialogResult.None)
        {
            return null;
        }
        return textbox.Text;
    }

    //public async Task ShowProgress(string title, Func<Task> working)
    //{
    //    var progress = new ProgressRing();
    //    progress.IsActive = true;
    //    progress.Width = 80;
    //    progress.Height = 80;

    //    var cts = new CancellationTokenSource();
    //    var dialog = new ContentDialog
    //    {
    //        Title = title,
    //        Content = progress,
    //        CloseButtonText = "Cancel",
    //        XamlRoot = VersatileApp.GetService<Window>().Content.XamlRoot
    //    };

    //    var task = new Task(working);

    //    await Task.WhenAny(dialog.ShowAsync().AsTask(), working());

    //    dialog.Hide();
    //}

}
