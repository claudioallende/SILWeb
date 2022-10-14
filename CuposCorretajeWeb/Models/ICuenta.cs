using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CuposCorretajeWeb.Models
{
    public interface ICuenta
    {
        long Cuenta { get; set; }
        string Nombre { get; set; }
        string Cuit { get; set; }
    }
}
