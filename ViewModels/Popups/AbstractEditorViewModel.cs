using Lib.DataAccessLayer.Models;
using Lib.DataAccessLayer.Repositories;
using Lib.ViewModels.Base;
using Lib.ViewModels.Commands;
using Lib.ViewModels.Services.Dialogs;
using MainLib.ViewModels.UIStructs;
using MainLib.ViewModels.Utils;
using System;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace MainLib.ViewModels.Popups
{
    public class AbstractEditorViewModel : BaseViewModel
    {
        private readonly Article _article;

        public ArticleForm ArticleForm { get; set; }

        public ICommand SaveCommand { get; set; }
        public ICommand CancelCommand { get; set; }

        public AbstractEditorViewModel(Article article)
        {
            this.Title = "Edit abstract...";
            this._article = article;
            this.ArticleForm = new ArticleForm();
            this.ArticleForm.Abstract = article.AbstractBody;

            this.SaveCommand = new RelayCommand(Save, CanSave);
            this.CancelCommand = new RelayCommand(Cancel);
        }

        public void Save(object input = null)
        {

            ArticleForm.Abstract = ArticleForm.Abstract.Trim();

            // If article has abstract change it (We have to do this check because we keep abstracts in separate table)
            if (!String.IsNullOrEmpty(_article.AbstractBody)) 
            { 
                new AbstractRepo().UpdateAbstractByArticleId((int)_article.ID, ArticleForm.Abstract);

                // Track abstract editing
                //new Tracker(Shared.GetInstance().User).TrackCreate(new Lib.DataAccessLayer.Info.AbstractInfo() { ArticleTitle = _article.Title, AbstractBody = ArticleForm.Abstract, InfoType = "AbstractInfo" });
                new Tracker(Shared.GetInstance().User).TrackUpdate(new Lib.DataAccessLayer.Info.AbstractInfo() { ArticleTitle = _article.Title, AbstractBody = ArticleForm.Abstract, InfoType = "AbstractInfo" }, _article.ID.ToString());
            }
            else
            // Otherwise create new abstract article relationshitp
            {
                new AbstractRepo().AddAbstract((int)_article.ID, ArticleForm.Abstract);

                // Track abstract creationg
                new Tracker(Shared.GetInstance().User).TrackCreate(new Lib.DataAccessLayer.Info.AbstractInfo() { ArticleTitle = _article.Title, AbstractBody = ArticleForm.Abstract, InfoType = "Abstract" });
            }
            _article.AbstractBody = ArticleForm.Abstract;
            this.Window.Close();
        }
        public bool CanSave(object input = null)
        {
            if (ArticleForm.ErrorCollection == null)
            {
                return true;
            }

            if (ArticleForm.ErrorCollection.ContainsKey("Abstract") && ArticleForm.ErrorCollection["Abstract"] != null)
            {
                return false;
            }

            return true;
        }
        public void Cancel(object input = null)
        {
            this.Window.Close();
        }
    }
}
