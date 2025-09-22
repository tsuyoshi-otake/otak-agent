using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace OtakAgent.App.Controls;

internal sealed class CharacterCanvas : Control
{
    private static readonly Color TransparentFillColor = Color.Magenta;
    private static readonly Color MagentaColorKey = Color.Magenta;

    private Image? _image;
    private bool _isAnimating;

    public CharacterCanvas()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.UserPaint |
                 ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.ResizeRedraw |
                 ControlStyles.SupportsTransparentBackColor, true);
        BackColor = TransparentFillColor;
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Image? Image
    {
        get => _image;
        set
        {
            if (ReferenceEquals(_image, value))
            {
                return;
            }

            DetachAnimation();
            _image = value;

            if (_image is not null && ImageAnimator.CanAnimate(_image))
            {
                ImageAnimator.Animate(_image, OnFrameChanged);
                _isAnimating = true;
            }

            Invalidate();
        }
    }

    protected override void OnPaintBackground(PaintEventArgs pevent)
    {
        var graphics = pevent.Graphics;
        graphics.Clear(TransparentFillColor);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        var image = _image;
        if (image is null || Width <= 0 || Height <= 0)
        {
            return;
        }

        if (_isAnimating)
        {
            ImageAnimator.UpdateFrames(image);
        }

        var graphics = e.Graphics;
        graphics.CompositingMode = CompositingMode.SourceOver;
        graphics.CompositingQuality = CompositingQuality.HighQuality;
        graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

        using var attributes = new ImageAttributes();
        attributes.SetColorKey(MagentaColorKey, MagentaColorKey);

        var destination = CalculateDestinationRectangle(image);
        graphics.DrawImage(image, destination, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            DetachAnimation();
            _image = null;
        }

        base.Dispose(disposing);
    }

    private void DetachAnimation()
    {
        if (_image is not null && _isAnimating)
        {
            ImageAnimator.StopAnimate(_image, OnFrameChanged);
            _isAnimating = false;
        }
    }

    private void OnFrameChanged(object? sender, EventArgs e)
    {
        Invalidate();
    }

    private Rectangle CalculateDestinationRectangle(Image image)
    {
        var controlWidth = Width;
        var controlHeight = Height;
        if (controlWidth <= 0 || controlHeight <= 0)
        {
            return Rectangle.Empty;
        }

        var imageAspect = (float)image.Width / image.Height;
        var controlAspect = (float)controlWidth / controlHeight;

        int width;
        int height;

        if (controlAspect > imageAspect)
        {
            height = controlHeight;
            width = (int)Math.Round(height * imageAspect);
        }
        else
        {
            width = controlWidth;
            height = (int)Math.Round(width / imageAspect);
        }

        width = Math.Max(1, width);
        height = Math.Max(1, height);

        var offsetX = (controlWidth - width) / 2;
        var offsetY = (controlHeight - height) / 2;

        return new Rectangle(offsetX, offsetY, width, height);
    }
}
