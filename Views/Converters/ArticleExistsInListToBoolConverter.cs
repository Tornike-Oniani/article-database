using Lib.DataAccessLayer.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

namespace MainLib.Views.Converters
{
    public class ArticleExistsInListToBoolConverter : IMultiValueConverter
    {

        private List<Article> _articles;
        private Article _article;
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            List<Article> articles = values[0] as List<Article>;
            Article article = values[1] as Article;

            this._articles = articles;
            this._article = article;

            return articles.Exists(a => a.ID == article.ID);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return new object[2] { _articles, _article };
        }
    }
}
