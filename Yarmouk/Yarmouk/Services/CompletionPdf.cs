using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Yarmouk.Services
{
    public static class CompletionPdf
    {
        // GDI+ shapes both Arabic and Latin names using the server's Windows fonts.
        // Embed the rendered A4 page in a standard PDF so no browser print plug-in
        // or external executable is needed on the web server.
        public static byte[] Create(JObject report)
        {
            byte[] jpeg;
            using (var image = new Bitmap(1190, 1684))
            using (var g = Graphics.FromImage(image))
            {
                g.Clear(Color.White);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                var green = Color.FromArgb(9, 91, 68);
                using (var brush = new SolidBrush(green)) g.FillRectangle(brush, 0, 0, 1190, 265);
                Draw(g, "YARMOUK UNIVERSITY", 80, 62, 1030, 45, 28, Color.White, true);
                Draw(g, "Faculty of Information Technology and Computer Science", 80, 122, 1030, 50, 18, Color.White);
                Draw(g, "FINAL INTERNSHIP REPORT", 80, 192, 1030, 40, 20, Color.White, true);
                Draw(g, "COMPLETION APPROVED", 80, 316, 1030, 40, 19, green, true);
                Draw(g, Value(report, "studentName"), 80, 375, 1030, 130, 34, Color.FromArgb(25, 45, 37), true);
                Draw(g, "Student number: " + Value(report, "studentNumber"), 80, 510, 1030, 55, 20, Color.DimGray);
                Row(g, "TRAINING PROVIDER", Value(report, "companyName"), 604);
                Row(g, "OPPORTUNITY", Value(report, "opportunityTitle"), 710);
                Row(g, "APPROVED TRAINING HOURS", Value(report, "approvedHours") + " / " + Value(report, "requiredHours") + " required hours", 816);
                using (var brush = new SolidBrush(Color.FromArgb(238, 246, 241))) g.FillRectangle(brush, 80, 960, 1030, 310);
                Draw(g, "Company evaluation (40%)", 112, 995, 665, 50, 22, Color.FromArgb(42, 65, 54));
                Draw(g, Value(report, "companyScore") + " / 100", 800, 995, 280, 50, 22, green, true);
                Draw(g, "Academic evaluation (60%)", 112, 1065, 665, 50, 22, Color.FromArgb(42, 65, 54));
                Draw(g, Value(report, "academicScore") + " / 100", 800, 1065, 280, 50, 22, green, true);
                Draw(g, "FINAL GRADE", 112, 1163, 600, 60, 25, green, true);
                Draw(g, Value(report, "finalScore") + " / 100", 740, 1153, 340, 80, 34, green, true);
                long timestamp;
                string date = long.TryParse(Value(report, "approvedAtUnixMs"), out timestamp)
                    ? DateTimeOffset.FromUnixTimeMilliseconds(timestamp).ToOffset(TimeSpan.FromHours(3)).ToString("dd MMM yyyy", CultureInfo.InvariantCulture) : "Not recorded";
                Draw(g, "Approved by the university coordinator on " + date, 80, 1330, 1030, 70, 20, Color.FromArgb(42, 65, 54));
                Draw(g, "Reference: " + Value(report, "evaluationId"), 80, 1440, 1030, 80, 15, Color.DimGray);
                Draw(g, "Issued through the University Internship Management System", 80, 1570, 1030, 45, 16, Color.DimGray);
                using (var stream = new MemoryStream()) {
                    var encoder = ImageCodecInfo.GetImageEncoders().First(e => e.MimeType == "image/jpeg");
                    using (var parameters = new EncoderParameters(1)) {
                        parameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 95L);
                        image.Save(stream, encoder, parameters);
                    }
                    jpeg = stream.ToArray();
                }
            }
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream, Encoding.ASCII))
            {
                Action<string> write = s => writer.Write(Encoding.ASCII.GetBytes(s));
                var offsets = new long[6];
                write("%PDF-1.4\n");
                offsets[1] = stream.Position; write("1 0 obj << /Type /Catalog /Pages 2 0 R >> endobj\n");
                offsets[2] = stream.Position; write("2 0 obj << /Type /Pages /Kids [3 0 R] /Count 1 >> endobj\n");
                offsets[3] = stream.Position; write("3 0 obj << /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /XObject << /Im0 4 0 R >> >> /Contents 5 0 R >> endobj\n");
                offsets[4] = stream.Position; write("4 0 obj << /Type /XObject /Subtype /Image /Width 1190 /Height 1684 /ColorSpace /DeviceRGB /BitsPerComponent 8 /Filter /DCTDecode /Length " + jpeg.Length + " >> stream\n");
                writer.Write(jpeg); write("\nendstream endobj\n");
                const string content = "q 595 0 0 842 0 0 cm /Im0 Do Q\n";
                offsets[5] = stream.Position; write("5 0 obj << /Length " + content.Length + " >> stream\n" + content + "endstream endobj\n");
                long xref = stream.Position;
                write("xref\n0 6\n0000000000 65535 f \n");
                for (int i = 1; i <= 5; i++) write(offsets[i].ToString("D10", CultureInfo.InvariantCulture) + " 00000 n \n");
                write("trailer << /Size 6 /Root 1 0 R >>\nstartxref\n" + xref + "\n%%EOF\n");
                return stream.ToArray();
            }
        }
        private static string Value(JObject report, string key) { return report[key] == null ? "" : report[key].ToString(); }
        private static void Row(Graphics g, string label, string value, int y) {
            Draw(g, label, 80, y, 1030, 35, 15, Color.DimGray, true);
            Draw(g, value, 80, y + 35, 1030, 65, 23, Color.FromArgb(25, 45, 37));
        }
        private static void Draw(Graphics g, string value, int x, int y, int w, int h, int size, Color color, bool bold = false) {
            using (var font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular, GraphicsUnit.Pixel))
            using (var brush = new SolidBrush(color))
            using (var format = new StringFormat()) {
                format.Trimming = StringTrimming.EllipsisWord;
                if (value.Any(c => c >= '\u0600' && c <= '\u06ff')) format.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
                g.DrawString(value, font, brush, new RectangleF(x, y, w, h), format);
            }
        }
    }
}
