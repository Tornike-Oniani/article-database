﻿using Lib.DataAccessLayer.Models;
using Lib.ViewModels.Base;
using Lib.ViewModels.Commands;
using MainLib.ViewModels.Utils;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace MainLib.ViewModels.Popups
{
    public class ReportsViewerViewModel : BaseViewModel
    {
        private string diaogSavedPath = String.Empty;
        private Document document;
        private PdfDocumentRenderer documentRenderer;
        private readonly Unit spaceAfterTitle = Unit.FromCentimeter(0.4);
        private readonly Unit spaceAfterRegular = Unit.FromCentimeter(0.2);
        private readonly Unit spaceAfterParagraph = Unit.FromCentimeter(1);
        private readonly Unit keywordIndentation = Unit.FromCentimeter(0.4);
        private readonly int titleFontSize = 16;
        private readonly int regularFontSize = 12;

        public List<Article> Articles { get; set; }

        public ICommand PrintCommand { get; set; }

        public ReportsViewerViewModel(List<Article> articles)
        {
            this.Title = "Reports Viewer";
            this.Articles = articles;
            InitializeDocument();            

            this.PrintCommand = new RelayCommand(Print);
        }

        public void Print(object input = null)
        {
            Section section = this.document.AddSection();

            foreach(Article article in this.Articles)
            {
                WriteArticle(article, section);
            }

            this.documentRenderer = new PdfDocumentRenderer();
            this.documentRenderer.Document = this.document.Clone();
            this.documentRenderer.RenderDocument();

            string path = Shared.GetInstance().BrowserService.OpenSaveFileDialog
                (
                    filter: "PDF files (*.pdf)|*.pdf",
                    defaultEx: "pdf",
                    fileName: "report.pdf",
                    savedPath: this.diaogSavedPath
                );

            this.documentRenderer.Save(path);
        }

        private void InitializeDocument()
        {
            this.document = new Document();
            // Title
            Style titleStyle = document.Styles["Heading1"];
            titleStyle.Font.Size = this.titleFontSize;
            titleStyle.Font.Bold = true;
            titleStyle.ParagraphFormat.SpaceAfter = this.spaceAfterTitle;
            // Year
            Style yearStyle = document.Styles.AddStyle("Year", "Normal");
            yearStyle.Font.Size = this.regularFontSize;
            yearStyle.Font.Color = Colors.Gray;
            yearStyle.ParagraphFormat.SpaceAfter = this.spaceAfterRegular;
            // Keyword
            Style keywordStyle = document.Styles.AddStyle("Keyword", "Normal");
            keywordStyle.Font.Size = this.regularFontSize;
            keywordStyle.Font.Italic = true;
            keywordStyle.ParagraphFormat.SpaceAfter = this.spaceAfterRegular;
            keywordStyle.ParagraphFormat.LeftIndent = this.keywordIndentation;
            // Authors
            Style authorStyle= document.Styles.AddStyle("Author", "Normal");
            authorStyle.Font.Size = this.regularFontSize;
            authorStyle.ParagraphFormat.SpaceAfter = this.spaceAfterRegular;
            // Abstarct
            Style abstractStyle = document.Styles.AddStyle("Abstract", "Normal");
            abstractStyle.Font.Size = this.regularFontSize;
            abstractStyle.ParagraphFormat.Alignment = ParagraphAlignment.Justify;
            // Space
            Style spaceStyle = document.Styles.AddStyle("Space", "Normal");
            spaceStyle.ParagraphFormat.SpaceAfter = this.spaceAfterParagraph;
        }

        private static void WriteArticle(Article article, Section section)
        {
            section.AddParagraph(article.Title, "Heading1");
            string year = article.Year.ToString();
            if (!String.IsNullOrEmpty(year))
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
