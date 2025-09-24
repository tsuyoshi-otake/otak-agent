#nullable enable
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using OtakAgent.App.Controls;

namespace OtakAgent.App.Forms;

partial class MainForm
{
    private IContainer? components = null;
    private Panel _bubblePanel = null!;
    private Label _promptLabel = null!;
    private TextBox _inputTextBox = null!;
    private Button _sendButton = null!;
    private Button _secondaryButton = null!;
    private Button _expandToggleButton = null!;
    private CharacterCanvas _characterPicture = null!;
    private NotifyIcon _notifyIcon = null!;
    private ContextMenuStrip _notifyContextMenu = null!;
    private ToolStripMenuItem _notifyToggleTopMostMenuItem = null!;
    private ToolStripMenuItem _notifyExitMenuItem = null!;
    private ToolTip _toolTip = null!;

    private static Color ButtonBaseColor => Color.FromArgb(255, 255, 255, 206);
    private static Color ButtonHoverColor => Color.FromArgb(255, 255, 247, 184);
    private static Color ButtonPressedColor => Color.FromArgb(255, 255, 238, 170);
    private static Color ButtonBorderColor => Color.FromArgb(255, 204, 153, 102);

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            components?.Dispose();
        }

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new Container();
        _bubblePanel = new Panel();
        _promptLabel = new Label();
        _inputTextBox = new TextBox();
        _sendButton = new Button();
        _secondaryButton = new Button();
        _expandToggleButton = new Button();
        _characterPicture = new CharacterCanvas();
        _notifyIcon = new NotifyIcon(components);
        _notifyContextMenu = new ContextMenuStrip(components);
        _notifyToggleTopMostMenuItem = new ToolStripMenuItem();
        _notifyExitMenuItem = new ToolStripMenuItem();
        _toolTip = new ToolTip(components);
        _bubblePanel.SuspendLayout();
        SuspendLayout();
        //
        //_bubblePanel
        //
        _bubblePanel.BackColor = Color.Transparent;
        _bubblePanel.Controls.Add(_promptLabel);
        _bubblePanel.Controls.Add(_inputTextBox);
        _bubblePanel.Controls.Add(_sendButton);
        _bubblePanel.Controls.Add(_secondaryButton);
        _bubblePanel.Controls.Add(_expandToggleButton);
        _bubblePanel.Location = new Point(44, 24);
        _bubblePanel.Name = "_bubblePanel";
        _bubblePanel.Size = new Size(226, 148);
        _bubblePanel.TabIndex = 0;
        //
        //_promptLabel
        //
        _promptLabel.AutoSize = true;
        _promptLabel.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
        _promptLabel.ForeColor = Color.Black;
        _promptLabel.Location = new Point(16, 14);
        _promptLabel.MaximumSize = new Size(194, 0);
        _promptLabel.Name = "_promptLabel";
        _promptLabel.Size = new Size(180, 19);
        _promptLabel.TabIndex = 0;
        _promptLabel.Text = "What would you like to do?";
        //
        //_inputTextBox
        //
        _inputTextBox.BorderStyle = BorderStyle.FixedSingle;
        _inputTextBox.Location = new Point(16, 42);
        _inputTextBox.MaxLength = 4000;
        _inputTextBox.Multiline = true;
        _inputTextBox.Name = "_inputTextBox";
        _inputTextBox.ScrollBars = ScrollBars.Vertical;
        _inputTextBox.Size = new Size(194, 56);
        _inputTextBox.TabIndex = 1;
        //
        //_sendButton
        //
        ApplyLegacyButtonStyle(_sendButton);
        _sendButton.Location = new Point(138, 105);
        _sendButton.Name = "_sendButton";
        _sendButton.Size = new Size(76, 20);
        _sendButton.TabIndex = 3;
        _sendButton.Text = "Send";
        //
        //_secondaryButton
        //
        ApplyLegacyButtonStyle(_secondaryButton);
        _secondaryButton.Location = new Point(12, 105);
        _secondaryButton.Name = "_secondaryButton";
        _secondaryButton.Size = new Size(76, 20);
        _secondaryButton.TabIndex = 2;
        _secondaryButton.Text = "Options";
        //
        //_expandToggleButton
        //
        _expandToggleButton.BackColor = Color.FromArgb(255, 255, 247, 184);
        _expandToggleButton.FlatAppearance.BorderSize = 0;
        _expandToggleButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(255, 255, 238, 170);
        _expandToggleButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(255, 255, 255, 206);
        _expandToggleButton.FlatStyle = FlatStyle.Flat;
        _expandToggleButton.Font = new Font("Segoe UI", 6F, FontStyle.Regular, GraphicsUnit.Point);
        _expandToggleButton.ForeColor = Color.FromArgb(102, 51, 0);
        _expandToggleButton.Location = new Point(200, 8);
        _expandToggleButton.Name = "_expandToggleButton";
        _expandToggleButton.Size = new Size(16, 16);
        _expandToggleButton.TabIndex = 6;
        _expandToggleButton.Text = "▼";
        _expandToggleButton.UseVisualStyleBackColor = false;
        //
        //_characterPicture
        //
        _characterPicture.BackColor = Color.Transparent;
        _characterPicture.Location = new Point(158, 181);
        _characterPicture.Margin = new Padding(0);
        _characterPicture.Name = "_characterPicture";
        _characterPicture.Size = new Size(100, 108);
        _characterPicture.TabIndex = 1;
        _characterPicture.TabStop = false;
        //
        //_notifyIcon
        //
        _notifyIcon.ContextMenuStrip = _notifyContextMenu;
        _notifyIcon.Text = "otak-agent";
        _notifyIcon.Visible = true;
        //
        //_notifyContextMenu
        //
        _notifyContextMenu.ImageScalingSize = new Size(20, 20);
        _notifyContextMenu.Items.AddRange(new ToolStripItem[] { _notifyToggleTopMostMenuItem, _notifyExitMenuItem });
        _notifyContextMenu.Name = "_notifyContextMenu";
        _notifyContextMenu.Size = new Size(158, 48);
        //
        //_notifyToggleTopMostMenuItem
        //
        _notifyToggleTopMostMenuItem.Checked = true;
        _notifyToggleTopMostMenuItem.CheckOnClick = true;
        _notifyToggleTopMostMenuItem.Name = "_notifyToggleTopMostMenuItem";
        _notifyToggleTopMostMenuItem.Size = new Size(157, 22);
        _notifyToggleTopMostMenuItem.Text = "Always on Top";
        //
        //_notifyExitMenuItem
        //
        _notifyExitMenuItem.Name = "_notifyExitMenuItem";
        _notifyExitMenuItem.Size = new Size(157, 22);
        _notifyExitMenuItem.Text = "Exit";
        //
        //_toolTip
        //
        _toolTip.AutomaticDelay = 150;
        _toolTip.AutoPopDelay = 5000;
        _toolTip.InitialDelay = 150;
        _toolTip.ReshowDelay = 30;
        //
        //MainForm
        //
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.Magenta;
        ClientSize = new Size(310, 312);
        Controls.Add(_characterPicture);
        Controls.Add(_bubblePanel);
        DoubleBuffered = true;
        FormBorderStyle = FormBorderStyle.None;
        Margin = new Padding(0);
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "MainForm";
        ShowIcon = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.Manual;
        Text = "otak-agent";
        TopMost = true;
        TransparencyKey = Color.Magenta;
        _bubblePanel.ResumeLayout(false);
        _bubblePanel.PerformLayout();
        ResumeLayout(false);
    }

    private static void ApplyLegacyButtonStyle(Button button)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.FlatAppearance.BorderColor = ButtonBorderColor;
        button.FlatAppearance.MouseOverBackColor = ButtonHoverColor;
        button.FlatAppearance.MouseDownBackColor = ButtonPressedColor;
        button.BackColor = ButtonBaseColor;
        button.ForeColor = Color.Black;
        button.UseVisualStyleBackColor = false;
        button.Margin = new Padding(0);
        button.Font = new Font("MS PGothic", 9F, FontStyle.Regular, GraphicsUnit.Point);
    }
}



