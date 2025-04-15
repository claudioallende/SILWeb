using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CuposCorretajeWeb.Models.Solicitudes
{
  public class SolicitudViewModel
  {
    public string NombreGrano { get; set; }
    public int CodigoGrano { get; set; }
    public long? CuentaVendedor { get; set; }
    public string Solicitante { get; set; }
    public long? CuentaComprador { get; set; }
    public string Comprador { get; set; }
    public long? CuentaDestino { get; set; }
    public string Zona { get; set; }
    public Dictionary<string, int> Fechas { get; set; }
  }
}