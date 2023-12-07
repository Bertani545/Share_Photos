using Newtonsoft.Json;
using SharePhotos.Models;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;

namespace SharePhotos.Views; 

public partial class Login_Page : ContentPage
{

    private readonly HttpClient client = new HttpClient();
    public Login_Page()
    {
        InitializeComponent();
    }

    private async void boton_Login_Clicked(object sender, EventArgs e)
    {
        string url = "https://sharephotos.azurewebsites.net/api/Cuentas/login";
        UserInfo usuario = new UserInfo
        {
            UserName = txtUserName.Text,
            Email = "basura@queleimporta.com",//Aquí podemos poner un Email cualquiera pues el token se arma con el usuario
                                        //Pero necesita este parametro para que no haga cosas raras el token
            Password = txtPassword.Text
        };


        string jsonUser = JsonConvert.SerializeObject(usuario);
        StringContent content = new StringContent(jsonUser, Encoding.UTF8, "application/json");

        var respuesta = await client.PostAsync(url, content);


        if (respuesta.IsSuccessStatusCode)
        {
            var tokenString = respuesta.Content.ReadAsStringAsync();
            var json = JsonConvert.DeserializeObject<UserToken>(tokenString.Result);
            await SecureStorage.SetAsync("token", json.Token);
            await Navigation.PushAsync(new MainPage(txtUserName.Text));

        }
        else
        {
            await DisplayAlert("Error", "No pudo ingresar", "Ok");
        }
    }
    

    

}