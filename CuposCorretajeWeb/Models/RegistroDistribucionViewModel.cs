using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CuposCorretajeWeb.Models
{
    /// <summary>
    /// VM de <c>POST /api/Cupos/ActualizarDistribucion</c> en SILApi.
    /// Se usa para DistribucionManual (legacy) y para SolicitudMatch (AltaSolicitud).
    /// En el modo SolicitudMatch sólo se envía <see cref="Modo"/> +
    /// <see cref="AsignacionesSolicitudCupo"/> + <see cref="Confirmacion"/>; el
    /// resto de los campos queda null/empty porque SILApi no los necesita.
    /// </summary>
    public class RegistroDistribucionViewModel
    {
        /// <summary>
        /// Modo de operación. Default = <see cref="ModoActualizacionDistribucion.DistribucionManual"/>
        /// para no romper callers legacy que no lo informen.
        /// </summary>
        public ModoActualizacionDistribucion? Modo { get; set; }

        /// <summary>
        /// Lista de asociaciones solicitud-cupo que SILApi debe procesar. Sólo
        /// se usa en modo <see cref="ModoActualizacionDistribucion.SolicitudMatch"/>.
        /// En DistribucionManual queda null.
        /// </summary>
        public IList<AsignacionSolicitudCupoDto> AsignacionesSolicitudCupo { get; set; }

        [Required]
        public IList<VistaCuposDistribuidos> cupos { get; set; }
        [Required]
        public Cupos nuevo { get; set; }
        [Required]
        public Cupos anterior { get; set; }
        [Required]
        public string puerto { get; set; }
        [Required]
        [Display(Name="Consignacion")]
        public Consignacion ConsignacionSeleccionada { get; set; }
        public string CosechaDesde { get; set; }
        public string CosechaHasta { get; set; }
        public DateTime? fecha { get; set; }
        public bool tieneVendedor { get; set; }
        [Required]
        [Display(Name = "Centro")]
        public string CentroSeleccionado { get; set; }
        public DateTime? fechaDesde { get; set; }
        public bool Confirmacion { get; set; }
    }
}