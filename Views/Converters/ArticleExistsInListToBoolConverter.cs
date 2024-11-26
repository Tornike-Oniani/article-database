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

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values[0] == null || values[1] == null)
            {
                return null;
            }

            List<Article> articles = values[0] as List<Article>;
            Article article = values[1] as Article;

            if (articles == null || article == null)
            {
                return null;
            }

            return articles.Exists(a => a.ID == article.ID);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
