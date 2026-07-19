using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using VPet.Plugin.AIPet.LLM;

namespace VPet.Plugin.AIPet
{
    /// <summary>
    /// AI desktop pet settings window (built entirely in code to avoid plugin XAML resource loading issues)
    /// </summary>
    public class winAISetting : Window
    {
        private readonly AIPetPlugin plugin;
        private AISetting Config => plugin.Config;

        private ComboBox cbProvider;
        private TextBox tbBaseUrl, tbModel, tbUserNick, tbPersona, tbInterval, tbQuietStart, tbQuietEnd, tbMaxHistory;
        private PasswordBox pbApiKey;
        private CheckBox chkTools, chkReactions, chkProactive, chkVision;
        private TextBlock tbStatus;

        public winAISetting(AIPetPlugin plugin)
        {
            this.plugin = plugin;
            Title = "AIDeskPet Settings";
            Width = 480;
            Height = 640;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Topmost = true;
            ResizeMode = ResizeMode.CanResize;
            Content = BuildUI();
            LoadValues();
        }

        private UIElement BuildUI()
        {
            var scroll = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
            var root = new StackPanel { Margin = new Thickness(16) };
            scroll.Content = root;

            root.Children.Add(Header("API"));

            cbProvider = new ComboBox { Margin = new Thickness(0, 2, 0, 8) };
            cbProvider.Items.Add(new ComboBoxItem { Content = "Anthropic (Claude native)", Tag = "anthropic" });
            cbProvider.Items.Add(new ComboBoxItem { Content = "OpenAI-compatible (DeepSeek/OpenRouter/etc.)", Tag = "openai" });
            cbProvider.SelectionChanged += (_, _) => UpdateUrlHint();
            root.Children.Add(Label("Protocol"));
            root.Children.Add(cbProvider);

            root.Children.Add(Label("API URL (leave empty for official default)"));
            tbBaseUrl = Input();
            root.Children.Add(tbBaseUrl);

            root.Children.Add(Label("API Key"));
            pbApiKey = new PasswordBox { Margin = new Thickness(0, 2, 0, 8), Padding = new Thickness(4) };
            root.Children.Add(pbApiKey);

            root.Children.Add(Label("Model"));
            tbModel = Input();
            root.Children.Add(tbModel);

            var btnTest = new Button { Content = "Test connection", Padding = new Thickness(12, 4, 12, 4), HorizontalAlignment = HorizontalAlignment.Left };
            btnTest.Click += BtnTest_Click;
            root.Children.Add(btnTest);
            tbStatus = new TextBlock { Margin = new Thickness(0, 4, 0, 8), TextWrapping = TextWrapping.Wrap };
            root.Children.Add(tbStatus);

            root.Children.Add(Header("Persona"));
            root.Children.Add(Label("What the assistant calls you"));
            tbUserNick = Input();
            root.Children.Add(tbUserNick);
            root.Children.Add(Label("Custom persona (leave empty for default; name is set in game settings)"));
            tbPersona = new TextBox
            {
                Margin = new Thickness(0, 2, 0, 8),
                Padding = new Thickness(4),
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                MinHeight = 80,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            };
            root.Children.Add(tbPersona);

            root.Children.Add(Header("Behavior"));
            chkTools = new CheckBox { Content = "Let the AI control the body (animations / sleep / memory)", Margin = new Thickness(0, 4, 0, 4) };
            root.Children.Add(chkTools);
            chkReactions = new CheckBox { Content = "React to interactions (pats, etc.)", Margin = new Thickness(0, 4, 0, 4) };
            root.Children.Add(chkReactions);
            chkProactive = new CheckBox { Content = "Allow proactive messages", Margin = new Thickness(0, 4, 0, 4) };
            root.Children.Add(chkProactive);
            chkVision = new CheckBox { Content = "Let her see your screen (screen vision — needs a vision-capable model)", Margin = new Thickness(0, 4, 0, 4) };
            root.Children.Add(chkVision);

            var grid = new Grid { Margin = new Thickness(0, 4, 0, 0) };
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            var left = new StackPanel { Margin = new Thickness(0, 0, 8, 0) };
            var right = new StackPanel();
            Grid.SetColumn(left, 0); Grid.SetColumn(right, 1);
            grid.Children.Add(left); grid.Children.Add(right);
            left.Children.Add(Label("Proactive interval (min)"));
            tbInterval = Input(); left.Children.Add(tbInterval);
            left.Children.Add(Label("Chat history kept"));
            tbMaxHistory = Input(); left.Children.Add(tbMaxHistory);
            right.Children.Add(Label("Quiet hours start"));
            tbQuietStart = Input(); right.Children.Add(tbQuietStart);
            right.Children.Add(Label("Quiet hours end"));
            tbQuietEnd = Input(); right.Children.Add(tbQuietEnd);
            root.Children.Add(grid);

            var btnRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 16, 0, 0) };
            var btnSave = new Button { Content = "Save", Padding = new Thickness(24, 6, 24, 6), Margin = new Thickness(0, 0, 12, 0) };
            btnSave.Click += BtnSave_Click;
            var btnClear = new Button { Content = "Clear chat history", Padding = new Thickness(12, 6, 12, 6) };
            btnClear.Click += (_, _) =>
            {
                if (MessageBox.Show("Clear all chat history? (long-term memory is not affected)", "AIDeskPet", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    plugin.Controller.ClearHistory();
                    tbStatus.Text = "Chat history cleared";
                }
            };
            btnRow.Children.Add(btnSave);
            btnRow.Children.Add(btnClear);
            root.Children.Add(btnRow);

            return scroll;
        }

