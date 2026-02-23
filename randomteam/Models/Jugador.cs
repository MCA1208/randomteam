using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace randomteam.Models
{
    public class Jugador
    {
        public string Nombre { get; set; }

        // Nivel opcional (0 a 5)
        public int Nivel { get; set; } = 0;

        public string EstrellasVisual =>
            new string('⭐', Nivel);

    }
}
