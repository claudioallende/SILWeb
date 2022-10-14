using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CuposCorretajeWeb.Models
{
    public class DTabla
    {
        public virtual string Entidad { get; set; }
        public virtual string Clave { get; set; }
        public virtual string Orden { get; set; }
        public virtual string Valor { get; set; }
        public virtual int Empresa { get; set; }
        public virtual string Uninego { get; set; }
        public virtual string Zona { get; set; }
        public virtual DateTime Nuevo1 { get; set; }
        public virtual long Uvalue { get; set; }
    }
}