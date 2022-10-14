using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CuposCorretajeWeb.Models.Configuracion
{
    public class CuentaCYOViewModel
    {
        [Display(Name = "Cuenta")]
        public string NumeroCuenta { get; set; }
        [Required]
        [Display(Name = "Nombre")]
        public string NombreCuenta { get; set; }
        [Required]
        public string Cuit { get; set; }
        [Display(Name = "Cuenta y Orden")]
        public bool EsCuentaYOrden { get; set; }

        private DTabla GetEntidad()
        {
            DTabla nuevo = new DTabla();
            nuevo.Entidad = "CUPCYO";
            nuevo.Clave = this.NumeroCuenta;
            nuevo.Empresa = 0;
            nuevo.Uninego = null;
            nuevo.Zona = null;
            nuevo.Nuevo1 = DateTime.Now;
            return nuevo;
        }

        public DTabla GetEntidadCuenta()
        {
            DTabla nuevo = GetEntidad();
            nuevo.Orden = "CYO01";
            nuevo.Valor = this.NumeroCuenta;
            return nuevo;
        }

        public DTabla GetEntidadDescripcionCuenta()
        {
            DTabla nuevo = GetEntidad();
            nuevo.Orden = "CYO02";
            nuevo.Valor = this.NombreCuenta;
            return nuevo;
        }

        public DTabla GetEntidadCuit()
        {
            DTabla nuevo = GetEntidad();
            nuevo.Orden = "CYO03";
            nuevo.Valor = this.Cuit;
            return nuevo;
        }
    }
}