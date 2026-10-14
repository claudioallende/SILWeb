using System.Collections.Generic;

namespace CuposCorretajeWeb.Models.Solicitudes
{
  /// <summary>
  /// Cuerpo del request <c>POST /api/ShiftRequest/Accept</c> en
  /// SILData. Mapea <c>SILData.Model.SolicitudTurno.ShiftRequestAcceptData</c>.
  ///
  /// Regla del backend (ver <c>SolicitudTurnoService.AcceptRequestsAsync</c>):
  /// la suma de <c>ShiftRequest[i].Cantidad</c> debe coincidir con
  /// <c>CuposToBeDistributed.Count</c>, si no el endpoint rechaza con 409.
  /// </summary>
  public class ShiftRequestAcceptDataViewModel
  {
    public List<SolicitudInputDto> ShiftRequest { get; set; }
    public List<CupoInputDto> CuposToBeDistributed { get; set; }

    public ShiftRequestAcceptDataViewModel()
    {
      ShiftRequest = new List<SolicitudInputDto>();
      CuposToBeDistributed = new List<CupoInputDto>();
    }
  }
}
