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

public partial class Update_Photo : ContentPage
{
    private HttpClient client = new HttpClient();
    Photo FOTO = new Photo();
    String usuario;
    public Update_Photo(int id, String usuario)
	{
        this.usuario = usuario;
        this.FOTO.Id = id;
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await load_Photo(FOTO.Id);


        if (FOTO.Id > 0)
        {
            LoadedImage.Source = FOTO.ImageUrl;
            txtTitle.Text = FOTO.Title;
            txtDescription.Text = FOTO.Description;
            txtURL.Text = FOTO.ImageUrl;
        }
        else
        {
            await DisplayAlert("Error", "No se pudo cargar", "Ok");
        }


    }


    async Task load_Photo(int id)
    {
        string url = "https://sharephotos.azurewebsites.net/api/Photos/Photo/";
        url += id.ToString();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", await SecureStorage.GetAsync("token"));
        var respuesta = await client.GetAsync(url);

        if (respuesta == null)
        {
            FOTO = new Photo { Id = -1 };
            return;
        }
        if (!respuesta.IsSuccessStatusCode)
        {
            FOTO = new Photo { Id = -1 };
            return;
        }

        var json = await respuesta.Content.ReadAsStringAsync();
        Photo tempFoto = JsonConvert.DeserializeObject<Photo>(json);

        FOTO = new Photo
        {
            Id = tempFoto.Id,
            Title = tempFoto.Title,
            User_Name = tempFoto.User_Name,
            Description = tempFoto.Description,
            ImageUrl = tempFoto.ImageUrl,
        };
    }



    private async void boton_Update_Clicked(object sender, EventArgs e)
    {
        string url = "https://sharephotos.azurewebsites.net/api/Photos/update/";
        url += FOTO.Id.ToString();
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
            await DisplayAlert("Registro", "Se ha actualizado correctamente", "Ok");
            await load_Photo(FOTO.Id);
            LoadedImage.Source = FOTO.ImageUrl;

        }
        else
        {
            await DisplayAlert("Error", "No se pudo actualizar la foto", "Ok");
        }
    }
}