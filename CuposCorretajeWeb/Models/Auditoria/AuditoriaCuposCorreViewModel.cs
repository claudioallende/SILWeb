using CuposCorretajeWeb.Models.Cupo;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CuposCorretajeWeb.Models.Auditoria
{
    public class AuditoriaCuposCorreViewModel
    {
        public AuditoriaCuposCorreViewModel()
        {
            Operaciones = new List<SelectListItem>();
            foreach (TransicionDeEstado transicion in TransicionesDeEstadoFactory.Transiciones)
            {
                Operaciones.Add(new SelectListItem
                {
                    Value = transicion.Valor,
                    Text = transicion.Nombre
                });
            }
        }

        public string Usuario { get; set; }
        [Display(Name="Fecha Desde")]
        public DateTime? FechaDesde { get; set; }
        [Display(Name = "Fecha Hasta")]
        public DateTime? FechaHasta { get; set; }
        [Display(Name = "Operación")]
        public IList<SelectListItem> Operaciones { get; set; }
        public string OperacionSeleccionada { get; set; }
    }
}