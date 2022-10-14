using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CuposCorretajeWeb.Models.Auditoria
{
    public class CupAuditHijo
    {
        public virtual string Campo { get; set; }
        public virtual string Vnew { get; set; }
        public virtual string Vold { get; set; }
        public virtual long Uvalue { get; set; }
        public virtual long UvaluePadre { get; set; }
    }
}