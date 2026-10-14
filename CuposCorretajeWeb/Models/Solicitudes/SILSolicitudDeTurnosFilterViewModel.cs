using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CuposCorretajeWeb.Models.Solicitudes
{
  public class SILSolicitudDeTurnosFilterViewModel
  {
    public List<string> Centros { get; set; } = new List<string>();
    public int Dias { get; set; }
  }
}