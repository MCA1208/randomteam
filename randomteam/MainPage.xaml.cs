using System.Collections.ObjectModel;
using Microsoft.Maui.Graphics.Platform;
using randomteam.Models;
using System.Runtime.CompilerServices;
using System.ComponentModel;


namespace randomteam
{
    public partial class MainPage : ContentPage, INotifyPropertyChanged
    {
        private ObservableCollection<Jugador> jugadores = new();
        private ObservableCollection<Jugador> equipoA = new();
        private ObservableCollection<Jugador> equipoB = new();
        private readonly Random random = new();
        private int estrellasSeleccionadas = 0;
        private bool puedeCompartir;
        public bool PuedeCompartir
        {
            get => puedeCompartir;
            set
            {
                if (puedeCompartir != value)
                {
                    puedeCompartir = value;
                    OnPropertyChanged();
                }
            }
        }

        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;

            listaJugadores.ItemsSource = jugadores;
            equipoAView.ItemsSource = equipoA;
            equipoBView.ItemsSource = equipoB;
        }


        private async void OnAgregarJugadorClicked(object sender, EventArgs e)
        {
            if (jugadores.Count >= 10)
            {
                await DisplayAlert("Límite alcanzado",
                                   "Solo se permiten 10 jugadores para Fútbol 5",
                                   "OK");
                return;
            }

            if (!string.IsNullOrWhiteSpace(entryJugador.Text))
            {
                jugadores.Add(new Jugador
                {
                    Nombre = entryJugador.Text,
                    Nivel = estrellasSeleccionadas
                });

                entryJugador.Text = string.Empty;

                //estrellasSeleccionadas = 0; // reset después de agregar
                //PintarEstrellas(0);
                ActualizarContador();
            }
        }
        private void ActualizarContador()
        {
            int cantidad = jugadores.Count;

            lblContador.Text = $"Jugadores: {cantidad} / 10";

            btnAgregar.IsEnabled = cantidad < 10;
            btnGenerar.IsEnabled = cantidad == 10;

            lblContador.TextColor = cantidad == 10
                ? Colors.Green
                : Colors.Black;
        }
        private async void OnGenerarEquiposClicked(object sender, EventArgs e)
        {
            if (jugadores.Count < 10)
            {
                await DisplayAlert("Error", "Necesitas al menos 10 jugadores", "OK");
                return;
            }

            var random = new Random();

            // Mezclar primero
            var mezclados = jugadores
                .OrderBy(x => random.Next())
                .OrderByDescending(x => x.Nivel) // Ordenar por nivel alto primero
                .ToList();

            equipoA.Clear();
            equipoB.Clear();

            int sumaA = 0;
            int sumaB = 0;

            foreach (var jugador in mezclados)
            {
                if (equipoA.Count < 5 && (sumaA <= sumaB || equipoB.Count >= 5))
                {
                    equipoA.Add(jugador);
                    sumaA += jugador.Nivel;
                }
                else
                {
                    equipoB.Add(jugador);
                    sumaB += jugador.Nivel;
                }
            }

            await Task.Delay(50);

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                equipoAView.Opacity = 0;
                equipoBView.Opacity = 0;

                await Task.WhenAll(
                    equipoAView.FadeTo(1, 400),
                    equipoBView.FadeTo(1, 400)
                );
            });

            PuedeCompartir = true;

            await DisplayAlert("Equipos generados",
                $"Equipo A ⭐ {sumaA} vs Equipo B ⭐ {sumaB}",
                "OK");

            
        }
        private void OnStarTapped(object sender, EventArgs e)
        {
            if (sender is Label label &&
                label.GestureRecognizers.FirstOrDefault() is TapGestureRecognizer tap &&
                tap.CommandParameter != null)
            {
                int nivelSeleccionado = int.Parse(tap.CommandParameter.ToString());

                // ✅ Si toca la misma estrella, volver a 0
                if (estrellasSeleccionadas == nivelSeleccionado)
                    estrellasSeleccionadas = 0;
                else
                    estrellasSeleccionadas = nivelSeleccionado;

                PintarEstrellas(estrellasSeleccionadas);
            }
        }
        private void PintarEstrellas(int nivel)
        {
            Label[] estrellas = { star1, star2, star3, star4, star5 };

            for (int i = 0; i < estrellas.Length; i++)
            {
                estrellas[i].TextColor = i < nivel
                    ? Colors.Gold
                    : Colors.Gray;
            }
        }
        private async Task CompartirEquiposAsync()
        {
            var image = await layoutParaCompartir.CaptureAsync();

            if (image == null)
                return;

            string fileName = $"equipos_{DateTime.Now.Ticks}.png";
            string filePath = Path.Combine(FileSystem.CacheDirectory, fileName);

            using (var stream = await image.OpenReadAsync())
            using (var fileStream = File.OpenWrite(filePath))
            {
                await stream.CopyToAsync(fileStream);
            }

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Equipos Fútbol 5",
                File = new ShareFile(filePath)
            });
        }
        private async void OnCompartirClicked(object sender, EventArgs e)
        {
            if (equipoA.Count == 0 || equipoB.Count == 0)
            {
                await DisplayAlert("Error", "Primero generá los equipos", "OK");
                return;
            }

            await CompartirEquiposAsync();
        }

        private void OnDragStarting(object sender, DragStartingEventArgs e)
        {
            if (sender is BindableObject bindable &&
                bindable.BindingContext is Jugador jugador)
            {
                e.Data.Properties.Add("Jugador", jugador);
            }
        }
        private void OnDropEquipoA(object sender, DropEventArgs e)
        {
            if (e.Data.Properties.TryGetValue("Jugador", out var item))
            {
                var jugador = item as Jugador;

                if (equipoB.Contains(jugador))
                {
                    equipoB.Remove(jugador);
                    equipoA.Add(jugador);
                }
            }
        }
        private void OnDropEquipoB(object sender, DropEventArgs e)
        {
            if (e.Data.Properties.TryGetValue("Jugador", out var item))
            {
                var jugador = item as Jugador;

                if (equipoA.Contains(jugador))
                {
                    equipoA.Remove(jugador);
                    equipoB.Add(jugador);
                }
            }
        }
        private void LimpiarTodo()
        {
            jugadores.Clear();
            equipoA.Clear();
            equipoB.Clear();

            estrellasSeleccionadas = 0;
            PintarEstrellas(0);

            PuedeCompartir = false;

            ActualizarContador();
        }
        private async void OnReiniciarClicked(object sender, EventArgs e)
        {
            bool confirmar = await DisplayAlert(
                "Reiniciar",
                "¿Querés borrar todos los jugadores y equipos?",
                "Sí",
                "No");

            if (confirmar)
                LimpiarTodo();
        }
    }

}
