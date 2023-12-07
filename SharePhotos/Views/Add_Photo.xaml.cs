using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Shapes;
using Newtonsoft.Json;
using SharePhotos.Models;
using SharePhotos.Views;
using System.Collections.ObjectModel;
using System.Net.Http.Headers;
using System.Text;
using System.Xml.Linq;

namespace SharePhotos.Views;

public partial class Add_Photo : ContentPage
{
    private HttpClient client = new HttpClient();
    String usuario;
    public Add_Photo(String usuario)
	{   
        this.usuario = usuario;
		InitializeComponent();
	}

    private async void boton_Registro_Clicked(object sender, EventArgs e)
    {
        string url = "https://sharephotos.azurewebsites.net/api/Photos/post_Photo";
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", await SecureStorage.GetAsync("token"));

        Photo photo = new Photo
        {
            Id = 0,
            User_Name = this.usuario,
            Title = txtTitle.Text,
            Description = txtDescription.Text,
            ImageUrl = txtURL.Text
        };


        string jsonPhoto = JsonConvert.SerializeObject(photo);
        StringContent content = new StringContent(jsonPhoto, Encoding.UTF8, "application/json");

        var respuesta = await client.PostAsync(url, content);


        if (respuesta.IsSuccessStatusCode)
        {
            await DisplayAlert("Registro", "Se ha subido correctamente", "Ok");
        }
        else
        {
            await DisplayAlert("Error", "No se pudo subir la foto", "Ok");
        }
    }
}