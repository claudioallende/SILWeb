using System;

namespace CuposCorretajeWeb.Models.Solicitudes
{
  public class SolicitudTurnoView
  {
    public long Id { get; set; }
    public long? CuentaComprador { get; set; }
    public string NombreComprador { get; set; }
    public long CuentaVendedor { get; set; }
    public string NombreVendedor { get; set; }
    public long? CuentaDestino { get; set; }
    public string NombreDestino { get; set; }
    public int CodigoEstado { get; set; }
    public int NombreEstado { get; set; }
    public int CodigoGrano { get; set; }
    public string NombreGrano { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaSolicitado { get; set; }
    public bool EsFuturo { get; set; }
    public string CodigoCentro { get; set; }
    public string NombreCentro { get; set; }
    public string Observacion { get; set; }
    public short? EstadoCupo { get; set; }
    public short? CtgCupo { get; set; }
  }
}