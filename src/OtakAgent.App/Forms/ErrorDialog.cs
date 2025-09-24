using System;
using System.Drawing;
using System.Windows.Forms;

namespace OtakAgent.App.Forms
{
    public class ErrorDialog : Form
    {
        private readonly TextBox _errorTextBox;
        private readonly Button _copyButton;
        private readonly Button _okButton;
        private readonly Panel _buttonPanel;

        public ErrorDialog(string title, string message, MessageBoxIcon icon = MessageBoxIcon.Error)
        {
            Text = title;
            ClientSize = new Size(450, 180);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            // Icon
            var iconPictureBox = new PictureBox
            {
                Location = new Point(15, 15),
                Size = new Size(32, 32),
                SizeMode = PictureBoxSizeMode.StretchImage
            };

            if (icon == MessageBoxIcon.Error)
                iconPictureBox.Image = SystemIcons.Error.ToBitmap();
            else if (icon == MessageBoxIcon.Warning)
                iconPictureBox.Image = SystemIcons.Warning.ToBitmap();
            else if (icon == MessageBoxIcon.Information)
                iconPictureBox.Image = SystemIcons.Information.ToBitmap();

            // Error message text box
            _errorTextBox = new TextBox
            {
                Location = new Point(60, 15),
                Size = new Size(375, 100),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Text = message,
                BackColor = SystemColors.Window,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Button panel for proper button positioning
            _buttonPanel = new Panel
            {
                Location = new Point(0, 125),
                Size = new Size(450, 55),
                BackColor = SystemColors.Control
            };

            // OK button (primary button, on the right)
            _okButton = new Button
            {
                Text = "OK",
                Location = new Point(355, 15),
                Size = new Size(80, 25),
                TabIndex = 0,
                DialogResult = DialogResult.OK,
                UseVisualStyleBackColor = true
            };

            // Copy button (secondary button, to the left of OK)
            _copyButton = new Button
            {
                Text = "Copy",
                Location = new Point(265, 15),
                Size = new Size(80, 25),
                TabIndex = 1,
                UseVisualStyleBackColor = true
            };
            _copyButton.Click += CopyButton_Click;

            // Add controls to button panel
            _buttonPanel.Controls.Add(_copyButton);
            _buttonPanel.Controls.Add(_okButton);

            // Add controls
            Controls.Add(iconPictureBox);
            Controls.Add(_errorTextBox);
            Controls.Add(_buttonPanel);

            AcceptButton = _okButton;
        }

        private void CopyButton_Click(object? sender, EventArgs e)
        {
            try
            {
                Clipboard.SetText(_errorTextBox.Text);
                _copyButton.Text = "Copied!";

                // Reset button text after 2 seconds
                var timer = new System.Windows.Forms.Timer { Interval = 2000 };
                timer.Tick += (s, args) =>
                {
                    _copyButton.Text = "Copy";
                    timer.Stop();
                    timer.Dispose();
                };
                timer.Start();
            }
            catch
            {
                // Silently fail if clipboard operation fails
            }
        }

        public static void ShowError(IWin32Window owner, string message, string title = "otak-agent")
        {
            using var dialog = new ErrorDialog(title, message, MessageBoxIcon.Error);
            dialog.ShowDialog(owner);
        }

        public static void ShowWarning(IWin32Window owner, string message, string title = "otak-agent")
        {
            using var dialog = new ErrorDialog(title, message, MessageBoxIcon.Warning);
            dialog.ShowDialog(owner);
        }
    }
}