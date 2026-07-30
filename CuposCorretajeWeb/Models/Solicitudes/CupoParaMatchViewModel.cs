using System;
using System.Collections.Generic;

namespace CuposCorretajeWeb.Models.Solicitudes
{
  /// <summary>
  /// ViewModel que consume el modal de Pantalla 3: un cupo con todas
  /// las solicitudes compatibles que el motor de matching detectó.
  ///
  /// Esta vista es la que dispara la decisión Variante A (cupo con
  /// vendedor) o Variante B (cupo sin vendedor): el JS decide en función
  /// de si <see cref="CodVendSIL"/> está poblado.
  /// </summary>
  public class CupoParaMatchViewModel
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
    public int CuposTotales { get; set; }

    /// <summary>
    /// Cupos totales a distribuir del cupo (de <c>VISTA_CUPOSDISTRIBUIDOV4</c>).
    /// Lo usa <c>Scripts/MatchingDistribucion.js#procesarRespuestaSearch</c>
    /// como gate para abrir el modal: sólo cuando hay cupos disponibles para
    /// el (comprador, vendedor, producto, centro) del cupo.
    /// </summary>
    public int Cupostotalesadist { get; set; }

    /// <summary>
    /// Solicitudes compatibles para este cupo, ya en formato de presentación.
    /// Se llena con la respuesta de <c>GET /api/ShiftRequest/MatchesPorCupo/{id}</c>.
    /// </summary>
    public List<SolicitudParaMatchViewModel> Matches { get; set; }

    public CupoParaMatchViewModel()
    {
      Matches = new List<SolicitudParaMatchViewModel>();
    }
  }
}
