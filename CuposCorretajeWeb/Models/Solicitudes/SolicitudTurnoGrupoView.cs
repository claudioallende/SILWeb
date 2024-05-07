using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CuposCorretajeWeb.Models.Solicitudes
{
  public class SolicitudTurnoGrupoView
  {
    public int CodigoGrano { get; set; }
    public string NombreGrano { get; set; }
    public long CuentaVendedor { get; set; }
    public string NombreVendedor { get; set; }
    public long? CuentaComprador { get; set; }
    public string NombreComprador { get; set; }
    public long? CuentaDestino { get; set; }
    public string NombreDestino { get; set; }
    public IEnumerable<SolicitudTurnoDetalleGrupoView> CantidadFechas { get; set; }
  }

  public class SolicitudTurnoDetalleGrupoView
  {
    public string Fecha { get; set; }
    public int Cantidad { get; set; }
    public int DiaSemana { get; set; }
  }
}