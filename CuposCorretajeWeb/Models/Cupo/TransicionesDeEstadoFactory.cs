using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CuposCorretajeWeb.Models.Cupo
{
    public static class TransicionesDeEstadoFactory
    {
        public static IList<TransicionDeEstado> Transiciones = new List<TransicionDeEstado>
            {
                new TransicionDeEstado { Nombre = "Alta", Valor = "alta", VOld = null, VNew = "0", Tipo = "ALT" },
                new TransicionDeEstado { Nombre = "Distribución", Valor = "distribucion", VOld = "0", VNew = "4", Tipo = "MOD" },
                new TransicionDeEstado { Nombre = "Anulación de distribución", Valor = "anulacion_de_distribucion", VOld = "4", VNew = "0", Tipo = "MOD" },
                new TransicionDeEstado { Nombre = "Distribución de cupo hijo cuenta y orden", Valor = "anulacion_de_distribucion_bloqueada", VOld = "4", VNew = "5", Tipo = "MOD" },
                new TransicionDeEstado { Nombre = "Anulación de distribución de cupo hijo cuenta y orden", Valor = "anulacion_de_distribucion_desbloqueada", VOld = "5", VNew = "4", Tipo = "MOD" },
                new TransicionDeEstado { Nombre = "Anulación de distribución por anulación de hijo cuenta y orden", Valor = "anulacion_de_distribucion_anulacion_hijo", VOld = "5", VNew = "0", Tipo = "MOD" },
                new TransicionDeEstado { Nombre = "Anulación de cupo", Valor = "anulacion_de_cupo", VOld = null, VNew = "3", Tipo = "MOD" },
                new TransicionDeEstado { Nombre = "Baja (Eliminado de Base de Datos)", Valor = "baja", VOld = null, VNew = null, Tipo = "BAJ" },
            };

        public static string GetNombreTransicion(string TipoAudit, string Vold = null, string Vnew = null)
        {
            if (!string.IsNullOrEmpty(TipoAudit))
            {
                var transiciones = Transiciones.Where(x => x.Tipo == TipoAudit);
                if (TipoAudit == "MOD")
                {
                    if (!string.IsNullOrEmpty(Vold) && Vnew != "3") transiciones = transiciones.Where(x => x.VOld == Vold);
                    transiciones = transiciones.Where(x => x.VNew == Vnew);
                }
                return transiciones.Select(x => x.Nombre).FirstOrDefault();
            }
            return "";
        }

        public static TransicionDeEstado GetTransicion(string valor)
        {
            return Transiciones.Where(x => x.Valor.ToUpper() == valor.ToUpper()).FirstOrDefault();
        }
    }
}