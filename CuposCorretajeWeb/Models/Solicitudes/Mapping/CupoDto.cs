using System;

namespace CuposCorretajeWeb.Models.Solicitudes.Mapping
{
  /// <summary>
  /// Mirror local de <c>Domain.Entities.Externo.Cupo</c> (campos mínimos
  /// que SILData deserializa al armar el payload de Accept). Mantener
  /// sincronizado con la versión SILData.
  /// </summary>
  public class CupoDto
  {
    public long Id { get; set; }
    public string Alfanumerico { get; set; }
    public DateTime Fecha { get; set; }
    public string CentroCupo { get; set; }
    public string CodGrano { get; set; }
    public string CodDestino { get; set; }
    public string CodVendSIL { get; set; }
    public string CodCompSIL { get; set; }
    public string NomCompSIL { get; set; }
    public short EstadoSIL { get; set; }
    public DateTime? FechaInformadoSIL { get; set; }
  }
}
