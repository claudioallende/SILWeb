using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CuposCorretajeWeb.Models
{
    public class Centro
    {
        public virtual string Id { get; set; }
        public virtual string CodigoCentro { get; set; }
        public virtual string Nombre { get; set; }
    }
}