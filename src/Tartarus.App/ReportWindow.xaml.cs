using System.Windows;

namespace Tartarus.App;

/// <summary>The bug report's comment: what was gathered when it was asked for goes with it on Send.</summary>
public partial class ReportWindow : Window
{
    private readonly BugReport _report;

    /// <summary>What sending said, for the app's log; null when cancelled.</summary>
    public string? Result { get; private set; }

    public ReportWindow(BugReport report)
    {
        _report = report;
        InitializeComponent();
        if (report.Crash != null) Prompt.Text = "程序捕获到一个错误。出错前你在做什么？";
        Loaded += (_, _) =>
        {
            Activate();
            CommentBox.Focus();
        };
    }

    private async void SendButton_Click(object sender, RoutedEventArgs e)
    {
        SendButton.IsEnabled = CancelButton.IsEnabled = CommentBox.IsEnabled = false;
        StatusText.Text = "正在发送…";
        StatusText.Visibility = Visibility.Visible;
        string comment = CommentBox.Text;
        Result = await Task.Run(() => BugReports.SendAsync(_report, comment));
        // Closed by its title bar's button while sending: no dialog left to answer (a crash on 0.7.21).
        if (IsVisible) DialogResult = true;
    }

    // Shift+Enter sends; Enter alone is a new line.
    private void CommentBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key != System.Windows.Input.Key.Enter || !System.Windows.Input.Keyboard.Modifiers.HasFlag(System.Windows.Input.ModifierKeys.Shift) || !SendButton.IsEnabled) return;
        e.Handled = true;
        SendButton_Click(sender, e);
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