        private static TextBlock Header(string text) => new()
        {
            Text = text,
            FontSize = 16,
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(0, 12, 0, 4),
        };
        private static TextBlock Label(string text) => new() { Text = text, Margin = new Thickness(0, 4, 0, 0) };
        private static TextBox Input() => new() { Margin = new Thickness(0, 2, 0, 8), Padding = new Thickness(4) };

        private void LoadValues()
        {
            cbProvider.SelectedIndex = Config.Provider == "openai" ? 1 : 0;
            tbBaseUrl.Text = Config.BaseUrl;
            pbApiKey.Password = Config.ApiKey;
            tbModel.Text = Config.Model;
            tbUserNick.Text = Config.UserNick;
            tbPersona.Text = Config.Persona;
            chkTools.IsChecked = Config.EnableTools;
            chkReactions.IsChecked = Config.EnableReactions;
            chkProactive.IsChecked = Config.EnableProactive;
            chkVision.IsChecked = Config.EnableVision;
            tbInterval.Text = Config.ProactiveInterval.ToString();
            tbQuietStart.Text = Config.QuietStart.ToString();
            tbQuietEnd.Text = Config.QuietEnd.ToString();
            tbMaxHistory.Text = Config.MaxHistory.ToString();
            UpdateUrlHint();
        }

        private void UpdateUrlHint()
        {
            if (tbBaseUrl == null) return;
            var provider = SelectedProvider;
            // hint only; does not overwrite what the user already entered
            var hint = provider == "anthropic" ? "https://api.anthropic.com" : "https://api.openai.com/v1";
            if (string.IsNullOrWhiteSpace(tbBaseUrl.Text))
                tbBaseUrl.ToolTip = "Default: " + hint;
        }

        private string SelectedProvider => (string)((ComboBoxItem)cbProvider.SelectedItem)?.Tag ?? "anthropic";

        private void ApplyValues()
        {
            Config.Provider = SelectedProvider;
            Config.BaseUrl = tbBaseUrl.Text.Trim();
            Config.ApiKey = pbApiKey.Password.Trim();
            Config.Model = tbModel.Text.Trim();
            Config.UserNick = string.IsNullOrWhiteSpace(tbUserNick.Text) ? "you" : tbUserNick.Text.Trim();
            Config.Persona = tbPersona.Text.Trim();
            Config.EnableTools = chkTools.IsChecked == true;
            Config.EnableReactions = chkReactions.IsChecked == true;
            Config.EnableProactive = chkProactive.IsChecked == true;
            Config.EnableVision = chkVision.IsChecked == true;
            if (int.TryParse(tbInterval.Text, out var i) && i >= 1) Config.ProactiveInterval = i;
            if (int.TryParse(tbQuietStart.Text, out var qs) && qs >= 0 && qs <= 23) Config.QuietStart = qs;
            if (int.TryParse(tbQuietEnd.Text, out var qe) && qe >= 0 && qe <= 23) Config.QuietEnd = qe;
            if (int.TryParse(tbMaxHistory.Text, out var mh) && mh >= 4) Config.MaxHistory = mh;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            ApplyValues();
            Config.Save();
            tbStatus.Text = "Saved ✓";
        }

        private async void BtnTest_Click(object sender, RoutedEventArgs e)
        {
            ApplyValues();
            if (!Config.IsConfigured)
            {
                tbStatus.Text = "Please fill in the API Key and model name first";
                return;
            }
            tbStatus.Text = "Testing...";
            try
            {
                var client = LLMClient.Create(Config);
                var reply = await Task.Run(() => client.ChatAsync(new LLMRequest
                {
                    Messages = { new ChatMsg("user", "Reply with exactly: connected") },
                    MaxTokens = 100,
                }));
                tbStatus.Text = "✓ Connected! Model replied: " + (reply.Length > 60 ? reply.Substring(0, 60) : reply);
            }
            catch (Exception ex)
            {
                tbStatus.Text = "✗ Failed: " + ex.Message;
            }
        }
    }
}
