using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Media;
using System.Threading.Tasks;
using System.Windows.Forms;
using OtakAgent.Core.Chat;
using OtakAgent.Core.Configuration;
using OtakAgent.Core.Personality;

namespace OtakAgent.App.Forms;

public partial class MainForm : Form
{
    private readonly SettingsService _settingsService;
    private readonly ChatService _chatService;
    private readonly PersonalityPromptBuilder _personalityPromptBuilder;
    private readonly List<ChatMessage> _history = new();
    private readonly System.Windows.Forms.Timer _animationTimer;

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

    public MainForm(SettingsService settingsService, ChatService chatService, PersonalityPromptBuilder personalityPromptBuilder)
    {
        _settingsService = settingsService;
        _chatService = chatService;
        _personalityPromptBuilder = personalityPromptBuilder;

        InitializeComponent();
        ApplyBubbleLayout();

        Load += MainForm_Load;
        FormClosing += MainForm_FormClosing;
        _sendButton.Click += async (_, _) => await HandleSendButtonClickAsync().ConfigureAwait(false);
        _secondaryButton.Click += async (_, _) => await HandleSecondaryButtonClickAsync();
        _notifyToggleTopMostMenuItem.CheckedChanged += (_, _) => TopMost = _notifyToggleTopMostMenuItem.Checked;
        _notifyExitMenuItem.Click += (_, _) => Close();
        _notifyIcon.MouseDoubleClick += (_, _) => ToggleWindowVisibility(true);
        _inputTextBox.KeyDown += InputTextBox_KeyDown;
        _inputTextBox.Enter += InputTextBox_Enter;
        _inputTextBox.Leave += InputTextBox_Leave;
        _bubblePanel.MouseDown += BubblePanel_MouseDown;
        _bubblePanel.MouseMove += BubblePanel_MouseMove;
        _bubblePanel.SizeChanged += (_, _) => UpdateBubbleBackground();
        _characterPicture.MouseDown += BubblePanel_MouseDown;
        _characterPicture.MouseMove += BubblePanel_MouseMove;

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
        _settings = await _settingsService.LoadAsync().ConfigureAwait(false);
        ApplyLocalization();
        LoadAssets();
        PositionWindow();
        PositionCharacter();
        UpdateTooltips();
    }

    private void ApplyLocalization()
    {
        Text = "AgentTalk";
        _promptLabel.Text = _settings.English ? "What would you like to do?" : "何をしますか？";
        if (ShouldShowPlaceholderText())
        {
            _inputTextBox.Text = PlaceholderText();
        }
        _sendButton.Text = SendText();
        _secondaryButton.Text = OptionsText();
        _notifyToggleTopMostMenuItem.Text = _settings.English ? "Always on Top" : "常に手前に表示";
        _notifyExitMenuItem.Text = _settings.English ? "Exit" : "終了";
        _toolTip.SetToolTip(_characterPicture, _settings.English ? "Double-click to bring AgentTalk to front" : "ダブルクリックでAgentTalkを手前に表示");
    }

    private void ApplyBubbleLayout()
    {
        const int bubbleWidth = 226;
        const int bubbleHeight = 148;
        _bubblePanel.Location = new Point(44, 24);
        _bubblePanel.Size = new Size(bubbleWidth, bubbleHeight);

        _promptLabel.MaximumSize = new Size(194, 0);
        _promptLabel.Location = new Point(16, 14);

        _inputTextBox.Location = new Point(16, 42);
        _inputTextBox.Size = new Size(194, 56);

        _secondaryButton.Size = new Size(76, 20);
        _secondaryButton.Location = new Point(12, 105);

        _sendButton.Size = new Size(76, 20);
        _sendButton.Location = new Point(138, 105);
    }

