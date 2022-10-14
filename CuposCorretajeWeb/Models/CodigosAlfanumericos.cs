using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CuposCorretajeWeb.Models
{
    public class CodigosAlfanumericos
    {
        public int NumeroDia { 
            get {
                return (Dia.Date - DateTime.Now.Date).Days;
            } 
        }
        public DateTime Dia { get; set; }
        public string Alfanumerico { get; set; }
        public string CantidadCupos { get; set; }

        public string DiaFormateado { 
            get 
            {
                return Dia.ToString("dd/MM");
            }
            set { }
        }

        public CodigosAlfanumericos() {}

        public bool CodigosIsNullOrEmpty()
        {
            return string.IsNullOrEmpty(this.Alfanumerico) && string.IsNullOrEmpty(this.CantidadCupos);
        }

        public Cupos GetCupo()
        {
            return new Cupos()
            {
                Nrocupo = Alfanumerico,
                Fecha = Dia
            };
        }
    }
}