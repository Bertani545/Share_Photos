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

public partial class DisplayImage : ContentPage
{
    private HttpClient client = new HttpClient();
    Photo FOTO = new Photo();
    String usuario;
    public DisplayImage(int id, String usuario)
	{
        this.usuario = usuario;
        this.FOTO.Id = id;

		InitializeComponent();

	}

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await load_Photo(FOTO.Id);


        if(FOTO.Id > 0)
        {
            LoadedImage.Source = FOTO.ImageUrl;
            txtTitle.Text = FOTO.Title;
            txtUsuario.Text = "@" + FOTO.User_Name;
            txtDescription.Text = "Descripición: " + FOTO.Description;

            if (usuario == FOTO.User_Name)
            {
                DeleteImage.IsEnabled = true;
                DeleteImage.IsVisible = true;

                UpdateImage.IsEnabled = true;
                UpdateImage.IsVisible = true;
            }
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


    private async void DeleteImage_Clicked(object sender, EventArgs e)
    {
        int id = FOTO.Id;
        string url = "https://sharephotos.azurewebsites.net/api/Photos/DeletePhoto/";
        url += id.ToString();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", await SecureStorage.GetAsync("token"));
        var respuesta = await client.DeleteAsync(url);

        if (respuesta.IsSuccessStatusCode)
        {
            await DisplayAlert("Correcto", "Se ha borrado correctamente", "Ok");
        }
        else
        {
            await DisplayAlert("Error", "No se pudo borrar", "Ok");
        }
    }

    private async void UpdateImage_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new NavigationPage(new Update_Photo(FOTO.Id, this.usuario)));
    }
}