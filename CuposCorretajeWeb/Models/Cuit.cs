using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CuposCorretajeWeb.Models
{
    public class CuposCuit : ICuenta
    {
        public virtual string Cuit { get; set; }
        public virtual string Nombre { get; set; }
        public virtual string Domicilio { get; set; }
        public virtual string Localidad { get; set; }
        public virtual string Provincia { get; set; }
        public virtual long Cuenta { get; set; }
    }
}