using System;
using System.Globalization;
using System.IO;
using System.Web;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using Newtonsoft.Json.Linq;

namespace Yarmouk.Services
{
    public static class CompletionCertificatePdf
    {
        public static byte[] Create(string studentName, string studentNumber, string companyName, string opportunityTitle, string reference, JObject evaluation)
        {
            var document = new Document();
            document.Info.Title = "Official internship completion certificate";
            document.Info.Author = "Yarmouk University";
            document.Styles[StyleNames.Normal].Font.Name = "Arial";
            document.Styles[StyleNames.Normal].Font.Size = 10.5;
            document.Styles[StyleNames.Normal].ParagraphFormat.SpaceAfter = Unit.FromPoint(7);

            var section = document.AddSection();
            section.PageSetup.PageFormat = PageFormat.A4;
            section.PageSetup.TopMargin = Unit.FromCentimeter(2.15);
            section.PageSetup.BottomMargin = Unit.FromCentimeter(2);
            section.PageSetup.LeftMargin = section.PageSetup.RightMargin = Unit.FromCentimeter(2.1);

            AddUniversityHeader(section);
            var rule = section.AddParagraph();
            rule.Format.Borders.Bottom.Color = Colors.Goldenrod;
            rule.Format.Borders.Bottom.Width = Unit.FromPoint(1.5);
            rule.Format.SpaceBefore = Unit.FromPoint(8);
            rule.Format.SpaceAfter = Unit.FromPoint(18);

            var title = section.AddParagraph("Internship Completion Certificate");
            Center(title, 24, true, Colors.DarkSlateGray);
            title.Format.SpaceAfter = Unit.FromPoint(15);
            Center(section.AddParagraph("This is to certify that"), 10.5, false, Colors.DimGray);
            var student = section.AddParagraph(Safe(studentName, "Student"));
            Center(student, 20, true, Colors.DarkGreen);
            student.Format.SpaceBefore = Unit.FromPoint(3);
            student.Format.SpaceAfter = Unit.FromPoint(3);
            var identifier = section.AddParagraph("Student ID · " + Safe(studentNumber, "Not recorded"));
            Center(identifier, 9.5, false, Colors.DimGray);
            identifier.Format.SpaceAfter = Unit.FromPoint(16);
            var completion = section.AddParagraph("has successfully completed the field-training requirements with " + Safe(companyName, "the assigned training provider") + ".");
            Center(completion, 11, false, Colors.DarkSlateGray);
            if (!string.IsNullOrWhiteSpace(opportunityTitle)) Center(section.AddParagraph("Training opportunity · " + opportunityTitle), 9.5, false, Colors.DimGray);

            var scoreTable = section.AddTable();
            scoreTable.AddColumn(Unit.FromCentimeter(16.8));
            scoreTable.Format.Alignment = ParagraphAlignment.Center;
            scoreTable.TopPadding = scoreTable.BottomPadding = Unit.FromPoint(10);
            scoreTable.Borders.Color = Colors.DarkSeaGreen;
            scoreTable.Borders.Width = Unit.FromPoint(0.8);
            var scoreRow = scoreTable.AddRow();
            scoreRow.Shading.Color = Colors.Honeydew;
            Center(scoreRow.Cells[0].AddParagraph("OVERALL FINAL AVERAGE"), 9, true, Colors.DarkOliveGreen);
            var score = scoreRow.Cells[0].AddParagraph(Average(evaluation));
            Center(score, 25, true, Colors.DarkGreen);
            score.Format.SpaceAfter = Unit.FromPoint(0);

            section.AddParagraph().Format.SpaceAfter = Unit.FromPoint(4);
            var details = section.AddTable();
            details.AddColumn(Unit.FromCentimeter(5.6));
            details.AddColumn(Unit.FromCentimeter(5.6));
            details.AddColumn(Unit.FromCentimeter(5.6));
            details.Borders.Color = Colors.LightGray;
            details.Borders.Width = Unit.FromPoint(0.6);
            details.TopPadding = details.BottomPadding = Unit.FromPoint(7);
            var row = details.AddRow();
            AddDetail(row, 0, "TRAINING PROVIDER", Safe(companyName, "Not recorded"));
            AddDetail(row, 1, "APPROVED ON", FormatDate(Value(evaluation, "coordinatorApprovedAtUnixMs", string.Empty)));
            AddDetail(row, 2, "STATUS", "Officially approved");

            var statement = section.AddParagraph("This certificate confirms that the final company and academic evaluations have been reviewed and approved by the university coordinator.");
            Center(statement, 9, false, Colors.DimGray);
            statement.Format.SpaceBefore = Unit.FromPoint(22);
            AddValidationMarks(section);
            var footer = section.Footers.Primary.AddParagraph("Yarmouk University  |  Official internship completion record  |  Certificate reference · " + Safe(reference, "Not recorded") + "  |  Page ");
            Center(footer, 8, false, Colors.DimGray);
            footer.AddPageField(); footer.AddText(" of "); footer.AddNumPagesField();

            var renderer = new PdfDocumentRenderer(true) { Document = document };
            renderer.RenderDocument();
            using (var stream = new MemoryStream()) { renderer.PdfDocument.Save(stream, false); return stream.ToArray(); }
        }

