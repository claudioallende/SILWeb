using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CuposCorretajeWeb.Models
{
    public class Puerto : ICuenta
    {
        public virtual long Id { get; set; }
        public virtual long Cuenta { get; set; }
        [Display(Name = "CUIT")]
        public virtual string Cuit { get; set; }
        public virtual string Domicilio { get; set; }
        public virtual string Localidad { get; set; }
        public virtual string Nombre { get; set; }
        public virtual string Provincia { get; set; }
        [Display(Name = "Tipo Proveedor")]
        public virtual int Stipprovee { get; set; }
        [Display(Name = "Tipo de Cuenta")]
        public virtual string Tipodecuenta { get; set; }
        public virtual long Cpostal { get; set; }
        public virtual string Ruca { get; set; }
        public virtual string IdTerminal { get; set; }
    }

    public class PuertoStop
    {
        [Display(Name = "Nro. Puerto STOP")]
        [Required]
        [Range(1, long.MaxValue, ErrorMessage = "Por favor ingrese un NRoPuertoSTOP")]
        [DefaultValue(1)]
        public virtual long NroPuerto { get; set; }
        [Display(Name = "Nombre Puerto")]
        public virtual string NombrePuerto { get; set; }
    }

    public class PuertoStopViewModel
    {
        public IList<PuertoStop> ListaPuertosStop { get; set; }
        public PuertoStop NuevoPuertoStop { get; set; }

        public PuertoStopViewModel()
        {
            NuevoPuertoStop = new PuertoStop();
        }
    }

    public class RelacionPuertoStopViewModel
    {
        public IList<RelacionPuertoStop> ListaRelacionesPuertosStop { get; set; }
        public NuevaRelacionPuertoStop NuevaRelacion { get; set; }

        public RelacionPuertoStopViewModel()
        {
            NuevaRelacion = new NuevaRelacionPuertoStop();
        }
    }

    public class NuevaRelacionPuertoStop
    {
        public long Id { get; set; }
        public IList<SelectListItem> PuertosSil { get; set; }
        [Required]
        [Display(Name = "Puerto SIL")]
        public long CodigoPuertoSil { get; set; }
        public string NombrePuertoSil { get; set; }
        public IList<SelectListItem> PuertosStop { get; set; }
        [Required]
        [Display(Name = "Puerto STOP")]
        public string CodigoPuertoStop { get; set; }
        public string NombrePuertoStop { get; set; }
        [Display(Name = "Es Valor por Defecto")]
        public bool ValorPorDefecto { get; set; }
    }

    public class RelacionPuertoStop
    {
        [Display(Name = "Nro. Puerto SIL")]
        //[Required]
        [Range(0, long.MaxValue, ErrorMessage = "Por favor ingrese un NroPuertoSIL")]
        [DefaultValue(0)]
        public int NroPuertoSIL { get; set; }
        [Display(Name = "Nombre Puerto")]
        public string nombrePuertoSil { get; set; }
        [Display(Name = "Nro. Puerto STOP")]
        //[Required]
        [Range(0, long.MaxValue, ErrorMessage = "Por favor ingrese un NRoPuertoSTOP")]
        [DefaultValue(0)]
        public long NroPuertoSTOP { get; set; }
        [Display(Name = "Nombre Puerto")]
        public string nombrePuertoSTOP { get; set; }
        [DefaultValue(0)]
        public long ValorPorDefecto { get; set; }
        public int Id { get; set; }
    }
}