using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BonApp.ViewModel
{
    public class SearchRecipeViewModel : ViewModelBase, INotifyPropertyChanged
    {
        private ICommand _listRecipesCommand;
        private INavigationService _navigationService;
        private String _searchInput;
        public String SearchInput
        {
            get { return _searchInput; }
            set
            {
                _searchInput = value;
                RaisePropertyChanged("SearchInput");
            }
        }

        public SearchRecipeViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
            var tradSearchDesc = loader.GetString("searchDesc");
            var tradSearch = loader.GetString("searchInput");
            var tradSearchTitle = loader.GetString("searchTitle");
        }

        public ICommand ListRecipesCommand
        {
            get
            {
                if (this._listRecipesCommand == null)
                {
                    this._listRecipesCommand = new RelayCommand(() => ListRecipesNavigate());
                }
                return this._listRecipesCommand;
            }
        }

        private void ListRecipesNavigate()
        {
            _navigationService.NavigateTo("ListRecipes", SearchInput);
        }


    }
}
