using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CuposCorretajeWeb.Models.Auditoria
{
    public class AuditoriaCuposCorre
    {
        public string Alfanumerico { get; set; }
        public string Usuario { get; set; }
        public string Vnew { get; set; }
        public string Vold { get; set; }
        public string TipoAudit { get; set; }
        public string Operacion { 
            get {
                return CuposCorretajeWeb.Models.Cupo.TransicionesDeEstadoFactory.GetNombreTransicion(this.TipoAudit, this.Vold, this.Vnew);
                //if (this.TipoAudit == "ALT") return "Alta";
                //if (this.TipoAudit == "MOD" && this.Vold == "0" && this.Vnew == "4") return "Distribución";
                //if (this.TipoAudit == "MOD" && this.Vold == "4" && this.Vnew == "0") return "Anulación de Distribución";
                //if (this.TipoAudit == "MOD" && this.Vold == "4" && this.Vnew == "5") return "Anulación de Distribución Bloqueada";// "Cancelación";
                //if (this.TipoAudit == "MOD" && this.Vold == "5" && this.Vnew == "4") return "Anulación de Distribución Desbloqueada";
                //if (this.TipoAudit == "MOD" && this.Vnew == "3") return "Anulación de Cupo";
                //if (this.TipoAudit == "BAJ") return "Baja";
                //return "";
            } 
            internal set { }
        }
        public string Fecha { get; set; }
        public string Hora { get; set; }
        public string Observacion { get; set; }
    }
}