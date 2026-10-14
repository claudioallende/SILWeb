using System.Collections.Generic;

namespace CuposCorretajeWeb.Models
{
  /// <summary>
  /// Modo de operación de <c>POST /api/Cupos/ActualizarDistribucion</c> en SILApi.
  /// Espejo de <c>ResourceServer.Models.ModoActualizacionDistribucion</c>.
  /// - <see cref="DistribucionManual"/>: comportamiento legacy para Distribucion.cshtml
  ///   (SILApi elige el CUPOSCORRE.Id final realmente distribuido a partir de la
  ///   clave de fila/día).
  /// - <see cref="SolicitudMatch"/>: AltaSolicitud.cshtml — cada asociación
  ///   informa <see cref="AsignacionSolicitudCupoDto.CupoSeleccionadoId"/> con
  ///   el id exacto del CUPOSCORRE a distribuir y que se persiste como
  ///   CUPO_ID en SOLTURNOS_DETALLE.
  /// </summary>
  public enum ModoActualizacionDistribucion
  {
    DistribucionManual = 1,
    SolicitudMatch = 2
  }

  /// <summary>
  /// Asociación solicitud-cupo enviada a SILApi en modo <see cref="ModoActualizacionDistribucion.SolicitudMatch"/>.
  ///
  /// Espejo de <c>ResourceServer.Models.AsignacionSolicitudCupoDto</c>. En este
  /// flujo siempre se informa <see cref="CupoSeleccionadoId"/> con el id
  /// exacto del CUPOSCORRE a distribuir, mientras que
  /// <see cref="CupoReferenciaId"/> queda en null (es el campo que usa
  /// DistribucionManual para que SILApi resuelva el id final).
  /// </summary>
  public class AsignacionSolicitudCupoDto
  {
    /// <summary>Id de la solicitud a la que se le asigna el cupo.</summary>
    public long SolicitudId { get; set; }

    /// <summary>
    /// AltaSolicitud: ID exacto del CUPOSCORRE que debe distribuirse y que
    /// se persiste como CUPO_ID en SOLTURNOS_DETALLE. Obligatorio en modo
    /// SolicitudMatch.
    /// </summary>
    public long? CupoSeleccionadoId { get; set; }

    /// <summary>
    /// Distribucion: referencia del matching; el ID final lo decide SILApi.
    /// No se usa en SolicitudMatch — queda null.
    /// </summary>
    public long? CupoReferenciaId { get; set; }

    /// <summary>
    /// Cantidad a asignar. En SolicitudMatch se fija siempre en 1: cada card
    /// representa un cupo y no se permite subdivisión intra-cupo en esta
    /// iteración.
    /// </summary>
    public int Cantidad { get; set; }

    /// <summary>
    /// Tipo de match devuelto por el motor: "Directo" | "Parcial" | "Condicional".
    /// Sólo se usa para diagnóstico en logs; el backend lo ignora al persistir.
    /// </summary>
    public string MatchType { get; set; }

    /// <summary>Clave de fila/día de Distribucion.cshtml. No se usa en SolicitudMatch.</summary>
    public long Compcta { get; set; }
    /// <summary>Clave de fila/día de Distribucion.cshtml. No se usa en SolicitudMatch.</summary>
    public long Vendcta { get; set; }
    /// <summary>Clave de fila/día de Distribucion.cshtml. No se usa en SolicitudMatch.</summary>
    public int Codproducto { get; set; }
    /// <summary>Clave de fila/día de Distribucion.cshtml. No se usa en SolicitudMatch.</summary>
    public long Ctadestino { get; set; }
    /// <summary>Clave de fila/día de Distribucion.cshtml. No se usa en SolicitudMatch.</summary>
    public string Cosecha { get; set; }
    /// <summary>Clave de fila/día de Distribucion.cshtml. No se usa en SolicitudMatch.</summary>
    public string Centro { get; set; }
    /// <summary>Clave de fila/día de Distribucion.cshtml. No se usa en SolicitudMatch.</summary>
    public int Fechaent { get; set; }
    /// <summary>Clave de fila/día de Distribucion.cshtml. No se usa en SolicitudMatch.</summary>
    public int Dia { get; set; }
  }

  /// <summary>
  /// Relación solicitud-cupo efectivamente persistida por SILApi en SOLTURNOS_DETALLE.
  /// Espejo de <c>ResourceServer.Models.AsignacionRealizadaDto</c>. La UI lo recibe
  /// dentro de <c>ActualizarDistribucionResult.Relaciones</c> y puede mostrarlo como
  /// acuse de recibo.
  /// </summary>
  public class AsignacionRealizadaDto
  {
    public long SolicitudId { get; set; }
    public long CupoId { get; set; }
  }

  /// <summary>
  /// Respuesta estructurada de <c>POST /api/Cupos/ActualizarDistribucion</c>.
  /// Espejo de <c>ResourceServer.Models.ActualizarDistribucionResult</c>.
  /// <see cref="Codigo"/> conserva la semántica legacy (1 = OK, 100/200 = errores,
  /// 300 = sin cambios) para compatibilidad con Distribucion.cshtml. En
  /// SolicitudMatch el flujo siempre termina en commit o rollback, así que
  /// Codigo será 1 cuando la operación es OK.
  /// </summary>
  public class ActualizarDistribucionResult
  {
    public int Codigo { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; }

    public int Solicitados { get; set; }
    public int Asignados { get; set; }
    public int Pendientes { get; set; }

    public IList<AsignacionRealizadaDto> Relaciones { get; set; }

    public ActualizarDistribucionResult()
    {
      Relaciones = new List<AsignacionRealizadaDto>();
    }
  }
}
