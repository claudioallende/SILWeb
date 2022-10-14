using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CuposCorretajeWeb.Models
{
    public class DistribucionDisponible
    {
        public IList<VistaCuposDistribuidos> Distribuciones { get; set; }
        public IList<int> TotalesDisponiblesDias { get; set; }
        public IList<int> TotalesDistribuidosDias
        {
            get { return GetTotalesDistribudidosDias(Distribuciones); }
        }
        private IList<int> GetTotalesDistribudidosDias(IList<VistaCuposDistribuidos> Distribuciones)
        {
            IList<int> Totales = new List<int>();
            Totales.Add(Distribuciones.Sum(x => x.Hoy));
            Totales.Add(Distribuciones.Sum(x => x.Dia1));
            Totales.Add(Distribuciones.Sum(x => x.Dia2));
            Totales.Add(Distribuciones.Sum(x => x.Dia3));
            Totales.Add(Distribuciones.Sum(x => x.Dia4));
            Totales.Add(Distribuciones.Sum(x => x.Dia5));
            Totales.Add(Distribuciones.Sum(x => x.Dia6));
            Totales.Add(Distribuciones.Sum(x => x.Dia7));
            Totales.Add(Distribuciones.Sum(x => x.Dia8));
            Totales.Add(Distribuciones.Sum(x => x.Dia9));
            Totales.Add(Distribuciones.Sum(x => x.Dia10));
            Totales.Add(Distribuciones.Sum(x => x.Dia11));
            Totales.Add(Distribuciones.Sum(x => x.Dia12));
            Totales.Add(Distribuciones.Sum(x => x.Dia13));
            Totales.Add(Distribuciones.Sum(x => x.Dia14));
            Totales.Add(Distribuciones.Sum(x => x.Dia15));
            Totales.Add(Distribuciones.Sum(x => x.Dia16));
            Totales.Add(Distribuciones.Sum(x => x.Dia17));
            Totales.Add(Distribuciones.Sum(x => x.Dia18));
            Totales.Add(Distribuciones.Sum(x => x.Dia19));
            Totales.Add(Distribuciones.Sum(x => x.Dia20));
            return Totales;
        }

        public DistribucionDisponible(IList<VistaCuposDistribuidos> Distribuciones)
        {
            this.Distribuciones = Distribuciones;
        }
    }
}