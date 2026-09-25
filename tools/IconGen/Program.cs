using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace IconGen;

// 从仓库根目录运行：dotnet run --project tools/IconGen
// 生成 assets/app.ico（多尺寸）与 assets/app.png（256px 预览）
internal static class Program
{
    private static readonly int[] Sizes = { 16, 24, 32, 48, 64, 128, 256 };

    [STAThread]
    private static int Main(string[] args)
    {
        var outputPath = args.Length > 0 ? args[0] : Path.Combine("assets", "app.ico");
        var outputDir = Path.GetDirectoryName(Path.GetFullPath(outputPath))!;
        Directory.CreateDirectory(outputDir);

        var frames = Sizes.Select(RenderPng).ToArray();
        WriteIco(outputPath, frames);

        var pngPath = Path.Combine(outputDir, "app.png");
        File.WriteAllBytes(pngPath, frames[^1]);

        Console.WriteLine($"已生成 {Path.GetFullPath(outputPath)}");
        Console.WriteLine($"已生成 {pngPath}");
        return 0;
    }

    private static byte[] RenderPng(int size)
    {
        var visual = new DrawingVisual();
        using (var dc = visual.RenderOpen())
        {
            dc.PushTransform(new ScaleTransform(size / 256.0, size / 256.0));
            DrawIcon(dc);
            dc.Pop();
        }

        var bitmap = new RenderTargetBitmap(size, size, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(visual);

        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var stream = new MemoryStream();
        encoder.Save(stream);
        return stream.ToArray();
    }

    // 在 256x256 坐标系中绘制：蓝紫渐变圆角底 + 白色折角文件 + 指向文件的光标
    private static void DrawIcon(DrawingContext dc)
    {
        var background = new LinearGradientBrush(
            Color.FromRgb(0x63, 0x66, 0xF1),
            Color.FromRgb(0x43, 0x38, 0xCA),
            new Point(0, 0), new Point(1, 1));
        dc.DrawRoundedRectangle(background, null, new Rect(0, 0, 256, 256), 48, 48);

        var document = new StreamGeometry();
        using (var ctx = document.Open())
        {
            ctx.BeginFigure(new Point(72, 44), true, true);
            ctx.LineTo(new Point(148, 44), true, false);
            ctx.LineTo(new Point(184, 80), true, false);
            ctx.LineTo(new Point(184, 212), true, false);
            ctx.LineTo(new Point(72, 212), true, false);
        }
        document.Freeze();
        dc.DrawGeometry(Brushes.White, null, document);

        var fold = new StreamGeometry();
        using (var ctx = fold.Open())
        {
            ctx.BeginFigure(new Point(148, 44), true, true);
            ctx.LineTo(new Point(184, 80), true, false);
            ctx.LineTo(new Point(148, 80), true, false);
        }
        fold.Freeze();
        dc.DrawGeometry(new SolidColorBrush(Color.FromRgb(0xC7, 0xD2, 0xFE)), null, fold);

        var lineBrush = new SolidColorBrush(Color.FromRgb(0xA5, 0xB4, 0xFC));
        dc.DrawRoundedRectangle(lineBrush, null, new Rect(88, 100, 80, 12), 6, 6);
        dc.DrawRoundedRectangle(lineBrush, null, new Rect(88, 124, 80, 12), 6, 6);
        dc.DrawRoundedRectangle(lineBrush, null, new Rect(88, 148, 52, 12), 6, 6);

        var cursor = new StreamGeometry();
        using (var ctx = cursor.Open())
        {
            ctx.BeginFigure(new Point(0, 0), true, true);
            ctx.LineTo(new Point(0, 58), true, false);
            ctx.LineTo(new Point(14, 45), true, false);
            ctx.LineTo(new Point(23, 68), true, false);
            ctx.LineTo(new Point(32, 65), true, false);
            ctx.LineTo(new Point(23, 42), true, false);
            ctx.LineTo(new Point(40, 42), true, false);
        }
        cursor.Freeze();

        dc.PushTransform(new TranslateTransform(150, 140));
        dc.DrawGeometry(
            new SolidColorBrush(Color.FromRgb(0x11, 0x18, 0x27)),
            new Pen(Brushes.White, 6) { LineJoin = PenLineJoin.Round },
            cursor);
        dc.Pop();
    }

    private static void WriteIco(string path, byte[][] frames)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        writer.Write((ushort)0);
        writer.Write((ushort)1);
        writer.Write((ushort)frames.Length);

        var offset = 6 + frames.Length * 16;
        foreach (var (frame, size) in frames.Zip(Sizes))
        {
            writer.Write((byte)(size >= 256 ? 0 : size));
            writer.Write((byte)(size >= 256 ? 0 : size));
            writer.Write((byte)0);
            writer.Write((byte)0);
            writer.Write((ushort)1);
            writer.Write((ushort)32);
            writer.Write((uint)frame.Length);
            writer.Write((uint)offset);
            offset += frame.Length;
        }

        foreach (var frame in frames)
        {
            writer.Write(frame);
        }

        File.WriteAllBytes(path, stream.ToArray());
    }
}
