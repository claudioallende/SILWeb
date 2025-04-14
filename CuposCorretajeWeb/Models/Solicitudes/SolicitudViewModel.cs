using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CuposCorretajeWeb.Models.Solicitudes
{
  public class SolicitudViewModel
  {
    public string NombreGrano { get; set; }
    public string Solicitante { get; set; }
    public string Comprador { get; set; }
    public string Zona { get; set; }
    public Dictionary<string, int> Fechas { get; set; }
  }
}