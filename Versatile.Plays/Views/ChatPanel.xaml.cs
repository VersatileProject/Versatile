using System;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

namespace Versatile.Plays.Views;

public sealed partial class ChatPanel : UserControl
{
    public static DependencyProperty IsLockedProperty = DependencyProperty.Register("IsLocked", typeof(bool), typeof(ChatPanel), new PropertyMetadata(false));
    public bool IsLocked { get => (bool)GetValue(IsLockedProperty); set => SetValue(IsLockedProperty, value); }

    public static DependencyProperty MaxMessageLengthProperty = DependencyProperty.Register("MaxMessageLength", typeof(int), typeof(ChatPanel), new PropertyMetadata(1024));
    public int MaxMessageLength { get => (int)GetValue(MaxMessageLengthProperty); set => SetValue(MaxMessageLengthProperty, value); }

    public event EventHandler<ChatMessageEventArgs> MessageSended;
    public event EventHandler LogBoxDoubleClicked;

    public ChatPanel()
    {
        this.InitializeComponent();
        
        LogTextBox.AddHandler(DoubleTappedEvent, new DoubleTappedEventHandler(LogTextBox_DoubleTapped), true);
    }

    private void TextBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (IsLocked) return;
        if (e.Key == Windows.System.VirtualKey.Enter)
        {
            var textbox = (TextBox)sender;
            var message = textbox.Text.Trim();
            if(MaxMessageLength > 0 && message.Length > MaxMessageLength)
            {
                message = message[..MaxMessageLength];
            }
            if (message.Length > 0)
            {
                var args = new ChatMessageEventArgs
                {
                    Message = message
                };
                MessageSended?.Invoke(this, args);
                textbox.Text = "";
            }
        }
    }

    public void Clear()
    {
        LogTextBox.Blocks.Clear();
        ChatTextBox.Text = string.Empty;
    }

    public void AppendText(string message)
    {
        AppendText(new LogRun() {
            Text = message,
        });
    }

    public void AppendText(params LogRun[] runs)
    {
        var paragraph = new Paragraph();
        foreach(var runinfo in runs)
        {
            var run = new Run();
            if (runinfo.IsBold) run.FontWeight = FontWeights.Bold;
            if (runinfo.IsItalic) run.FontStyle = Windows.UI.Text.FontStyle.Italic;
            if (runinfo.Color.HasValue) run.Foreground = new SolidColorBrush(runinfo.Color.Value);
            run.Text = runinfo.Text;
            paragraph.Inlines.Add(run);
        }
        LogTextBox.Blocks.Add(paragraph);
    }

    private void LogTextBox_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        LogBoxDoubleClicked?.Invoke(this, new());
    }

}
