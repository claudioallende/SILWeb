using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CuposCorretajeWeb.Models.Auditoria
{
    public class CupAudit
    {
        public virtual Cupos Cupo { get; set; }
        public virtual long Uvalue { get; set; }	
        public virtual string Tabla { get; set; }
        public virtual long IdInterno { get; set; }
        public virtual string TipoAudit { get; set; }
        public virtual string Usuario { get; set; }
        public virtual int Fecha { get; set; }
        public virtual int Hora { get; set; }
        public virtual string Observacion { get; set; }
        public virtual ISet<CupAuditHijo> ListaCupAuditHijo { get; set; }
    }
}