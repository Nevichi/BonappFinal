using BonApp.Data;
using BonApp.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using NotificationsExtensions.Toasts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.Xaml.Navigation;

namespace BonApp.ViewModel
{
    public class ListRecipesViewModel : ViewModelBase, INotifyPropertyChanged
    {
        private ObservableCollection<Recipe> _recipes;
        private Recipe _selectedRecipe;
        private INavigationService _navigationService;
        F2fDataAccess data;

        private ICommand _showRecipeCommand;
        public ICommand ShowRecipeCommand
        {
            get
            {
                if (this._showRecipeCommand == null)
                {
                    this._showRecipeCommand = new RelayCommand(() => ShowRecipe());
                }
                return this._showRecipeCommand;
            }
        }

        public Recipe SelectedRecipe
        {
            get { return _selectedRecipe; }
            set
            {
                _selectedRecipe = value;
                if (_selectedRecipe != null)
                {
                    RaisePropertyChanged("SelectedRecipe");
                }
            }
        }

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

        private void ShowRecipe()
        {
            if (CanExecute())
            {
                _navigationService.NavigateTo("Recipe", SelectedRecipe);
            }
        }

        public bool CanExecute()
        {
            return (SelectedRecipe == null) ? false : true; //renvoie false
        }



        [PreferredConstructor]
        public ListRecipesViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            Recipes = new ObservableCollection<Recipe>();
            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
            var tradSearch = loader.GetString("searchDesc");
        }

        public ObservableCollection<Recipe> Recipes
        {
            get { return _recipes; }
            set
            {
                _recipes = value;
                RaisePropertyChanged("Recipes");
            }
        }


        public void OnNavigatedTo(NavigationEventArgs e)
        {
            SearchInput = (String)e.Parameter;
            GetAllRecipes(SearchInput);
        }



        public async void GetAllRecipes(String input)
        {
            Recipes.Clear();
            data = new F2fDataAccess();
            List<Recipe> listRecipes = await data.GetAllRecipes(input);
            if (listRecipes.Count() == 0){
                createToast("noRecipe");
            }
            else { 
                foreach (var item in listRecipes)
                {
                    Recipes.Add(item);
                }
                listRecipes = null;
            }
            input = null;
        }

        public void createToast(String value)
        {
            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
            ToastVisual visual = new ToastVisual()
            {
                TitleText = new ToastText()
                {

                    Text = loader.GetString(value)
                },
            };

            ToastContent toastContent = new ToastContent();
            toastContent.Visual = visual;
            var toast = new ToastNotification(toastContent.GetXml());
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }
    }
}
