using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CuposCorretajeWeb.Models.Solicitudes
{
  public class FilterCuposDisponible
  {
    public long CuentaVendedor { get; set; }
    public DateTime Fecha { get; set; }
    public long CuentaComprador { get; set; }
    public int CodigoGrano { get; set; }
    public int ZonaGeografica { get; set; }
  }
}