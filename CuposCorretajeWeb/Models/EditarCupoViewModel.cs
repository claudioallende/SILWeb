using CuposCorretajeWeb.Models.Configuracion;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CuposCorretajeWeb.Models
{
    public class EditarCupoViewModel
    {
        public string Id { get; set; }
        public virtual IEnumerable<SelectListItem> ListaConsignaciones { get; set; }
        private IList<Cupos> CuerposConsignacion { get; set; }
        public string Consignacion { get; set; }
        public ConsignacionDpo ConsignacionSeleccionada { get; set; }

        public IEnumerable<ConsignacionDpo> Consignaciones;
        public string Producto { get; set; }
        public string Comprador { get; set; }
        public string Vendedor { get; set; }
        public string Puerto { get; set; }
        public int CodigoProducto { get; set; }
        public long CuentaComprador { get; set; }
        public long CuentaVendedor { get; set; }
        public long CuentaPuerto { get; set; }
        public bool CyO { get; set; }
        public IList<NuevoAlfanumerico> ListaAlfanumerico { get; set; }
        public IEnumerable<DTabla> MotivosAnulacion { get; set; }
        [Display(Name = "Motivo")]
        public virtual string MotivoAnulacionSeleccionado { get; set; }
        public string Error { get; set; }
    }

    public class NuevoAlfanumerico
    {
        public int NumeroDia { get; set; }
        public string Dia { get; set; }
        public IList<string> AlfaDia { get; set; }
        public IList<Cupos> CuerposDia { get; set; }
        public string GetDiaFormateado()
        {
            return DateTime.Now.AddDays(this.NumeroDia).Date.ToString("dd/MM");
        }
    }

    public class RespuestaAnularViewModel
    {
        public string Respuesta { get; set; }
        public string Tipo { get; set; }
        public IList<AnuladoViewModel> Cupos { get; set; }
    }
}