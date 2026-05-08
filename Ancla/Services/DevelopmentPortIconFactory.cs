using System.Drawing.Drawing2D;

namespace Ancla.Services;

public static class DevelopmentPortIconFactory
{
    private static readonly Dictionary<string, Image> Cache = new(StringComparer.OrdinalIgnoreCase);

    public static Image Get(string profileKey)
    {
        if (Cache.TryGetValue(profileKey, out var image))
        {
            return image;
        }

        var created = Create(profileKey);
        Cache[profileKey] = created;
        return created;
    }

    private static Image Create(string profileKey)
    {
        var bitmap = new Bitmap(20, 20);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.Clear(Color.Transparent);

        switch (profileKey.ToLowerInvariant())
        {
            case "docker":
                DrawDocker(graphics);
                break;
            case "react":
                DrawReact(graphics);
                break;
            case "sails":
                DrawSails(graphics);
                break;
            case "vue":
                DrawVue(graphics);
                break;
            case "angular":
                DrawBadge(graphics, Color.FromArgb(221, 36, 62), "A");
                break;
            case "ant-design":
                DrawBadge(graphics, Color.FromArgb(25, 137, 250), "AD");
                break;
            case "storybook":
                DrawBadge(graphics, Color.FromArgb(255, 71, 133), "SB");
                break;
            case "aspnet":
                DrawBadge(graphics, Color.FromArgb(82, 45, 128), ".N");
                break;
            default:
                DrawBadge(graphics, Color.FromArgb(89, 97, 111), "D");
                break;
        }

        return bitmap;
    }

    private static void DrawBadge(Graphics graphics, Color backgroundColor, string text)
    {
        using var backgroundBrush = new SolidBrush(backgroundColor);
        using var textBrush = new SolidBrush(Color.White);
        using var font = new Font("Segoe UI Semibold", text.Length > 1 ? 6.2F : 8F, FontStyle.Bold, GraphicsUnit.Point);
        using var path = RoundedRect(new RectangleF(1, 1, 18, 18), 5);

        graphics.FillPath(backgroundBrush, path);

        var format = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };

        graphics.DrawString(text, font, textBrush, new RectangleF(1, 1, 18, 18), format);
    }

    private static void DrawReact(Graphics graphics)
    {
        using var pen = new Pen(Color.FromArgb(97, 218, 251), 1.4F);
        using var centerBrush = new SolidBrush(Color.FromArgb(97, 218, 251));

        graphics.TranslateTransform(10, 10);
        for (var index = 0; index < 3; index++)
        {
            graphics.RotateTransform(60);
            graphics.DrawEllipse(pen, -7.5F, -3F, 15F, 6F);
        }

        graphics.FillEllipse(centerBrush, -2.2F, -2.2F, 4.4F, 4.4F);
        graphics.ResetTransform();
    }

    private static void DrawVue(Graphics graphics)
    {
        using var darkBrush = new SolidBrush(Color.FromArgb(53, 73, 94));
        using var greenBrush = new SolidBrush(Color.FromArgb(65, 184, 131));

        Point[] outer =
        [
            new(2, 3),
            new(7, 3),
            new(10, 8),
            new(13, 3),
            new(18, 3),
            new(10, 17)
        ];

        Point[] inner =
        [
            new(5, 3),
            new(8, 3),
            new(10, 7),
            new(12, 3),
            new(15, 3),
            new(10, 12)
        ];

        graphics.FillPolygon(darkBrush, outer);
        graphics.FillPolygon(greenBrush, inner);
    }

    private static void DrawDocker(Graphics graphics)
    {
        using var blueBrush = new SolidBrush(Color.FromArgb(36, 150, 237));
        using var whiteBrush = new SolidBrush(Color.White);

        graphics.FillRectangle(blueBrush, 3, 9, 3, 3);
        graphics.FillRectangle(blueBrush, 7, 9, 3, 3);
        graphics.FillRectangle(blueBrush, 11, 9, 3, 3);
        graphics.FillRectangle(blueBrush, 7, 5, 3, 3);
        graphics.FillRectangle(blueBrush, 11, 5, 3, 3);
        graphics.FillRectangle(blueBrush, 15, 9, 2, 3);
        graphics.FillRectangle(blueBrush, 4, 13, 11, 2);

        Point[] whale =
        [
            new(4, 14),
            new(14, 14),
            new(17, 12),
            new(17, 15),
            new(15, 17),
            new(7, 17),
            new(5, 16)
        ];

        graphics.FillPolygon(blueBrush, whale);
        graphics.FillEllipse(whiteBrush, 13.4F, 14.3F, 1.8F, 1.8F);
    }

    private static void DrawSails(Graphics graphics)
    {
        using var seaBrush = new SolidBrush(Color.FromArgb(36, 123, 214));
        using var hullBrush = new SolidBrush(Color.FromArgb(33, 41, 59));
        using var sailBrush = new SolidBrush(Color.FromArgb(250, 250, 252));
        using var mastPen = new Pen(Color.FromArgb(33, 41, 59), 1.4F);

        graphics.FillRectangle(seaBrush, 2, 15, 16, 2);
        graphics.DrawLine(mastPen, 9, 4, 9, 15);

        Point[] mainSail =
        [
            new(9, 5),
            new(9, 13),
            new(4, 12)
        ];

        Point[] frontSail =
        [
            new(10, 7),
            new(14, 12),
            new(10, 12)
        ];

        Point[] hull =
        [
            new(4, 13),
            new(15, 13),
            new(13, 15),
            new(6, 15)
        ];

        graphics.FillPolygon(sailBrush, mainSail);
        graphics.FillPolygon(sailBrush, frontSail);
        graphics.FillPolygon(hullBrush, hull);
    }

    private static GraphicsPath RoundedRect(RectangleF bounds, float radius)
    {
        var diameter = radius * 2;
        var path = new GraphicsPath();
        path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
        path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
        path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();
        return path;
    }
}
