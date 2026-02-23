using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace randomteam.Models
{
    public  class Equipo
    {
        public string Nombre { get; set; }
        public ObservableCollection<Jugador> Jugadores { get; set; } = new();
    }
}
