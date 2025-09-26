using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using OtakAgent.Core.Chat;
using OtakAgent.Core.Configuration;
using OtakAgent.Core.Personality;
using OtakAgent.Core.Updates;

namespace OtakAgent.App.Forms;

public partial class MainForm : Form
{

    private readonly SettingsService _settingsService;
    private readonly ChatService _chatService;
    private readonly PersonalityPromptBuilder _personalityPromptBuilder;
    private readonly UpdateChecker _updateChecker;
    private readonly List<ChatMessage> _history = new();
    private const int MaxHistoryItems = 100; // Prevent unbounded growth
    private readonly System.Windows.Forms.Timer _animationTimer;

    private static readonly Color BubbleFillColor = Color.FromArgb(255, 255, 206);
    private static readonly Color MagentaColorKey = Color.Magenta;

    private AgentTalkSettings _settings = AgentTalkSettings.CreateDefault();
    private Image? _primaryCharacterFrame;
    private Image? _secondaryCharacterFrame;
    private Image? _bubbleTopImage;
    private Image? _bubbleCenterImage;
    private Image? _bubbleBottomImage;
    private string? _processingOriginalSendText;
    private bool _isProcessing;
    private Point? _dragOffset;

    private const string EnglishPlaceholderText = "Type your question here, and then click Send.";
    private const string JapanesePlaceholderText = "ここに質問を書いて『送信』ボタンを押してください。";
    private const string EnglishNoApiKeyText = "Please configure your OpenAI API Key in Options.";
    private const string JapaneseNoApiKeyText = "オプションでOpenAI API Keyを設定してください。";

    private bool _isPlaceholderActive;

    public MainForm(SettingsService settingsService, ChatService chatService, PersonalityPromptBuilder personalityPromptBuilder, UpdateChecker updateChecker)
    {
        _settingsService = settingsService;
        _chatService = chatService;
        _personalityPromptBuilder = personalityPromptBuilder;
        _updateChecker = updateChecker;

        InitializeComponent();
        ApplyBubbleLayout();

        // Enable form-level key preview to handle shortcuts even when TextBox is readonly
        KeyPreview = true;
        KeyDown += MainForm_KeyDown;

        Load += MainForm_Load;
        FormClosing += MainForm_FormClosing;
        _sendButton.Click += async (_, _) => await HandleSendButtonClickAsync().ConfigureAwait(false);
        _secondaryButton.Click += async (_, _) => await HandleSecondaryButtonClickAsync();
        _expandToggleButton.Click += (_, _) => ToggleExpandedView();
        _notifyToggleTopMostMenuItem.CheckedChanged += (_, _) => TopMost = _notifyToggleTopMostMenuItem.Checked;
        _notifyExitMenuItem.Click += (_, _) => Close();
        _notifyIcon.MouseDoubleClick += (_, _) => ToggleWindowVisibility(true);
        _inputTextBox.KeyDown += InputTextBox_KeyDown;
        _inputTextBox.Enter += InputTextBox_Enter;
        _inputTextBox.Leave += InputTextBox_Leave;
        _bubblePanel.MouseDown += BubblePanel_MouseDown;
        _bubblePanel.MouseMove += BubblePanel_MouseMove;
        _bubblePanel.SizeChanged += (_, _) => UpdateBubbleBackground();
        _characterPicture.MouseDown += CharacterPicture_MouseDown;
        _characterPicture.MouseMove += BubblePanel_MouseMove;
        _characterPicture.MouseDoubleClick += (_, _) => _bubblePanel.Visible = !_bubblePanel.Visible;
        _characterPicture.MouseUp += CharacterPicture_MouseUp;
        _notifyContextMenu.Opening += (_, _) => UpdateContextMenu();

        _animationTimer = new System.Windows.Forms.Timer { Interval = 1400 };
        _animationTimer.Tick += OnAnimationTimerTick;

        Controls.SetChildIndex(_characterPicture, 0);
    }

    private async void MainForm_Load(object? sender, EventArgs e)
    {
        await InitializeAsync().ConfigureAwait(false);
    }

    private async Task InitializeAsync()
    {
        try
        {
            _settings = await _settingsService.LoadAsync().ConfigureAwait(false);

            // Run UI updates on UI thread
            if (InvokeRequired)
            {
                Invoke(() =>
                {
                    ApplyLocalization();
                    UpdateContextMenu();
                    LoadAssets();
                    PositionWindow();
                    PositionCharacter();
                    UpdateTooltips();
                });
            }
            else
            {
                ApplyLocalization();
                UpdateContextMenu();
                LoadAssets();
                PositionWindow();
                PositionCharacter();
                UpdateTooltips();
            }



            // Set initial tooltip with resource info
            UpdateSystemTrayTooltip();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Initialization error: {ex.Message}");
        }
    }

