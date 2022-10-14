using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CuposCorretajeWeb.Models
{
    public class Grano
    {
        public virtual string Id { get; set; }
        public virtual int CodigoGrano { get; set; }
        public virtual string Nombre { get; set; }
    }

    public class GranoStop
    {
        [Display(Name = "Nro. Grano STOP")]
        [Required]
        [Range(1, long.MaxValue, ErrorMessage = "Por favor ingrese un NRoGranoSTOP")]
        [DefaultValue(1)]
        public virtual long NroGrano { get; set; }
        [Display(Name = "Nombre Grano")]
        public virtual string NombreGrano { get; set; }
    }

    public class GranoStopViewModel
    {
        public IList<GranoStop> ListaGranosStop { get; set; }
        public GranoStop NuevoGranoStop { get; set; }

        public GranoStopViewModel()
        {
            NuevoGranoStop = new GranoStop();
        }
    }

    public class RelacionGranoStopViewModel
    {
        public IList<RelacionGranoStop> ListaRelacionesGranosStop { get; set; }
        public NuevaRelacionGranoStop NuevaRelacion { get; set; }

        public RelacionGranoStopViewModel()
        {
            NuevaRelacion = new NuevaRelacionGranoStop();
        }
    }

    public class NuevaRelacionGranoStop
    {
        public long Id { get; set; }
        public IList<SelectListItem> GranosSil { get; set; }
        [Required]
        [Display(Name="Grano SIL")]
        public long CodigoGranoSil { get; set; }
        public string NombreGranoSil { get; set; }
        public IList<SelectListItem> GranosStop { get; set; }
        [Required]
        [Display(Name = "Grano STOP")]
        public string CodigoGranoStop { get; set; }
        public string NombreGranoStop { get; set; }
        [Display(Name = "Es Valor por Defecto")]
        public bool ValorPorDefecto { get; set; }
    }

    public class RelacionGranoStop
    {
        [Display(Name = "Nro. Grano SIL")]
        //[Required]
        [Range(0, long.MaxValue, ErrorMessage = "Por favor ingrese un NroGranoSIL")]
        [DefaultValue(0)]
        public int NroGranoSIL { get; set; }
        [Display(Name = "Nombre Grano")]
        public string nombreGranoSil { get; set; }
        [Display(Name = "Nro. Grano STOP")]
        //[Required]
        [Range(0, long.MaxValue, ErrorMessage = "Por favor ingrese un NRoGranoSTOP")]
        [DefaultValue(0)]
        public long NroGranoSTOP { get; set; }
        [Display(Name = "Nombre Grano")]
        public string nombreGranoSTOP { get; set; }
        [DefaultValue(0)]
        public long ValorPorDefecto { get; set; }
        public int Id { get; set; }
    }
}