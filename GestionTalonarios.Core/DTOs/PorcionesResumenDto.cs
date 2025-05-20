using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestionTalonarios.Core.DTOs
{
    public class PorcionesResumenDto
    {
        public int PorcionesTradicionalesRestantes { get; set; }
        public int PorcionesVeganasRestantes { get; set; }
        public int TotalPorcionesRestantes => PorcionesTradicionalesRestantes + PorcionesVeganasRestantes;
    }
}