    private void ApplyLocalization()
    {
        Text = "otak-agent";
        _promptLabel.Text = _settings.English ? "What would you like to do?" : "何をしますか？";

        if (string.IsNullOrWhiteSpace(_inputTextBox.Text) || _isPlaceholderActive || IsPlaceholderText(_inputTextBox.Text))
        {
            ShowPlaceholder();
        }
        else
        {
            _isPlaceholderActive = false;
        }

        _sendButton.Text = SendText();
        _secondaryButton.Text = OptionsText();
        _toolTip.SetToolTip(_characterPicture, _settings.English ? "Double-click to bring otak-agent to front" : "ダブルクリックでotak-agentを手前に表示");
    }
    private void ApplyBubbleLayout()
    {
        const int bubbleWidth = 226;
        const int defaultFormHeight = 312;
        int bubbleHeight = 148;
        int bubblePanelY = 24;
        int textBoxHeight = 59;
        int buttonY = 100;
        int formHeight = defaultFormHeight;
        
        // Update toggle button text
        _expandToggleButton.Text = _settings.ExpandedTextbox ? "▲" : "▼";
        
        // Apply 5x expansion if ExpandedTextbox is enabled - expand upwards
        if (_settings.ExpandedTextbox)
        {
            int expansionAmount = (59 * 5) - 59; // Additional height needed (236 pixels)
            textBoxHeight = 59 * 5; // 295 pixels
            bubbleHeight = 148 + expansionAmount; // Expand bubble height
            formHeight = defaultFormHeight + expansionAmount; // Expand form height
            // Keep bubble panel at same position - don't move it up
            buttonY = 32 + textBoxHeight + 9; // Position buttons below expanded textbox with proper spacing
            
            // Only adjust position if we're not already expanded
            if (Size.Height != formHeight)
            {
                var currentLocation = Location;
                Size = new Size(310, formHeight);
                Location = new Point(currentLocation.X, currentLocation.Y - expansionAmount);
            }
        }
        else
        {
            // Only adjust position if we're currently expanded
            if (Size.Height != defaultFormHeight)
            {
                int expansionAmount = (59 * 5) - 59; // Amount to move back down
                var currentLocation = Location;
                Size = new Size(310, defaultFormHeight);
                Location = new Point(currentLocation.X, currentLocation.Y + expansionAmount);
            }
        }
        
        _bubblePanel.Location = new Point(44, bubblePanelY);
        _bubblePanel.Size = new Size(bubbleWidth, bubbleHeight);

        _promptLabel.MaximumSize = new Size(194, 0);
        _promptLabel.Location = new Point(8, 8);

        _inputTextBox.Location = new Point(8, 32);
        _inputTextBox.Size = new Size(210, textBoxHeight);

        _secondaryButton.Size = new Size(76, 20);
        _secondaryButton.Location = new Point(12, buttonY);

        _sendButton.Size = new Size(76, 20);
        _sendButton.Location = new Point(138, buttonY);
    }

    private void LoadAssets()
    {
        _animationTimer.Stop();

        _characterPicture.Image = null;
        DisposeCharacterFrames();
        DisposeBubbleImages();

        var iconPath = GetResourcePath("app.ico");
        if (File.Exists(iconPath))
        {
            using var iconStream = File.OpenRead(iconPath);
            Icon = new Icon(iconStream);
            _notifyIcon.Icon = Icon;

            // Initialize tooltip text (will be updated with resource info later)
            _notifyIcon.Text = "otak-agent";
        }

        var baseImageName = _settings.English ? "clippy" : "kairu";
        _primaryCharacterFrame = LoadImageFromResources($"{baseImageName}_start.gif", treatMagentaAsTransparent: true);
        _secondaryCharacterFrame = LoadImageFromResources($"{baseImageName}.gif", treatMagentaAsTransparent: true);

        var startFrameVisible = _primaryCharacterFrame is not null && HasVisibleContent(_primaryCharacterFrame);
        if (startFrameVisible)
        {
            SetCharacterImage(_primaryCharacterFrame);
        }
        else
        {
            SetCharacterImage(_secondaryCharacterFrame); // ensure fallback when start gif invisible
        }

        _bubbleTopImage = LoadImageFromResources("windowTop.png", treatMagentaAsTransparent: true);
        _bubbleCenterImage = LoadImageFromResources("windowCenter.png", treatMagentaAsTransparent: true);
        _bubbleBottomImage = LoadImageFromResources("windowBottom.png", treatMagentaAsTransparent: true);
        UpdateBubbleBackground();

        var soundPath = GetResourcePath(_settings.English ? "clippy.wav" : "kairu.wav");
        if (File.Exists(soundPath))
        {
            try
            {
                using var soundPlayer = new SoundPlayer(soundPath);
                soundPlayer.Play();
            }
            catch
            {
                // ignore sound errors
            }
        }

        if (_secondaryCharacterFrame is not null && startFrameVisible)
        {
            var animationDuration = GetAnimationDurationMilliseconds(_primaryCharacterFrame!);
            _animationTimer.Interval = animationDuration > 0 ? animationDuration : 1400;
            _animationTimer.Start();
        }
        else if (_secondaryCharacterFrame is not null)
        {
            SetCharacterImage(_secondaryCharacterFrame);
        }

        ApplyBubbleLayout();
        PositionCharacter();
    }

