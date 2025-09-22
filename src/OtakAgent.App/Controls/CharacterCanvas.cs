using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.ComponentModel;
using System.Windows.Forms;

namespace OtakAgent.App.Controls;

internal sealed class CharacterCanvas : Control
{
    private Image? _image;

    public CharacterCanvas()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.UserPaint |
                 ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.ResizeRedraw |
                 ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.Transparent;
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

            _image = value;
            Invalidate();
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        var image = _image;
        if (image is null || Width <= 0 || Height <= 0)
        {
            return;
        }

        var graphics = e.Graphics;
        graphics.Clear(Color.Transparent);
        graphics.CompositingMode = CompositingMode.SourceOver;
        graphics.CompositingQuality = CompositingQuality.HighQuality;
        graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

        var destination = CalculateDestinationRectangle(image);
        graphics.DrawImage(image, destination);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _image?.Dispose();
            _image = null;
        }

        base.Dispose(disposing);
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



