using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Text.RegularExpressions;
using MigraDoc.DocumentObjectModel;

namespace Yarmouk.Helpers
{
    public static class PdfText
    {
        // MigraDoc 1.50 does not shape Arabic. Windows GDI+ performs shaping
        // and bidi layout; embed the rendered run so every PDF viewer agrees.
        public static void Add(Paragraph paragraph, string text, double size = 10, bool bold = false, double width = 450)
        {
            text = text ?? string.Empty;
            if (!Regex.IsMatch(text, @"[\u0600-\u06ff\u0750-\u077f\u08a0-\u08ff]"))
            { paragraph.AddText(text); return; }
            var firstLetter = Regex.Match(text, @"[A-Za-z\u0600-\u06ff\u0750-\u077f\u08a0-\u08ff]");
            bool rightToLeft = firstLetter.Success && firstLetter.Value[0] > 127;
            const float scale = 4;
            using (var font = new System.Drawing.Font("Arial", (float)size * scale,
                bold ? FontStyle.Bold : FontStyle.Regular, GraphicsUnit.Pixel))
            using (var format = new StringFormat(rightToLeft ? StringFormatFlags.DirectionRightToLeft : (StringFormatFlags)0))
            using (var measure = new Bitmap(1, 1))
            using (var graphics = Graphics.FromImage(measure))
            {
                var bounds = graphics.MeasureString(text, font, (int)(width * scale), format);
                int pixelsWide = Math.Max(1, (int)Math.Ceiling(bounds.Width) + 12);
                int pixelsHigh = Math.Max(1, (int)Math.Ceiling(bounds.Height) + 12);
                using (var bitmap = new Bitmap(pixelsWide, pixelsHigh))
                using (var canvas = Graphics.FromImage(bitmap))
                using (var stream = new MemoryStream())
                {
                    canvas.Clear(System.Drawing.Color.White);
                    canvas.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                    canvas.DrawString(text, font, Brushes.Black,
                        new RectangleF(6, 6, pixelsWide - 12, pixelsHigh - 12), format);
                    bitmap.Save(stream, ImageFormat.Png);
                    var image = paragraph.AddImage("base64:" + Convert.ToBase64String(stream.ToArray()));
                    image.Width = Unit.FromPoint(pixelsWide / scale);
                    image.Height = Unit.FromPoint(pixelsHigh / scale);
                }
            }
        }
    }
}