    private void UpdateBubbleBackground()
    {
        if (_bubbleTopImage == null || _bubbleCenterImage == null || _bubbleBottomImage == null)
        {
            return;
        }

        var width = _bubblePanel.Width;
        var height = _bubblePanel.Height;
        using var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        using var g = Graphics.FromImage(bitmap);
        g.SmoothingMode = SmoothingMode.HighQuality;
        g.CompositingMode = CompositingMode.SourceOver;
        g.CompositingQuality = CompositingQuality.HighQuality;
        g.InterpolationMode = InterpolationMode.NearestNeighbor;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        g.Clear(Color.Transparent);

        using var attributes = new ImageAttributes();
        attributes.SetColorKey(MagentaColorKey, MagentaColorKey);

        var topCrop = 0;

        var topSrcHeight = _bubbleTopImage.Height;
        var bottomSrcHeight = _bubbleBottomImage.Height;

        var topDestY = 0;
        var bottomDestY = Math.Max(topDestY + topSrcHeight, height - bottomSrcHeight - 5);

        var centerStart = topDestY + topSrcHeight;
        var centerHeight = Math.Max(0, bottomDestY - centerStart);
        if (centerHeight > 0)
        {
            using var fillBrush = new SolidBrush(BubbleFillColor);
            g.FillRectangle(fillBrush, new Rectangle(0, centerStart, width, centerHeight));

            if (_bubbleCenterImage is not null)
            {
                DrawTiledImage(g, _bubbleCenterImage, new Rectangle(0, centerStart, width, centerHeight), attributes);
            }
        }

        if (topSrcHeight > 0)
        {
            var topSource = new Rectangle(0, topCrop, _bubbleTopImage.Width, topSrcHeight);
            var topDest = new Rectangle(0, topDestY, width, topSrcHeight);
            g.DrawImage(_bubbleTopImage, topDest, topSource.X, topSource.Y, topSource.Width, topSource.Height, GraphicsUnit.Pixel, attributes);
        }

        if (bottomSrcHeight > 0)
        {
            var bottomSource = new Rectangle(0, 0, _bubbleBottomImage.Width, bottomSrcHeight);
            var bottomDest = new Rectangle(0, bottomDestY, width, bottomSrcHeight);
            g.DrawImage(_bubbleBottomImage, bottomDest, bottomSource.X, bottomSource.Y, bottomSource.Width, bottomSource.Height, GraphicsUnit.Pixel, attributes);
        }

        _bubblePanel.BackgroundImage?.Dispose();
        _bubblePanel.BackgroundImage = (Image)bitmap.Clone();
    }
    private void PositionWindow()
    {
        var workingArea = Screen.PrimaryScreen?.WorkingArea ?? new Rectangle(0, 0, 1280, 720);
        var targetX = workingArea.Right - Width - 20;
        var targetY = workingArea.Bottom - Height - 20;
        Location = new Point(targetX, targetY);
    }

    private void ToggleExpandedView()
    {
        _settings.ExpandedTextbox = !_settings.ExpandedTextbox;
        _expandToggleButton.Text = _settings.ExpandedTextbox ? "▲" : "▼";
        ApplyBubbleLayout();
        UpdateBubbleBackground();
        PositionCharacter();
        _ = _settingsService.SaveAsync(_settings);
    }

    private void PositionCharacter()
    {
        var characterLocation = new Point(158, 173);
        
        // Adjust character position when textbox is expanded
        if (_settings.ExpandedTextbox)
        {
            int expansionAmount = (59 * 5) - 59; // 236 pixels
            characterLocation = new Point(characterLocation.X, characterLocation.Y + expansionAmount);
        }
        
        if (_settings.English)
        {
            characterLocation = new Point(characterLocation.X - 8, characterLocation.Y);
        }
        else
        {
            characterLocation = new Point(characterLocation.X - 2, characterLocation.Y - 10);
        }

        _characterPicture.Location = characterLocation;
        _characterPicture.Size = new Size(100, 108);
        _characterPicture.BringToFront();
    }

