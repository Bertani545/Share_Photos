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


using MauiGrid = Microsoft.Maui.Controls.Grid;
using CompatibilityGrid = Microsoft.Maui.Controls.Compatibility.Grid;


namespace SharePhotos
{
    public partial class MainPage : ContentPage
    {
        private HttpClient client = new HttpClient();
        public ObservableCollection<Photo> Photos { get; set; }
        BoxView[,] boxViewsMatrix;

        private int Photo_Number = 0;
        private bool loggedIn = false;
        String usuario;

        private MauiGrid MainGrid = new MauiGrid();

        public MainPage()
        {
            
            InitializeComponent();
            addLoginAndSigin();
            BindingContext = this;
        }

        

        public MainPage(String usuario)
        {
            this.usuario = usuario;
            this.loggedIn = true;
            InitializeComponent();
            addUploadPhoto();
            BindingContext = this;

            if (usuario == "SuperUsuario") addDelete();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await load_Photos();

            build_Grid();
            
        }
        
        private void addLoginAndSigin()
        {
            var grid = new MauiGrid
            {
                Margin = new Thickness(0),
            };

            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1.0, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.5, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.5, GridUnitType.Star) });

            //We add the buttons
            Button LogIn = new Button
            {
                Text = "Ingresar",
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.Center,
                Margin = new Thickness(10, 10, 10, 10),

                BackgroundColor = Colors.DarkMagenta,
                FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label)),
                TextColor = Colors.White
            };
            LogIn.Clicked += async (sender, args) =>
            {
                await Navigation.PushAsync(new NavigationPage(new Login_Page()));
            };


            Button SignIn = new Button
            {
                Text = "Registrarse",
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.Center,
                Margin = new Thickness(10, 10, 10, 10),

                BackgroundColor = Colors.DarkMagenta,
                FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label)),
                TextColor = Colors.White
            };
            SignIn.Clicked += async (sender, args) =>
            {
                await Navigation.PushAsync(new NavigationPage(new SignIn_Page()));
            };

            grid.Add(LogIn, 0, 0);
            grid.Add(SignIn, 1, 0);

            Content_Layout.Add(grid);
        }


        private void addUploadPhoto()
        {
            Button addPhoto = new Button
            {
                Text = "Subir foto",
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.Center,
                Margin = new Thickness(20, 20, 20, 20),

                BackgroundColor = Colors.DarkMagenta,
                FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label)),
                TextColor = Colors.White

            };
            addPhoto.Clicked += async (sender, args) =>
            {
                await Navigation.PushAsync(new NavigationPage(new Add_Photo(this.usuario)));
            };

            Content_Layout.Add(addPhoto);
        }

        private void addDelete()
        {
            Button deletePhoto = new Button
            {
                Text = "Eliminar TODAS las fotos",
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Margin = new Thickness(20, 20, 20, 20),

                BackgroundColor = Colors.Red,
                FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label)),
                TextColor = Colors.White

            };
            deletePhoto.Clicked += async (sender, args) =>
            {
                string url = "https://sharephotos.azurewebsites.net/api/Photos/deleteALLphotos";
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", await SecureStorage.GetAsync("token"));
                var respuesta = await client.DeleteAsync(url);

                if (respuesta.IsSuccessStatusCode)
                {
                    await DisplayAlert("Correcto", "Se han eliminado todas las fotos", "Ok");
                    await load_Photos();
                    build_Grid();
                }
                else
                {
                    await DisplayAlert("Error", "No se pudieron eliminar las fotos", "Ok");
                }
            };

            Content_Layout.Add(deletePhoto);
        }

        async Task load_Photos()
        {
            ObservableCollection<Photo> photosAPI = new ObservableCollection<Photo>();

            string url = "https://sharephotos.azurewebsites.net/api/Photos/Get_Last_10_Photos";

            //En este header ponemos la seguridad
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", await SecureStorage.GetAsync("token"));

            var respuesta = await client.GetAsync(url);

            if(respuesta == null)
            {
                Photos = new ObservableCollection<Photo>();
                Photo_Number = 0;
                return;
            }
            if (!respuesta.IsSuccessStatusCode)
            {

                Photos = new ObservableCollection<Photo>();
                Photo_Number = 0;
                return;
            }

            var json = await respuesta.Content.ReadAsStringAsync();
            photosAPI = JsonConvert.DeserializeObject<ObservableCollection<Photo>>(json);


            Photos = new ObservableCollection<Photo>(photosAPI);
            Photo_Number = Photos.Count;
        }

        async void photo_Pressed(Photo photo, String usuario)
        {
            if (!loggedIn)
            {
                await DisplayAlert("No esta registrado", "Debe registrarse para ver el contenido", "Ok");
            }
            else
            {
                await Navigation.PushAsync(new DisplayImage(photo.Id, usuario));
            }
        }

        private void add_photo_to_grid(MauiGrid grid, int i, int j)
        {
            Photo photo_to_add = Photos.ElementAt(i * 2 + j);

            //Order the elements
            MauiGrid innerGrid = new MauiGrid();
            innerGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(7, GridUnitType.Star) });
            innerGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(3, GridUnitType.Star) });


            Image photo = new Image
            {
                Source = photo_to_add.ImageUrl,
                Aspect = Aspect.AspectFill
            };

            var frame = new Frame
            {
                Padding = 0,
                CornerRadius = 30,//new CornerRadius(30, 30, 0, 30),
                HasShadow = false,
                Content = photo,
                BackgroundColor = Colors.Transparent
                
            };



            //To make text readable
            RoundRectangle Roundrect = new RoundRectangle
            {
                HeightRequest = 210,
                //WidthRequest = 300,
                CornerRadius = new CornerRadius(30, 30, 30, 30),
                Opacity = 1,
                Fill = new LinearGradientBrush
                {
                    EndPoint = new Point(0, 1),
                    GradientStops =
                    {
                        new GradientStop { Color = Colors.Transparent, Offset = 0 },
                        new GradientStop { Color = Colors.Black, Offset = 1 }
                    }
                }
            };

            //Contains the elements
            String user_Name;
            String photo_Title;

            if(photo_to_add.Title.Length > 10)
            {
                photo_Title = photo_to_add.Title.Substring(0, 8) + "...";
            }
            else
            {
                photo_Title = photo_to_add.Title;
            }

            if(photo_to_add.User_Name.Length > 20)
            {
                user_Name = photo_to_add.User_Name.Substring(0,17) + "...";
            }
            else
            {
                user_Name = photo_to_add.User_Name;
            }

            VerticalStackLayout stackLayout = new VerticalStackLayout
            {
                Margin = new Thickness(10, 0, 0, 0),
                Children =
                {
                    new Label { Text = photo_Title,
                                TextColor = Colors.White,
                                FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label)),
                                },
                    new Label { Text = "@"+ user_Name, TextColor = Colors.White }
                }
            };


            //We make a button to click on the photo
            Button photoButton = new Button
            {
                BackgroundColor = Colors.Transparent, // Transparent background to make it visually like an overlay
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand
            };
            photoButton.Clicked += async (sender, args) =>
            {
                photo_Pressed(photo_to_add, usuario);
            };


            innerGrid.Add(frame, 0, 0);
            innerGrid.Add(Roundrect, 0, 0);
            innerGrid.Add(stackLayout, 0, 1);
            innerGrid.Add(photoButton, 0, 0);

            innerGrid.SetRowSpan(frame, 3);
            innerGrid.SetRowSpan(Roundrect, 2);


            //Nice looking
            Border border = new Border
            {
                BackgroundColor = Colors.Transparent,
                HeightRequest = 200,
                //WidthRequest = 300,
                Stroke = new LinearGradientBrush
                {
                    EndPoint = new Point(0, 1),
                    GradientStops = new GradientStopCollection
                    {
                        new GradientStop { Color = Colors.Orange, Offset = 0.1f },
                        new GradientStop { Color = Colors.Brown, Offset = 1.0f }
                    },
                },
                StrokeThickness = 4,
                StrokeShape = new RoundRectangle
                {
                    CornerRadius = new CornerRadius(30, 30, 30, 30)
                },
            };
            border.Content = innerGrid;

            grid.Add(border, j, i);

        }

        void build_Grid()
        {
            bool hasMainGrid = false;
            foreach (var child in Content_Layout.Children)
            {
                if (child == MainGrid)
                {
                    hasMainGrid = true;
                    break; // Found MainGrid, no need to continue searching
                }
            }
            if (hasMainGrid) Content_Layout.Children.Remove(MainGrid);

            MainGrid = new MauiGrid {
                RowSpacing = 6,
                ColumnSpacing = 10,
            };

            int Limit_i = Photo_Number/2 + Photo_Number%2;


            for (int i = 0; i < Limit_i; i++)
            {
                MainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(200) });
            }
            for (int i = 0; i < 2; i++)
            {
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.5, GridUnitType.Star) });
            }

            //For when we need to edit them
            if(Photo_Number > 0)
            {
                boxViewsMatrix = new BoxView[Limit_i, 2];

                //We add the photos
                for (int i = 0; i < Limit_i - 1; i++)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        add_photo_to_grid(MainGrid, i, j);
                    }
                }

                //Last column, especial case. 1 or 2 photos
                for (int j = 0; j < (Photo_Number + 1) % 2 + 1; j++)
                {
                    add_photo_to_grid(MainGrid, Limit_i - 1, j);
                }


                
            }
            Content_Layout.Add(MainGrid);

        }

    }

}