    private void LoadAssets()
    {
        _animationTimer.Stop();

        _characterPicture.Image?.Dispose();
        _characterPicture.Image = null;
        DisposeCharacterFrames();
        DisposeBubbleImages();

        var iconPath = GetResourcePath("app.ico");
        if (File.Exists(iconPath))
        {
            using var iconStream = File.OpenRead(iconPath);
            Icon = new Icon(iconStream);
            _notifyIcon.Icon = Icon;
        }

        var baseImageName = _settings.English ? "clippy" : "kairu";
        _primaryCharacterFrame = LoadImageFromResources($"{baseImageName}_start.gif");
        _secondaryCharacterFrame = LoadImageFromResources($"{baseImageName}.gif");
        SetCharacterImage(_primaryCharacterFrame);

        _bubbleTopImage = LoadImageFromResources("windowTop.png");
        _bubbleCenterImage = LoadImageFromResources("windowCenter.png");
        _bubbleBottomImage = LoadImageFromResources("windowBottom.png");
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

        if (_secondaryCharacterFrame is not null)
        {
            _animationTimer.Interval = 1400;
            _animationTimer.Start();
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
        using var bitmap = new Bitmap(width, height);
        using var g = Graphics.FromImage(bitmap);
        g.SmoothingMode = SmoothingMode.HighQuality;

        var topHeight = _bubbleTopImage.Height;
        var bottomHeight = _bubbleBottomImage.Height;
        var centerHeight = Math.Max(0, height - topHeight - bottomHeight);

        g.DrawImage(_bubbleTopImage, new Rectangle(0, 0, width, topHeight));
        g.DrawImage(_bubbleBottomImage, new Rectangle(0, height - bottomHeight, width, bottomHeight));
        if (centerHeight > 0)
        {
            using var texture = new TextureBrush(_bubbleCenterImage, WrapMode.Tile);
            texture.ScaleTransform((float)width / _bubbleCenterImage.Width, 1f);
            g.FillRectangle(texture, new Rectangle(0, topHeight, width, centerHeight));
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

    private void PositionCharacter()
    {
        var characterLocation = new Point(158, 173);
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
            return;
        }

        if (_sendButton.Text == ContinueText())
        {
            EnterInputMode(clearText: true);
            return;
        }

        var rawInput = _inputTextBox.Text;
        if (IsPlaceholderText(rawInput))
        {
            rawInput = string.Empty;
        }

        var question = rawInput.Trim();
        if (string.IsNullOrEmpty(question))
        {
            SystemSounds.Beep.Play();
            return;
        }

        try
        {
            BeginProcessing();
            _inputTextBox.ReadOnly = true;
            _inputTextBox.BackColor = Color.FromArgb(255, 255, 206);

            var historySnapshot = _history.ToList();
            var systemPrompt = _personalityPromptBuilder.Build(_settings);
            if (string.IsNullOrWhiteSpace(systemPrompt))
            {
                systemPrompt = null;
            }

            var response = await _chatService.SendAsync(new ChatRequest(
                _settings,
                question,
                historySnapshot,
                systemPrompt)).ConfigureAwait(false);

            _history.Add(new ChatMessage("user", question));
            _history.Add(new ChatMessage("assistant", response));

            _promptLabel.Text = _settings.English ? "Here is the response!" : "回答が届いたよ！";
            _inputTextBox.Text = response;
            _sendButton.Text = ContinueText();
            _secondaryButton.Text = ResetButtonText();
            UpdateTooltips();

            if (_settings.AutoCopyToClipboard && !string.IsNullOrEmpty(response))
            {
                TryCopyToClipboard(response);
            }

            _settings.LastUserMessage = question;
            await _settingsService.SaveAsync(_settings).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _promptLabel.Text = _settings.English ? "Something went wrong." : "エラーが発生しました。";
            _inputTextBox.ReadOnly = false;
            _inputTextBox.BackColor = Color.White;
            MessageBox.Show(this, ex.Message, "AgentTalk", MessageBoxButtons.OK, MessageBoxIcon.Error);
            EnterInputMode(clearText: false);
        }
        finally
        {
            EndProcessing();
        }
    }

    private Task HandleSecondaryButtonClickAsync()
    {
        if (_isProcessing)
        {
            return Task.CompletedTask;
        }

        if (_secondaryButton.Text == ResetButtonText())
        {
            _history.Clear();
            EnterInputMode(clearText: true);
            return Task.CompletedTask;
        }

        using var settingsForm = new SettingsForm(_settingsService, _settings);
        if (settingsForm.ShowDialog(this) == DialogResult.OK)
        {
            _settings = settingsForm.UpdatedSettings;
            ApplyLocalization();
            LoadAssets();
            UpdateTooltips();
        }

        return Task.CompletedTask;
    }

    private void EnterInputMode(bool clearText)
    {
        _sendButton.Text = SendText();
        _secondaryButton.Text = OptionsText();
        _inputTextBox.ReadOnly = false;
        _inputTextBox.BackColor = Color.White;
        if (clearText)
        {
            _inputTextBox.Clear();
        }
        _inputTextBox.Focus();
        _promptLabel.Text = _settings.English ? "What would you like to do?" : "何をしますか？";
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
        var sendTooltip = _sendButton.Text == ContinueText()
            ? (_settings.English ? "Continue (Ctrl+Enter)" : "続ける (Ctrl+Enter)")
            : (_settings.English ? "Send (Ctrl+Enter)" : "送信 (Ctrl+Enter)");
        _toolTip.SetToolTip(_sendButton, sendTooltip);

        var secondaryTooltip = _secondaryButton.Text == ResetButtonText()
            ? (_settings.English ? "Reset (Ctrl+Backspace)" : "リセット (Ctrl+Backspace)")
            : (_settings.English ? "Options" : "オプション");
        _toolTip.SetToolTip(_secondaryButton, secondaryTooltip);
    }

    private string PlaceholderText() => _settings.English ? EnglishPlaceholderText : JapanesePlaceholderText;

    private bool ShouldShowPlaceholderText()
    {
        var current = _inputTextBox.Text;
        return string.IsNullOrWhiteSpace(current) || IsPlaceholderText(current);
    }

    private static bool IsPlaceholderText(string value) => value == EnglishPlaceholderText || value == JapanesePlaceholderText;
    private void InputTextBox_Enter(object? sender, EventArgs e)
    {
        if (IsPlaceholderText(_inputTextBox.Text))
        {
            _inputTextBox.Clear();
        }
    }

    private void InputTextBox_Leave(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_inputTextBox.Text))
        {
            _inputTextBox.Text = PlaceholderText();
        }
    }
    private void InputTextBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Control && e.KeyCode == Keys.Enter)
        {
            e.Handled = true;
            e.SuppressKeyPress = true;
            _ = HandleSendButtonClickAsync();
        }
        else if (e.Control && e.KeyCode == Keys.Back)
        {
            e.Handled = true;
            e.SuppressKeyPress = true;
            _ = HandleSecondaryButtonClickAsync();
        }
    }

    private void OnAnimationTimerTick(object? sender, EventArgs e)
    {
        _animationTimer.Stop();
        SetCharacterImage(_secondaryCharacterFrame);
    }

    private void SetCharacterImage(Image? source)
    {
        if (source is null)
        {
            return;
        }

        Image? clone = null;
        try
        {
            clone = (Image)source.Clone();
        }
        catch
        {
            return;
        }

        var previous = _characterPicture.Image;
        _characterPicture.Image = clone;
        previous?.Dispose();
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
        _characterPicture.Image?.Dispose();
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

    private Image? LoadImageFromResources(string fileName)
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
    private string ContinueText() => _settings.English ? "Continue" : "続ける";
    private string ResetButtonText() => _settings.English ? "Reset" : "リセット";
    private string OptionsText() => _settings.English ? "Options" : "オプション";
    private string ProcessingText() => _settings.English ? "Processing..." : "処理中...";
}
