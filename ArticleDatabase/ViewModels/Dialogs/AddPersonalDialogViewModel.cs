using ArticleDatabase.Commands;
using ArticleDatabase.DataAccessLayer;
using ArticleDatabase.DataAccessLayer.Repositories;
using ArticleDatabase.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ArticleDatabase.ViewModels
{
    public class AddPersonalDialogViewModel : BaseViewModel
    {
        public DataViewViewModel Parent { get; set; }
        public string PersonalComment { get; set; }
        public int SIC { get; set; }

        public RelayCommand AddPersonalCommand { get; set; }

        public AddPersonalDialogViewModel(DataViewViewModel parent)
        {
            this.Title = "Add comment & SIC";

            // 1. Set attributes from parent
            Parent = parent;
            PersonalComment = parent.SelectedArticle.PersonalComment;
            SIC = parent.SelectedArticle.SIC;

            // 2. Initialize commands
            AddPersonalCommand = new RelayCommand(AddPersonal);

        }

        public void AddPersonal(object input)
        {
            if (PersonalComment != Parent.SelectedArticle.PersonalComment || SIC != Parent.SelectedArticle.SIC)
            {
                // 1. Send new comment and SIC values to parent
                Parent.SelectedArticle.PersonalComment = PersonalComment;
                Parent.SelectedArticle.SIC = SIC;

                // 2. Update record in database
                (new ArticleRepo()).UpdatePersonal(Parent.SelectedArticle, Parent.User);

                // 3. Close window
                (input as ICommand).Execute(null);
            }
        }
    }
}