    private async Task HandleSendButtonClickAsync()
    {
        if (_isProcessing)
        {
            SystemSounds.Beep.Play();
            return;
        }

        if (_sendButton.Text == InputButtonText())
        {
            EnterInputMode(clearText: true);
            return;
        }

        var question = _isPlaceholderActive ? string.Empty : _inputTextBox.Text.Trim();
        if (string.IsNullOrEmpty(question))
        {
            SystemSounds.Beep.Play();
            return;
        }

        // Check if API key is configured
        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            SystemSounds.Beep.Play();
            var message = _settings.English
                ? "Please configure your OpenAI API Key in Options first."
                : "まずオプションでOpenAI API Keyを設定してください。";
            MessageBox.Show(this, message, "otak-agent", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            BeginProcessing();

            // Update UI on UI thread
            if (InvokeRequired)
            {
                Invoke(() =>
                {
                    _inputTextBox.ReadOnly = true;
                    _inputTextBox.BackColor = Color.FromArgb(255, 255, 206);
                });
            }
            else
            {
                _inputTextBox.ReadOnly = true;
                _inputTextBox.BackColor = Color.FromArgb(255, 255, 206);
            }

            var historySnapshot = _history.ToList();
            var systemPrompt = _personalityPromptBuilder.Build(_settings);
            if (string.IsNullOrWhiteSpace(systemPrompt))
            {
                systemPrompt = null;
            }

            // ChatService automatically detects and uses the appropriate endpoint
            string response;

            // Use cancellation token with timeout to prevent freezing
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            try
            {
                response = await _chatService.SendAsync(new ChatRequest(
                    _settings,
                    question,
                    historySnapshot,
                    systemPrompt), cts.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                response = _settings.English
                    ? "Request timed out. Please check your internet connection and try again."
                    : "リクエストがタイムアウトしました。インターネット接続を確認してもう一度お試しください。";
            }

            // Limit history size to prevent unbounded growth
            const int maxHistoryItems = 100;
            if (_history.Count >= maxHistoryItems)
            {
                _history.RemoveRange(0, _history.Count - maxHistoryItems + 2);
            }

            _history.Add(new ChatMessage("user", question));
            _history.Add(new ChatMessage("assistant", response));

            // Update UI on UI thread
            if (InvokeRequired)
            {
                Invoke(() =>
                {
                    _promptLabel.Text = _settings.English ? "Here is the response!" : "回答が届いたよ！";
                    _inputTextBox.Text = response;
                    _inputTextBox.ReadOnly = true;
                    _inputTextBox.BackColor = SystemColors.Control;
                    _sendButton.Text = InputButtonText();
                    _secondaryButton.Text = ResetButtonText();
                    _processingOriginalSendText = null; // Clear to prevent EndProcessing from overwriting
                    UpdateTooltips();
                });
            }
            else
            {
                _promptLabel.Text = _settings.English ? "Here is the response!" : "回答が届いたよ！";
                _inputTextBox.Text = response;
                _inputTextBox.ReadOnly = true;
                _inputTextBox.BackColor = SystemColors.Control;
                _sendButton.Text = InputButtonText();
                _secondaryButton.Text = ResetButtonText();
                _processingOriginalSendText = null; // Clear to prevent EndProcessing from overwriting
                UpdateTooltips();
            }

            if (_settings.AutoCopyToClipboard && !string.IsNullOrEmpty(response))
            {
                if (InvokeRequired)
                {
                    Invoke(() => TryCopyToClipboard(response));
                }
                else
                {
                    TryCopyToClipboard(response);
                }
            }

            _settings.LastUserMessage = question;
            await _settingsService.SaveAsync(_settings).ConfigureAwait(false);
        }
        catch (HttpRequestException ex)
        {
            // Network-related errors
            if (InvokeRequired)
            {
                Invoke(() =>
                {
                    _promptLabel.Text = _settings.English ? "Network error occurred." : "ネットワークエラーが発生しました。";
                    _inputTextBox.ReadOnly = false;
                    _inputTextBox.BackColor = Color.White;
                    BeginInvoke(() => ErrorDialog.ShowError(this, ex.Message));
                    EnterInputMode(clearText: false);
                });
            }
            else
            {
                _promptLabel.Text = _settings.English ? "Network error occurred." : "ネットワークエラーが発生しました。";
                _inputTextBox.ReadOnly = false;
                _inputTextBox.BackColor = Color.White;
                BeginInvoke(() => ErrorDialog.ShowError(this, ex.Message));
                EnterInputMode(clearText: false);
            }
        }
        catch (InvalidOperationException ex)
        {
            // Configuration errors
            if (InvokeRequired)
            {
                Invoke(() =>
                {
                    _promptLabel.Text = _settings.English ? "Configuration error." : "設定エラーです。";
                    _inputTextBox.ReadOnly = false;
                    _inputTextBox.BackColor = Color.White;
                    BeginInvoke(() => ErrorDialog.ShowWarning(this, ex.Message));
                    EnterInputMode(clearText: false);
                });
            }
            else
            {
                _promptLabel.Text = _settings.English ? "Configuration error." : "設定エラーです。";
                _inputTextBox.ReadOnly = false;
                _inputTextBox.BackColor = Color.White;
                BeginInvoke(() => ErrorDialog.ShowWarning(this, ex.Message));
                EnterInputMode(clearText: false);
            }
        }
        catch (Exception ex)
        {
            // Other errors
            if (InvokeRequired)
            {
                Invoke(() =>
                {
                    _promptLabel.Text = _settings.English ? "Something went wrong." : "エラーが発生しました。";
                    _inputTextBox.ReadOnly = false;
                    _inputTextBox.BackColor = Color.White;
                    BeginInvoke(() => ErrorDialog.ShowError(this, ex.Message));
                    EnterInputMode(clearText: false);
                });
            }
            else
            {
                _promptLabel.Text = _settings.English ? "Something went wrong." : "エラーが発生しました。";
                _inputTextBox.ReadOnly = false;
                _inputTextBox.BackColor = Color.White;
                BeginInvoke(() => ErrorDialog.ShowError(this, ex.Message));
                EnterInputMode(clearText: false);
            }
        }
        finally
        {
            if (InvokeRequired)
            {
                Invoke(() => EndProcessing());
            }
            else
            {
                EndProcessing();
            }
        }
    }

    private async Task HandleSecondaryButtonClickAsync()
    {
        if (_isProcessing)
        {
            SystemSounds.Beep.Play();
            return;
        }

        if (_secondaryButton.Text == ResetButtonText())
        {
            _history.Clear();
            EnterInputMode(clearText: true);
            return;
        }

        // Show settings dialog on a background task to avoid blocking UI
        await Task.Run(() =>
        {
            if (InvokeRequired)
            {
                Invoke(() => ShowSettingsDialogSync());
            }
            else
            {
                ShowSettingsDialogSync();
            }
        }).ConfigureAwait(false);
    }

    private void ShowSettingsDialogSync()
    {
        var previousTopMost = TopMost;
        try
        {
            if (previousTopMost)
            {
                TopMost = false;
            }

            using var settingsForm = new SettingsForm(_settingsService, _settings)
            {
                StartPosition = FormStartPosition.CenterParent,
                TopMost = true
            };

            var result = settingsForm.ShowDialog(this);
            if (result == DialogResult.OK)
            {
                _settings = settingsForm.UpdatedSettings;
                ApplyLocalization();
                UpdateContextMenu();
                LoadAssets();
                UpdateTooltips();
            }
        }
        finally
        {
            TopMost = previousTopMost;
            if (previousTopMost)
            {
                Activate();
            }
        }
    }

    private void EnterInputMode(bool clearText, bool continuingConversation = false)
    {
        _sendButton.Text = SendText();
        _secondaryButton.Text = continuingConversation ? ResetButtonText() : OptionsText();
        _inputTextBox.ReadOnly = false;
        _inputTextBox.BackColor = Color.White;

        if (clearText || string.IsNullOrWhiteSpace(_inputTextBox.Text) || IsPlaceholderText(_inputTextBox.Text))
        {
            // Don't show placeholder when continuing conversation
            if (!continuingConversation)
            {
                ShowPlaceholder();
            }
            else
            {
                _inputTextBox.Text = string.Empty;
                _isPlaceholderActive = false;
            }
        }
        else
        {
            _isPlaceholderActive = false;
        }

        _inputTextBox.Focus();

        // Different prompt text when continuing conversation
        if (continuingConversation)
        {
            _promptLabel.Text = _settings.English ? "Continue the conversation..." : "会話を続けてください...";
        }
        else
        {
            _promptLabel.Text = _settings.English ? "What would you like to do?" : "何をしますか？";
        }

        UpdateTooltips();
    }
    private void BeginProcessing()
    {
        _isProcessing = true;
        _processingOriginalSendText = _sendButton.Text;
        _sendButton.Enabled = false;
        _secondaryButton.Enabled = false;
        _sendButton.Text = ProcessingText();
        UseWaitCursor = true;
    }

    private void EndProcessing()
    {
        _isProcessing = false;
        UseWaitCursor = false;
        _sendButton.Enabled = true;
        _secondaryButton.Enabled = true;
        if (_processingOriginalSendText != null)
        {
            _sendButton.Text = _processingOriginalSendText;
            _processingOriginalSendText = null;
        }
        UpdateTooltips();
    }

    private void UpdateTooltips()
    {
        var sendTooltip = _sendButton.Text == InputButtonText()
            ? (_settings.English ? "Enter new message (Ctrl+Enter)" : "新しいメッセージを入力 (Ctrl+Enter)")
            : (_settings.English ? "Send (Ctrl+Enter)" : "送信 (Ctrl+Enter)");
        _toolTip.SetToolTip(_sendButton, sendTooltip);

        var secondaryTooltip = _secondaryButton.Text == ResetButtonText()
            ? (_settings.English ? "Reset (Ctrl+Backspace)" : "リセット (Ctrl+Backspace)")
            : (_settings.English ? "Options" : "オプション");
        _toolTip.SetToolTip(_secondaryButton, secondaryTooltip);
    }

    private string PlaceholderText()
    {
        // Check if API key is set
        bool hasApiKey = !string.IsNullOrWhiteSpace(_settings.ApiKey);

        if (!hasApiKey)
        {
            return _settings.English ? EnglishNoApiKeyText : JapaneseNoApiKeyText;
        }

        return _settings.English ? EnglishPlaceholderText : JapanesePlaceholderText;
    }

    private static bool IsPlaceholderText(string value) =>
        value == EnglishPlaceholderText || value == JapanesePlaceholderText ||
        value == EnglishNoApiKeyText || value == JapaneseNoApiKeyText;

    private void ShowPlaceholder()
    {
        _isPlaceholderActive = true;
        _inputTextBox.Text = PlaceholderText();
    }

    private void ClearPlaceholderIfNeeded()
    {
        if (_isPlaceholderActive || IsPlaceholderText(_inputTextBox.Text))
        {
            _isPlaceholderActive = false;
            _inputTextBox.Clear();
        }
    }

    private void InputTextBox_Enter(object? sender, EventArgs e)
    {
        ClearPlaceholderIfNeeded();
    }

    private void InputTextBox_Leave(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_inputTextBox.Text))
        {
            ShowPlaceholder();
        }
        else
        {
            _isPlaceholderActive = IsPlaceholderText(_inputTextBox.Text);
        }
    }
    private void MainForm_KeyDown(object? sender, KeyEventArgs e)
    {
        // Handle form-level keyboard shortcuts when TextBox is readonly
        if (_inputTextBox.ReadOnly)
        {
            if (e.Control && e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;

                // Check if we're in response mode (Input button is shown)
                if (_sendButton.Text == InputButtonText())
                {
                    // Act as Input button - continue conversation
                    EnterInputMode(clearText: true, continuingConversation: true);
                    return;
                }
            }
            else if (e.Control && e.KeyCode == Keys.Back)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;

                // Only handle secondary button if it's showing Reset
                if (_secondaryButton.Text == ResetButtonText())
                {
                    _ = HandleSecondaryButtonClickAsync();
                }
            }
        }
    }

    private void InputTextBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Control && e.KeyCode == Keys.Enter)
        {
            e.Handled = true;
            e.SuppressKeyPress = true;

            // Check if we're in response mode (Input button is shown)
            if (_sendButton.Text == InputButtonText())
            {
                // Act as Input button - continue conversation
                EnterInputMode(clearText: true, continuingConversation: true);
                return;
            }
            else
            {
                // Clear placeholder if needed before sending
                ClearPlaceholderIfNeeded();

                // Focus the input box to ensure it's ready
                _inputTextBox.Focus();

                _ = HandleSendButtonClickAsync();
            }
        }
        else if (e.Control && e.KeyCode == Keys.Back)
        {
            e.Handled = true;
            e.SuppressKeyPress = true;

            // Only handle reset if the secondary button is showing Reset
            if (_secondaryButton.Text == ResetButtonText())
            {
                _ = HandleSecondaryButtonClickAsync();
            }
        }
        else if (!e.Control && !e.Alt && !e.Shift)
        {
            // Clear placeholder on any regular key press
            ClearPlaceholderIfNeeded();
        }
    }

    private void OnAnimationTimerTick(object? sender, EventArgs e)
    {
        _animationTimer.Stop();

        // Use BeginInvoke to avoid blocking the UI thread
        if (!IsDisposed && !Disposing)
        {
            BeginInvoke(() =>
            {
                SetCharacterImage(_secondaryCharacterFrame);
            });
        }
    }


    private void UpdateSystemTrayTooltip()
    {
        if (!IsDisposed && !Disposing && _notifyIcon != null)
        {
            _notifyIcon.Text = "otak-agent";
        }
    }

    private void SetCharacterImage(Image? source)
    {
        if (source is null || IsDisposed || Disposing)
        {
            return;
        }

        // Ensure we're on the UI thread
        if (InvokeRequired)
        {
            BeginInvoke(() => SetCharacterImage(source));
            return;
        }

        _characterPicture.Image = source;
    }

    private void CharacterPicture_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            _dragOffset = new Point(e.X, e.Y);
        }
    }

    private void CharacterPicture_MouseUp(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Right)
        {
            UpdateContextMenu();
            _notifyContextMenu.Show(_characterPicture, e.Location);
        }
    }

    private void UpdateContextMenu()
    {
        _notifyContextMenu.Items.Clear();

        // Add "System Prompt Presets" submenu
        var presetsMenuItem = new ToolStripMenuItem(_settings.English ? "System Prompt Presets" : "システムプロンプトプリセット");

        // Add built-in presets
        var builtInPresets = SystemPromptPreset.GetBuiltInPresets(_settings.English);
        foreach (var preset in builtInPresets)
        {
            var builtInLabel = _settings.English ? "[Built-in]" : "[組み込み]";
            var menuItem = new ToolStripMenuItem($"{builtInLabel} {preset.Name}");
            var capturedPreset = preset;

            // Check if this is the currently selected preset by comparing prompt content
            if (!string.IsNullOrEmpty(_settings.SystemPrompt) && _settings.SystemPrompt == preset.Prompt)
            {
                menuItem.Checked = true;
            }

            menuItem.Click += (_, _) => ApplyPreset(capturedPreset);
            presetsMenuItem.DropDownItems.Add(menuItem);
        }

        // Add user presets if any
        if (_settings.SystemPromptPresets != null && _settings.SystemPromptPresets.Count > 0)
        {
            if (presetsMenuItem.DropDownItems.Count > 0)
            {
                presetsMenuItem.DropDownItems.Add(new ToolStripSeparator());
            }

            foreach (var preset in _settings.SystemPromptPresets)
            {
                var menuItem = new ToolStripMenuItem(preset.Name);
                var capturedPreset = preset;

                // Check if this is the currently selected preset
                if (!string.IsNullOrEmpty(_settings.SelectedPresetId) && _settings.SelectedPresetId == preset.Id)
                {
                    menuItem.Checked = true;
                    menuItem.Font = new System.Drawing.Font(menuItem.Font, System.Drawing.FontStyle.Bold);
                }

                menuItem.Click += (_, _) => ApplyPreset(capturedPreset);
                presetsMenuItem.DropDownItems.Add(menuItem);
            }
        }

        _notifyContextMenu.Items.Add(presetsMenuItem);
        _notifyContextMenu.Items.Add(new ToolStripSeparator());

        // Add Settings option
        var settingsMenuItem = new ToolStripMenuItem(_settings.English ? "Settings..." : "設定...");
        settingsMenuItem.Click += (_, _) => ShowSettingsDialogSync();
        _notifyContextMenu.Items.Add(settingsMenuItem);

        // Add Check for Updates option
        var updateMenuItem = new ToolStripMenuItem(_settings.English ? "Check for Updates..." : "アップデートの確認...");
        updateMenuItem.Click += async (_, _) => await CheckForUpdatesAsync();
        _notifyContextMenu.Items.Add(updateMenuItem);

        // Add separator
        _notifyContextMenu.Items.Add(new ToolStripSeparator());

        // Update the existing Always on Top menu item
        _notifyToggleTopMostMenuItem.Text = _settings.English ? "Always on Top" : "常に手前に表示";
        _notifyToggleTopMostMenuItem.Checked = TopMost;
        _notifyContextMenu.Items.Add(_notifyToggleTopMostMenuItem);

        // Update the existing Exit menu item
        _notifyExitMenuItem.Text = _settings.English ? "Exit" : "終了";
        _notifyContextMenu.Items.Add(_notifyExitMenuItem);
    }

    private async void ApplyPreset(SystemPromptPreset preset)
    {
        _settings.SystemPrompt = preset.Prompt;
        _settings.SelectedPresetId = preset.Id;

        // Save settings to persist the change
        await _settingsService.SaveAsync(_settings).ConfigureAwait(false);

        // Show confirmation
        var message = _settings.English
            ? $"Applied preset: {preset.Name}"
            : $"プリセットを適用しました: {preset.Name}";

        BeginInvoke(() =>
        {
            _toolTip.Show(message, _characterPicture, 50, 50, 3000);
        });
    }

    private async Task CheckForUpdatesAsync()
    {
        try
        {
            // Show checking message
            var checkingMsg = _settings.English
                ? "Checking for updates..."
                : "アップデートを確認中...";

            BeginInvoke(() =>
            {
                _toolTip.Show(checkingMsg, _characterPicture, 50, 50, 2000);
            });

            var updateInfo = await _updateChecker.CheckForUpdatesAsync();

            if (updateInfo != null)
            {
                // New version available
                var message = _settings.English
                    ? $"New version available: v{updateInfo.LatestVersion}\n" +
                      $"Current version: v{updateInfo.CurrentVersion}\n\n" +
                      $"Visit the release page to download?"
                    : $"新しいバージョンがあります: v{updateInfo.LatestVersion}\n" +
                      $"現在のバージョン: v{updateInfo.CurrentVersion}\n\n" +
                      $"リリースページを開きますか？";

                var title = _settings.English ? "Update Available" : "アップデート可能";

                BeginInvoke(() =>
                {
                    var result = MessageBox.Show(this, message, title,
                        MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                    if (result == DialogResult.Yes)
                    {
                        // Open the release page in default browser
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = updateInfo.ReleaseUrl,
                            UseShellExecute = true
                        });
                    }
                });
            }
            else
            {
                // Already up to date
                var message = _settings.English
                    ? "You are using the latest version."
                    : "最新バージョンを使用しています。";

                BeginInvoke(() =>
                {
                    _toolTip.Show(message, _characterPicture, 50, 50, 3000);
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Update check failed: {ex.Message}");

            var errorMsg = _settings.English
                ? "Failed to check for updates."
                : "アップデートの確認に失敗しました。";

            BeginInvoke(() =>
            {
                _toolTip.Show(errorMsg, _characterPicture, 50, 50, 3000);
            });
        }
    }

    private void BubblePanel_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            _dragOffset = new Point(e.X, e.Y);
        }
    }

    private void BubblePanel_MouseMove(object? sender, MouseEventArgs e)
    {
        if (_dragOffset.HasValue && e.Button == MouseButtons.Left)
        {
            var screenPosition = PointToScreen(e.Location);
            Location = new Point(screenPosition.X - _dragOffset.Value.X, screenPosition.Y - _dragOffset.Value.Y);
        }
    }

    private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        _notifyIcon.Visible = false;
        _animationTimer.Stop();
        _animationTimer.Dispose();
        _characterPicture.Image = null;
        DisposeCharacterFrames();
        DisposeBubbleImages();
    }

    private void ToggleWindowVisibility(bool bringToFront)
    {
        if (WindowState == FormWindowState.Minimized)
        {
            WindowState = FormWindowState.Normal;
        }

        if (!Visible)
        {
            Show();
        }

        if (bringToFront)
        {
            Activate();
        }
    }

    private static void TryCopyToClipboard(string response)
    {
        try
        {
            Clipboard.SetText(response);
        }
        catch
        {
            // ignore clipboard exceptions
        }
    }

    private static void DrawTiledImage(Graphics graphics, Image tile, Rectangle destination, ImageAttributes attributes)
    {
        var tileWidth = tile.Width;
        var tileHeight = tile.Height;

        for (var y = destination.Top; y < destination.Bottom; y += tileHeight)
        {
            var remainingHeight = Math.Min(tileHeight, destination.Bottom - y);
            for (var x = destination.Left; x < destination.Right; x += tileWidth)
            {
                var remainingWidth = Math.Min(tileWidth, destination.Right - x);
                var destRect = new Rectangle(x, y, remainingWidth, remainingHeight);
                var srcRect = new Rectangle(0, 0, remainingWidth, remainingHeight);
                graphics.DrawImage(tile, destRect, srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height, GraphicsUnit.Pixel, attributes);
            }
        }
    }

    private static bool HasVisibleContent(Image image)
    {
        if (ImageAnimator.CanAnimate(image))
        {
            try
            {
                return image.GetFrameCount(FrameDimension.Time) > 0;
            }
            catch
            {
                return true;
            }
        }

        const int minAlpha = 32;
        const int fullyOpaqueAlpha = 224;
        const int minFullyOpaquePixels = 200;
        const int minAccumulatedAlpha = 50000;

        try
        {
            using var bitmap = new Bitmap(image);

            var width = bitmap.Width;
            var height = bitmap.Height;
            var accumulatedAlpha = 0;
            var fullyOpaquePixels = 0;

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var pixel = bitmap.GetPixel(x, y);
                    if (pixel.A < minAlpha)
                    {
                        continue;
                    }

                    if (IsApproximatelyMagenta(pixel))
                    {
                        continue;
                    }

                    accumulatedAlpha += pixel.A;
                    if (pixel.A >= fullyOpaqueAlpha)
                    {
                        fullyOpaquePixels++;
                        if (fullyOpaquePixels >= minFullyOpaquePixels)
                        {
                            return true;
                        }
                    }
                }
            }

            return accumulatedAlpha >= minAccumulatedAlpha;
        }
        catch
        {
            return true;
        }
    }

    private static bool IsApproximatelyMagenta(Color color)
    {
        if (color.A == 0)
        {
            return false;
        }

        const int maxGreen = 48;
        const int minRedBlue = 180;
        const int maxDeviation = 80;

        if (color.G > maxGreen)
        {
            return false;
        }

        var redDeviation = Math.Abs(color.R - 255);
        var blueDeviation = Math.Abs(color.B - 255);

        return color.R >= minRedBlue
               && color.B >= minRedBlue
               && redDeviation <= maxDeviation
               && blueDeviation <= maxDeviation;
    }
        private static int GetAnimationDurationMilliseconds(Image image)
    {
        return 1200;
    }



    private Image? LoadImageFromResources(string fileName, bool treatMagentaAsTransparent = false)
    {
        var path = GetResourcePath(fileName);
        if (!File.Exists(path))
        {
            return null;
        }

        try
        {
            return new Bitmap(path);
        }
        catch
        {
            return null;
        }
    }

    private void DisposeCharacterFrames()
    {
        _primaryCharacterFrame?.Dispose();
        _primaryCharacterFrame = null;
        _secondaryCharacterFrame?.Dispose();
        _secondaryCharacterFrame = null;
    }

    private void DisposeBubbleImages()
    {
        _bubbleTopImage?.Dispose();
        _bubbleTopImage = null;
        _bubbleCenterImage?.Dispose();
        _bubbleCenterImage = null;
        _bubbleBottomImage?.Dispose();
        _bubbleBottomImage = null;

        _bubblePanel.BackgroundImage?.Dispose();
        _bubblePanel.BackgroundImage = null;
    }

    private string GetResourcePath(string fileName) => Path.Combine(AppContext.BaseDirectory, "Resources", fileName);

    private string SendText() => _settings.English ? "Send" : "送信";
    private string InputButtonText() => _settings.English ? "Input" : "入力";
    private string ResetButtonText() => _settings.English ? "Reset" : "リセット";
    private string OptionsText() => _settings.English ? "Settings" : "設定";
    private string ProcessingText() => _settings.English ? "Processing..." : "処理中...";

}





