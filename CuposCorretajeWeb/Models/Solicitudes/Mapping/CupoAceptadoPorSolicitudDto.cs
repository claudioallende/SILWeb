using System.Collections.Generic;

namespace CuposCorretajeWeb.Models.Solicitudes.Mapping
{
  /// <summary>
  /// Mirror local de
  /// <c>SILData.Model.SolicitudTurno.CupoAceptadoPorSolicitudDto</c>.
  /// Para una solicitud, devuelve los <c>cupo_id</c> que ya fueron
  /// aceptados (figuran en <c>SOLTURNOS_DETALLE</c> con estado=1).
  /// La grilla lo usa para descontar esos cupos del conteo de matches
  /// del motor bulk: si la solicitud ya aceptó 3 cupos, esos 3 no deben
  /// contar como "match disponible" en la columna "Cupos compatibles".
  /// </summary>
  public class CupoAceptadoPorSolicitudDto
  {
    public long SolicitudId { get; set; }
    public List<long> CupoIds { get; set; } = new List<long>();
  }
}
