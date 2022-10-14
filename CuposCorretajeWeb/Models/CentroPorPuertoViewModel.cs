using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CuposCorretajeWeb.Models
{
    public class CentroPorPuertoViewModel
    {
        public IList<CentroPorPuertoDTO> ListaCentrosPorPuerto { get; set; }
        public NuevoCentroPorPuertoViewModel Nuevo { get; set; }
    }

    public class NuevoCentroPorPuertoViewModel
    {
        [Display(Name = "Código o Nombre de Puerto")]
        public string IdOrNombrePuerto { get; set; }
        public long IdRelacion { get; set; }
        [Display(Name = "Centro")]
        public string CodigoOrNombreCentro { get; set; }
        public IList<SelectListItem> Centros { get; set; }
        public ServicioCentro ServicioCentro { get; set; }

        public NuevoCentroPorPuertoViewModel()
        {
            ServicioCentro = new ServicioCentro();
        }

        public async Task SetCentros()
        {
            IList<Centro> ListaCentros = await ServicioCentro.GetCentros();
            this.Centros = ListaCentros.Select(x => new SelectListItem()
            {
                Value = x.CodigoCentro,
                Text = x.Nombre
            })
            .ToList();
        }
    }
}