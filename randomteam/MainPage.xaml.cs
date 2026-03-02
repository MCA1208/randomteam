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


        private void OnAgregarJugadorClicked(object sender, EventArgs e)
        {

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

            lblContador.Text = $"Jugadores: {cantidad} / Mínimo 10";

            // Agregar SIEMPRE habilitado
            btnAgregar.IsEnabled = true;

            // Generar solo con 10 o más
            btnGenerar.IsEnabled = cantidad >= 10;

            // Color visual
            lblContador.TextColor = cantidad >= 10
                ? Colors.Green
                : Colors.Red;
        }
        private async void OnGenerarEquiposClicked(object sender, EventArgs e)
        {
            if (jugadores.Count < 10)
            {
                await DisplayAlert("Mínimo requerido", "Necesitás al menos 10 jugadores", "OK");
                return;
            }

            equipoA.Clear();
            equipoB.Clear();

            int mitad = jugadores.Count / 2;

            // Ordenar por nivel DESC (los mejores primero)
            var ordenados = jugadores
                .OrderByDescending(j => j.Nivel)
                .ToList();

            int sumaA = 0;
            int sumaB = 0;

            foreach (var jugador in ordenados)
            {
                // Si uno ya está lleno, va al otro
                if (equipoA.Count >= mitad)
                {
                    equipoB.Add(jugador);
                    sumaB += jugador.Nivel;
                }
                else if (equipoB.Count >= mitad)
                {
                    equipoA.Add(jugador);
                    sumaA += jugador.Nivel;
                }
                else
                {
                    // Balance por suma de nivel
                    if (sumaA <= sumaB)
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
            }

            // Animación suave
            equipoAView.Opacity = 0;
            equipoBView.Opacity = 0;

            await Task.WhenAll(
                equipoAView.FadeTo(1, 400),
                equipoBView.FadeTo(1, 400)
            );

            PuedeCompartir = true;

            await DisplayAlert("Equipos generados",
                $"🟢 Equipo A ({equipoA.Count} jugadores) ⭐ {sumaA}\n" +
                $"🔵 Equipo B ({equipoB.Count} jugadores) ⭐ {sumaB}",
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
            if (e.Data.Properties.ContainsKey("Jugador"))
            {
                var jugador = e.Data.Properties["Jugador"] as Jugador;

                if (jugador != null)
                {
                    if (equipoB.Contains(jugador))
                        equipoB.Remove(jugador);

                    if (!equipoA.Contains(jugador))
                        equipoA.Add(jugador);
                }
            }
        }

        private void OnDropEquipoB(object sender, DropEventArgs e)
        {
            if (e.Data.Properties.ContainsKey("Jugador"))
            {
                var jugador = e.Data.Properties["Jugador"] as Jugador;

                if (jugador != null)
                {
                    if (equipoA.Contains(jugador))
                        equipoA.Remove(jugador);

                    if (!equipoB.Contains(jugador))
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
        private void OnEliminarJugadorClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var jugador = button?.CommandParameter as Jugador;

            if (jugador != null)
            {
                jugadores.Remove(jugador);

                lblContador.Text = $"Jugadores: {jugadores.Count}";

                btnGenerar.IsEnabled = jugadores.Count >= 2;
            }
        }

    }

}
