using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CuposCorretajeWeb.Models.Data;

namespace CuposCorretajeWeb.Models.Solicitudes.Mapping
{
  /// <summary>
  /// Compone el payload que Pantalla 2 envía a <c>POST /api/ShiftRequest/Accept</c>.
  /// </summary>
  /// <remarks>
  /// <para>
  /// El MVC no tiene una referencia de proyecto a SILData: el payload se arma
  /// con DTOs espejo (<see cref="SolicitudTurnoDto"/>, <see cref="CupoDto"/>,
  /// <see cref="ShiftRequestAcceptDataDto"/>) que coinciden en camelCase con las
  /// propiedades del backend. Newtonsoft.Json los serializa tal cual.
  /// </para>
  /// <para>
  /// Los DTOs "completos" (SolicitudTurno, Cupo) se obtienen de SILData
  /// mediante 2 GETs:
  /// </para>
  /// <list type="bullet">
  ///   <item><c>GET /api/ShiftRequest/{id}</c> → la solicitud completa.</item>
  ///   <item><c>POST /api/ShiftRequest/Cupos/ByIds</c> → los cupos completos.</item>
  /// </list>
  /// </remarks>
  public static class AcceptPayloadBuilder
  {
    /// <summary>
    /// Compone el payload SILData a partir de <paramref name="mvcRequest"/> +
    /// las entidades (solicitud y cupos) recuperadas previamente con GET.
    /// </summary>
    /// <remarks>
    /// En el modelo "Detalle Acumulativo" la solicitud nunca se muta en
    /// <c>Cantidad</c> ni en <c>CupoId</c> — esas columnas están FROZEN.
    /// Sólo se computa <see cref="SolicitudTurnoDto.Cantidad"/> como
    /// cuántos cupos quiere asignar el operador <i>en esta corrida</i>
    /// (no la cantidad pedida original).
    /// </remarks>
    /// <param name="mvcRequest">Request que envió la vista (idSolicitud, cupos, cantidades).</param>
    /// <param name="solicitudCompleta">Solicitud recuperada de <c>GET /api/ShiftRequest/{id}</c>.</param>
    /// <param name="cuposCompletos">Cupos recuperados de <c>POST /api/ShiftRequest/Cupos/ByIds</c>.</param>
    /// <returns>DTO serializable a JSON para enviar al backend.</returns>
    public static ShiftRequestAcceptDataDto Build(
      ConfirmarAsignacionRequest mvcRequest,
      SolicitudTurnoDto solicitudCompleta,
      IList<CupoDto> cuposCompletos)
    {
      if (mvcRequest == null)
        throw new System.ArgumentNullException(nameof(mvcRequest));
      if (solicitudCompleta == null)
        throw new System.ArgumentNullException(nameof(solicitudCompleta));
      cuposCompletos ??= new List<CupoDto>();

      // Cantidad en el payload = cuánto quiere asignar AHORA el operador.
      // El backend lo valida contra la cantidad pedida original (campo
      // FROZEN en SOLTURNOS) y rechaza con 400 si lo excede.
      int cantidadEfectiva = mvcRequest.CantidadPorCupo != null
                              && mvcRequest.CantidadPorCupo.Any()
        ? mvcRequest.CantidadPorCupo.Values.Sum()
        : mvcRequest.CupoIds?.Count ?? cuposCompletos.Count;
      if (cantidadEfectiva <= 0) cantidadEfectiva = 1;

      // Sobreescribir Cantidad en el DTO con la cantidad efectiva para
      // este Accept. La Cantidad original del modelo (FROZEN en BD)
      // sigue siendo solicitadaCompleta.Cantidad original — la copiamos
      // primero por seguridad aunque la BD la ignore.
      solicitudCompleta.Cantidad = cantidadEfectiva;

      return new ShiftRequestAcceptDataDto
      {
        ShiftRequest = new List<SolicitudTurnoDto> { solicitudCompleta },
        CuposToBeDistributed = cuposCompletos.ToList(),
        CantidadPorCupo = mvcRequest.CantidadPorCupo ?? new Dictionary<long, int>()
      };
    }

    /// <summary>
    /// Helper que combina la consulta GET a SILData para recuperar la
    /// solicitud completa y los cupos, y luego arma el payload. Útil cuando
    /// el caller no quiere tener que manejar los DTOs intermedios.
    /// </summary>
    /// <returns>Payload listo para serializar a JSON y enviar al Accept.</returns>
    public static async Task<ShiftRequestAcceptDataDto> BuildAsync(
      WebServiceSILRespository repo,
      ConfirmarAsignacionRequest mvcRequest)
    {
      // 1) Traer la solicitud completa. Si el operador eligió cupos pero la
      //    solicitud no existe, el endpoint devuelve 404 → propagamos.
      var solicitud = await repo.RequestSILDataGetAndDeserializeAsync<SolicitudTurnoDto>(
        "ShiftRequest", mvcRequest.IdSolicitud.ToString());

      // 2) Traer los cupos por id. POST con body = lista de ids.
      var cupos = mvcRequest.CupoIds != null && mvcRequest.CupoIds.Count > 0
        ? (await repo.RequestSILDataPostAndDeserializeAsync<List<CupoDto>>(
            "ShiftRequest", "Cupos/ByIds", mvcRequest.CupoIds))
          ?? new List<CupoDto>()
        : new List<CupoDto>();

      return Build(mvcRequest, solicitud, cupos);
    }
  }
}
