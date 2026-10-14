namespace CuposCorretajeWeb.Models
{
  /// <summary>
  /// Modo de operación de POST /api/Cupos/ActualizarDistribucion en SILApi.
  /// Espejo de SILApi/SILResourceServer/Models/DTO/AsignacionSolicitudCupo.cs.
  /// </summary>
  public enum ModoActualizacionDistribucion
  {
    /// <summary>
    /// Comportamiento legacy (Distribucion.cshtml). El cliente envía cantidades
    /// agrupadas por fila/día y SILApi elige los CUPOSCORRE.Id físicos a
    /// distribuir.
    /// </summary>
    DistribucionManual = 1,

    /// <summary>
    /// Aceptación y distribución desde AltaSolicitud.cshtml. El cliente envía
    /// pares explícitos SolicitudId / CupoSeleccionadoId (unidades) y SILApi
    /// distribuye exactamente esos cupos y persiste el vínculo en
    /// SOLTURNOS_DETALLE.
    /// </summary>
    SolicitudMatch = 2
  }
}