        private static void AddUniversityHeader(Section section)
        {
            var header = section.AddTable();
            header.AddColumn(Unit.FromCentimeter(3));
            header.AddColumn(Unit.FromCentimeter(10.8));
            header.AddColumn(Unit.FromCentimeter(3));
            header.Borders.Bottom.Color = Colors.Goldenrod;
            header.Borders.Bottom.Width = Unit.FromPoint(1.5);
            var row = header.AddRow();
            string logoPath = HttpContext.Current == null ? string.Empty : HttpContext.Current.Server.MapPath("~/Content/Images/yarmouk-college-logo.png");
            if (!string.IsNullOrWhiteSpace(logoPath) && File.Exists(logoPath))
            {
                var logo = row.Cells[0].AddImage(logoPath);
                logo.LockAspectRatio = true;
                logo.Width = Unit.FromCentimeter(2.25);
            }
            Center(row.Cells[1].AddParagraph("YARMOUK UNIVERSITY"), 12, true, Colors.DarkGreen);
            Center(row.Cells[1].AddParagraph("Faculty of Information Technology and Computer Science"), 9, false, Colors.DimGray);
            var serial = row.Cells[2].AddParagraph("OFFICIAL\nCERTIFICATE");
            serial.Format.Alignment = ParagraphAlignment.Right;
            serial.Format.Font.Size = 7.5;
            serial.Format.Font.Bold = true;
            serial.Format.Font.Color = Colors.DarkOliveGreen;
            header.BottomPadding = Unit.FromPoint(8);
            section.AddParagraph().Format.SpaceAfter = Unit.FromPoint(10);
        }

        private static void AddValidationMarks(Section section)
        {
            var marks = section.AddTable();
            marks.AddColumn(Unit.FromCentimeter(6));
            marks.AddColumn(Unit.FromCentimeter(4.8));
            marks.AddColumn(Unit.FromCentimeter(6));
            marks.TopPadding = marks.BottomPadding = Unit.FromPoint(4);
            var row = marks.AddRow();
            var signature = row.Cells[0].AddParagraph("/s/ University Coordinator\n____________________________\nCoordinator approval");
            signature.Format.Font.Name = "Segoe Script";
            signature.Format.Font.Size = 9;
            signature.Format.Font.Color = Colors.DarkSlateGray;
            var sealIcon = row.Cells[1].AddParagraph("◉");
            sealIcon.Format.Alignment = ParagraphAlignment.Center;
            sealIcon.Format.Font.Bold = true;
            sealIcon.Format.Font.Size = 28;
            sealIcon.Format.Font.Color = Colors.DarkGreen;
            sealIcon.Format.SpaceAfter = Unit.FromPoint(0);
            var seal = row.Cells[1].AddParagraph("YARMOUK UNIVERSITY\nOFFICIAL SEAL");
            seal.Format.Alignment = ParagraphAlignment.Center;
            seal.Format.Font.Bold = true;
            seal.Format.Font.Size = 7.5;
            seal.Format.Font.Color = Colors.DarkGreen;
            var record = row.Cells[2].AddParagraph("Verified completion record\nYarmouk Internship Management System");
            record.Format.Alignment = ParagraphAlignment.Right;
            record.Format.Font.Size = 8;
            record.Format.Font.Color = Colors.DimGray;
        }

        private static void Center(Paragraph paragraph, double size, bool bold, Color color)
        {
            paragraph.Format.Alignment = ParagraphAlignment.Center;
            paragraph.Format.Font.Size = size;
            paragraph.Format.Font.Bold = bold;
            paragraph.Format.Font.Color = color;
        }
        private static void AddDetail(Row row, int index, string label, string value)
        {
            Center(row.Cells[index].AddParagraph(label), 7.5, true, Colors.DimGray);
            var content = row.Cells[index].AddParagraph(value);
            Center(content, 9, true, Colors.DarkSlateGray);
            content.Format.SpaceAfter = Unit.FromPoint(0);
        }
        private static string Average(JObject record)
        {
            double company, academic;
            return double.TryParse(Value(record, "companyScore", string.Empty), NumberStyles.Number, CultureInfo.InvariantCulture, out company)
                && double.TryParse(Value(record, "academicScore", string.Empty), NumberStyles.Number, CultureInfo.InvariantCulture, out academic)
                ? Math.Round((company + academic) / 2d, 2).ToString("0.##", CultureInfo.InvariantCulture) + " / 100" : "Not recorded";
        }
        private static string Value(JObject record, string key, string fallback) { return record == null || record[key] == null || string.IsNullOrWhiteSpace(record[key].ToString()) ? fallback : record[key].ToString(); }
        private static string Safe(string value, string fallback) { return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim(); }
        private static string FormatDate(string value)
        {
            long milliseconds;
            return long.TryParse(value, out milliseconds) && milliseconds > 0 && milliseconds <= 253402300799999 ? DateTimeOffset.FromUnixTimeMilliseconds(milliseconds).ToString("dd MMMM yyyy", CultureInfo.InvariantCulture) : "Recorded by Yarmouk University";
        }
    }
}
