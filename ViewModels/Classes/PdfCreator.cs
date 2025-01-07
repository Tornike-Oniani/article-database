using Lib.DataAccessLayer.Models;
using MainLib.ViewModels.Utils;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MainLib.ViewModels.Classes
{
    public class PdfCreator
    {
        private readonly Shared services = Shared.GetInstance();
        private readonly string diaogSavedPath = String.Empty;
        private Document document;
        private readonly Unit spaceAfterTitle = Unit.FromCentimeter(0.4);
        private readonly Unit spaceAfterRegular = Unit.FromCentimeter(0.2);
        private readonly Unit spaceAfterParagraph = Unit.FromCentimeter(1);
        private readonly Unit keywordIndentation = Unit.FromCentimeter(0.4);
        private readonly int titleFontSize = 16;
        private readonly int regularFontSize = 12;

        public PdfCreator()
        {
            InitializeDocument();
        }

        public async Task Print(List<Article> articles)
        {
            string path = Shared.GetInstance().BrowserService.OpenSaveFileDialog
                (
                    filter: "PDF files (*.pdf)|*.pdf",
                    defaultEx: "pdf",
                    fileName: "report.pdf",
                    savedPath: this.diaogSavedPath
                );

            if (String.IsNullOrEmpty(path))
            {
                return;
            }

            services.IsWorking(true);

            await Task.Run(() =>
            {
                Section section = this.document.AddSection();

                foreach (Article article in articles)
                {
                    WriteArticle(article, section);
                }

                PdfDocumentRenderer documentRenderer = new PdfDocumentRenderer();
                documentRenderer.Document = this.document.Clone();
                documentRenderer.RenderDocument();

                documentRenderer.Save(path);
            });

            InitializeDocument();

            services.IsWorking(false);
        }

        private void InitializeDocument()
        {
            this.document = new Document();
            // Title
            Style titleStyle = document.Styles["Heading1"];
            titleStyle.Font.Size = this.titleFontSize;
            titleStyle.Font.Bold = true;
            titleStyle.ParagraphFormat.SpaceAfter = this.spaceAfterTitle;
            titleStyle.ParagraphFormat.KeepTogether = true;
            titleStyle.ParagraphFormat.KeepWithNext = true;
            // Year
            Style yearStyle = document.Styles.AddStyle("Year", "Normal");
            yearStyle.Font.Size = this.regularFontSize;
            yearStyle.Font.Color = Colors.Gray;
            yearStyle.ParagraphFormat.SpaceAfter = this.spaceAfterRegular;
            yearStyle.ParagraphFormat.KeepTogether = true;
            yearStyle.ParagraphFormat.KeepWithNext = true;
            // Keyword
            Style keywordStyle = document.Styles.AddStyle("Keyword", "Normal");
            keywordStyle.Font.Size = this.regularFontSize;
            keywordStyle.Font.Italic = true;
            keywordStyle.ParagraphFormat.SpaceAfter = this.spaceAfterRegular;
            keywordStyle.ParagraphFormat.LeftIndent = this.keywordIndentation;
            keywordStyle.ParagraphFormat.KeepTogether = true;
            keywordStyle.ParagraphFormat.KeepWithNext = true;
            // Authors
            Style authorStyle = document.Styles.AddStyle("Author", "Normal");
            authorStyle.Font.Size = this.regularFontSize;
            authorStyle.ParagraphFormat.SpaceAfter = this.spaceAfterRegular;
            authorStyle.ParagraphFormat.KeepTogether = true;
            // Abstarct
            Style abstractStyle = document.Styles.AddStyle("Abstract", "Normal");
            abstractStyle.Font.Size = this.regularFontSize;
            abstractStyle.ParagraphFormat.Alignment = ParagraphAlignment.Justify;
            // Space
            Style spaceStyle = document.Styles.AddStyle("Space", "Normal");
            spaceStyle.ParagraphFormat.SpaceAfter = this.spaceAfterParagraph;
        }

        private void WriteArticle(Article article, Section section)
        {
            section.AddParagraph(article.Title, "Heading1");
            string year = article.Year.ToString();
            if (services.User.IsAdmin)
            {
                Console.WriteLine("Yo");
                // Create a table with two columns
                var table = section.AddTable();
                table.Borders.Visible = false; // Hide table borders
                table.Style = "Year";

                // Add two columns
                var column1 = table.AddColumn(Unit.FromCentimeter(8)); // Dynamically stretch column widths as needed
                var column2 = table.AddColumn(Unit.FromCentimeter(8));

                // Align the columns
                column1.Format.Alignment = ParagraphAlignment.Left;  // Left-align the first column
                column2.Format.Alignment = ParagraphAlignment.Right; // Right-align the second column

                // Add a row
                var row = table.AddRow();

                if (!String.IsNullOrEmpty(year))
                {
                    row.Cells[0].AddParagraph(year); // Add text to the left column
                }
                else
                {
                    row.Cells[0].AddParagraph(String.Empty); // Add text to the left column
                }
                row.Cells[1].AddParagraph(article.FileName); // Add text to the right column

                //section.AddParagraph(year, "Year");
            }
            else if (!String.IsNullOrEmpty(year))
            {
                section.AddParagraph(year, "Year");
            }
            if (!String.IsNullOrEmpty(article.Keywords))
            {
                section.AddParagraph(article.Keywords, "Keyword");
            }
            if (!String.IsNullOrEmpty(article.Authors))
            {
                section.AddParagraph(article.Authors, "Author");
            }
            if (!String.IsNullOrEmpty(article.AbstractBody) &&
                article.AbstractBody.IndexOf("No abstract", StringComparison.OrdinalIgnoreCase) < 0)
            {
                section.AddParagraph(article.AbstractBody, "Abstract");
            }
            section.AddParagraph(String.Empty, "Space");
        }
    }
}
