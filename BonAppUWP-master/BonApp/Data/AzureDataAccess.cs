using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BonApp.Model;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Security;
using Windows.UI.Xaml;

namespace BonApp.Data
{
    public class AzureDataAccess
    {
        private HttpClient client;
        private App currentApp = Application.Current as App;

        public AzureDataAccess()
        {
            client = new HttpClient();
            client.BaseAddress = new Uri("http://bonappuwp.azurewebsites.net/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<List<Recipe>> GetAllRecipes()
        {
            var recipes = new List<Recipe>();
            try
            {
                HttpResponseMessage response = await client.GetAsync("api/recipes");
                
                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    recipes = JsonConvert.DeserializeObject<List<Recipe>>(json);
                }
            }
            catch (HttpRequestException e) {
                return recipes;
            }
            return recipes;
        }

        public async Task<List<Recipe>> GetRecipeFavorite()
        {
            var recipes = new List<Recipe>();
            var user = new User();
            try
            {
                HttpResponseMessage response = await client.GetAsync("api/users/" + currentApp.GlobalInstance.userId);
                if (response.IsSuccessStatusCode)
                {
                    string jsonUser = await response.Content.ReadAsStringAsync();
                    user = JsonConvert.DeserializeObject<User>(jsonUser);
                    foreach (var item in user.userfavorites)
                    {
                        String idrecipe = item.recipeid_fav;
                        HttpResponseMessage responseRecipe = await client.GetAsync("api/recipes/" + idrecipe);
                        if (responseRecipe.IsSuccessStatusCode)
                        {
                            string jsonRecipe = await responseRecipe.Content.ReadAsStringAsync();
                            recipes.Add(JsonConvert.DeserializeObject<Recipe>(jsonRecipe));
                        }
                    }
                }
            }
            catch (HttpRequestException e)
            {
                return recipes;
            }
            
            return recipes;
        }

        public async Task<bool> AddToFavorite(Recipe r)
        {
            try
            {
                string json = JsonConvert.SerializeObject(r);
                HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync("api/recipes", content);
                if ((response.IsSuccessStatusCode) || (response.StatusCode.ToString().Equals("Conflict")))
                {
                    String recipeId = r.recipe_id + currentApp.GlobalInstance.userId;
                    UserFavorite uFav = new UserFavorite(recipeId, currentApp.GlobalInstance.userId, r.recipe_id);
                    string jsonfav = JsonConvert.SerializeObject(uFav);
                    HttpContent contentfav = new StringContent(jsonfav, Encoding.UTF8, "application/json");
                    HttpResponseMessage responsefav = await client.PostAsync("api/userfavorites", contentfav);
                    if (responsefav.IsSuccessStatusCode)
                    {
                        return true;
                    }
                    return false;
                }
            }
            catch (HttpRequestException e) {
                return false;
            }

            return false;
        }

        public async Task<bool> RemoveFavorite(Recipe r)
        {
            try
            {
                HttpResponseMessage response = await client.DeleteAsync("api/userfavorites/" + r.recipe_id + currentApp.GlobalInstance.userId);
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
            }
            catch (HttpRequestException e) {
                return false;
            }

            return false;
        }


        public async Task<String> FindUser(String user, String password)
        {
            var users = new List<User>();
            try
            {
                HttpResponseMessage response = await client.GetAsync("api/users");
                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    users = JsonConvert.DeserializeObject<List<User>>(json);
                    foreach (var item in users)
                    {
                        if (item.username.Equals(user) && item.password.Equals(password))
                        {
                            currentApp.GlobalInstance.userId = item.userid;
                            Windows.Storage.ApplicationDataContainer localSetting = Windows.Storage.ApplicationData.Current.LocalSettings;
                            localSetting.Values["userid"] = item.userid;
                            return "success";
                        }
                    }
                }
                return "errorLogin";
            }
            catch (HttpRequestException e)
            {
                return "noInternet";
            }

        }


        public async Task<String> createUser(String user, String password)
        {
            var userToCreate = new User();
            userToCreate.username = user;
            var users = new List<User>();
            try
            {
                HttpResponseMessage response = await client.GetAsync("api/users");
                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    users = JsonConvert.DeserializeObject<List<User>>(json);
                    foreach (var item in users)
                    {
                        if (item.username.Equals(user))
                        {
                            return "errorSub";
                        }
                    }
                }
                userToCreate.password = password;
                userToCreate.userfavorites = new List<UserFavorite>();
                string jsonUser = JsonConvert.SerializeObject(userToCreate);
                HttpContent content = new StringContent(jsonUser, Encoding.UTF8, "application/json");
                HttpResponseMessage responseCreate = await client.PostAsync("api/users", content);
                if (responseCreate.StatusCode.ToString().Equals("Conflict"))
                {
                    return "errorSub";
                }
                return "success";
            }
            catch (HttpRequestException e) {
                return "noInternet";
            }

        }
    }
}
