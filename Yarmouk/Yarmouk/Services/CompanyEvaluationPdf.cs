using System;
using System.Globalization;
using System.IO;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using Newtonsoft.Json.Linq;
using Yarmouk.Models;

namespace Yarmouk.Services
{
    public static class CompanyEvaluationPdf
    {
        // The same criteria are used by the company's form and its downloadable copy.
        public static readonly string[] Criteria =
        {
            "His/Her attendance and punctuality",
            "His/Her involvement in social events of the company/establishment",
            "Ability to adapt to the work environment and understand the nature of activities in the company/establishment",
            "His/Her appreciation of security and safety rules and level of commitment to them",
            "Ability to comprehend and respond to the directions of the training supervisor",
            "Ability to understand others’ point of view and communicate his/her own",
            "Ability to fit into the team",
            "His/Her self-initiative and autonomy",
            "Ability to apply gained theoretical knowledge and willingness to learn more",
            "Ability to accomplish given tasks within the specified timetable",
            "His/Her manual skills (assembling, maintenance, installation)",
            "His/Her skills in preparing sketches and engineering plans for a given idea",
            "His/Her computer and programming skills (software and hardware)",
            "His/Her creativity, originality and imagination",
            "Ability to conduct research and efficiently use the available resources",
            "Interest and usefulness of the work for the establishment",
            "His/Her overall quality of work",
            "General quality of the student’s report (contents, syntax, presentation, literature survey)"
        };

        public static byte[] Create(SupervisorStudentProgress student, string evaluationId, JObject record)
        {
            var document = new Document();
            document.Info.Title = "Company final evaluation";
            document.Info.Author = "Yarmouk University";
            var normal = document.Styles[StyleNames.Normal];
            normal.Font.Name = "Arial";
            normal.Font.Size = 10;
            normal.ParagraphFormat.SpaceAfter = Unit.FromPoint(5);
            var section = document.AddSection();
            section.PageSetup.PageFormat = PageFormat.A4;
            section.PageSetup.TopMargin = section.PageSetup.BottomMargin = Unit.FromCentimeter(1.7);
            section.PageSetup.LeftMargin = section.PageSetup.RightMargin = Unit.FromCentimeter(1.8);
            var title = section.AddParagraph("Company final evaluation");
            title.Format.Font.Size = 20;
            title.Format.Font.Bold = true;
            section.AddParagraph("Yarmouk University | Submitted company assessment");
            section.AddParagraph("Student: " + student.StudentName + " | Student ID: " + student.StudentNumber);
            section.AddParagraph("Company: " + student.CompanyName);
            section.AddParagraph("Training period: " + student.TrainingPeriodLabel);
            section.AddParagraph("Evaluation reference: " + evaluationId);
            long submittedAt;
            if (long.TryParse((string)record["companySubmittedAtUnixMs"], out submittedAt)
                && submittedAt >= 0 && submittedAt <= 253402300799999)
                section.AddParagraph("Submitted (UTC): " + DateTimeOffset.FromUnixTimeMilliseconds(submittedAt)
                    .ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture));
            else section.AddParagraph("Submission date: Not recorded");
            var score = section.AddParagraph("Overall company grade: " + ((string)record["companyScore"] ?? "Not recorded") + " / 100");
            score.Format.Font.Bold = true;
            score.Format.SpaceAfter = Unit.FromPoint(12);

            var table = section.AddTable();
            table.Borders.Width = 0.5;
            table.Borders.Color = Colors.LightGray;
            table.TopPadding = table.BottomPadding = Unit.FromPoint(4);
            table.AddColumn(Unit.FromCentimeter(1));
            table.AddColumn(Unit.FromCentimeter(12.5));
            table.AddColumn(Unit.FromCentimeter(3.9));
            var header = table.AddRow();
            header.HeadingFormat = true;
            header.Shading.Color = Colors.LightGray;
            header.Format.Font.Bold = true;
            header.Cells[0].AddParagraph("No.");
            header.Cells[1].AddParagraph("Evaluation criterion");
            header.Cells[2].AddParagraph("Rating");
            var ratings = record["companyCriteria"];
            for (int i = 0; i < Criteria.Length; i++)
            {
                string key = (i + 1).ToString(CultureInfo.InvariantCulture);
                // Firebase can return consecutive numeric keys as an array instead of an object.
                JToken rating = ratings is JObject ? ratings[key]
                    : ratings is JArray && ((JArray)ratings).Count > i + 1 ? ratings[i + 1] : null;
                var row = table.AddRow();
                row.Cells[0].AddParagraph(key);
                row.Cells[1].AddParagraph(Criteria[i]);
                row.Cells[2].AddParagraph((string)rating ?? "Not recorded");
            }
            var observations = section.AddParagraph("Company observations");
            observations.Format.SpaceBefore = Unit.FromPoint(14);
            observations.Format.Font.Bold = true;
            observations.Format.KeepWithNext = true;
            string feedback = (string)record["companyFeedback"] ?? (string)record["companyComments"];
            section.AddParagraph(string.IsNullOrWhiteSpace(feedback) ? "No observations provided." : feedback);
            var footer = section.Footers.Primary.AddParagraph();
            footer.Format.Font.Size = 8;
            footer.AddText("Company evaluation | Page ");
            footer.AddPageField();
            footer.AddText(" of ");
            footer.AddNumPagesField();

            var renderer = new PdfDocumentRenderer(true) { Document = document };
            renderer.RenderDocument();
            using (var stream = new MemoryStream())
            {
                renderer.PdfDocument.Save(stream, false);
                return stream.ToArray();
            }
        }
    }
}
