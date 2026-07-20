using System;

namespace CuposCorretajeWeb.Models.Solicitudes
{
  /// <summary>
  /// Subset de <c>Domain.Entities.Externo.Cupo</c> que el endpoint
  /// <c>POST /api/ShiftRequest/Accept</c> necesita dentro del array
  /// <c>CuposToBeDistributed</c>.
  ///
  /// Para esta primera integración, sólo usamos los identificadores y los
  /// campos relevantes para el matching (grano, vendedor, comprador, destino,
  /// fecha). El resto de los campos opcionales del <c>Cupo</c> original no
  /// son necesarios para Accept.
  /// </summary>
  public class CupoInputDto
  {
    public long Id { get; set; }
    public string CodGrano { get; set; }
    public string NomGrano { get; set; }
    public string CodVendSIL { get; set; }
    public string NomVendSIL { get; set; }
    public string CodCompSIL { get; set; }
    public string NomCompSIL { get; set; }
    public string CodDestino { get; set; }
    public string NomDestino { get; set; }
    public DateTime? Fecha { get; set; }
    public string CentroCupo { get; set; }
  }
}
