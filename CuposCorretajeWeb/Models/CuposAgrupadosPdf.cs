using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CuposCorretajeWeb.Models
{
    public class CuposAgrupadosPdf
    {
        public virtual int Id { get; set; }
        [Display(Name = "Grano")]
        public virtual int Grano { get; set; }
        [Display(Name = "Nombre")]
        public virtual string Nomgrano { get; set; }
        [Display(Name = "Cuenta Comprador")]
        public virtual long Compcta { get; set; }
        [Display(Name = "Nombre")]
        public virtual string Comprador { get; set; }
        [Display(Name = "Cuenta Vendedor")]
        public virtual long VendCta { get; set; }
        [Display(Name = "Nombre")]
        public virtual string Vendedor { get; set; }
        [Display(Name = "Puerto")]
        public virtual long PuertoCta { get; set; }
        [Display(Name = "Nombre")]
        public virtual string Puerto { get; set; }
        [Display(Name = "Cupos Recibidos")]
        public virtual int D0Cr { get; set; }
        [Display(Name = "Cupos Otorgados")]
        public virtual int D0Co { get; set; }

        [Display(Name = "Cupos Recibidos")]
        public virtual int D1Cr { get; set; }
        [Display(Name = "Cupos Otorgados")]
        public virtual int D1Co { get; set; }

        [Display(Name = "Cupos Recibidos")]
        public virtual int D2Cr { get; set; }
        [Display(Name = "Cupos Otorgados")]
        public virtual int D2Co { get; set; }

        [Display(Name = "Cupos Recibidos")]
        public virtual int D3Cr { get; set; }
        [Display(Name = "Cupos Otorgados")]
        public virtual int D3Co { get; set; }

        [Display(Name = "Cupos Recibidos")]
        public virtual int D4Cr { get; set; }
        [Display(Name = "Cupos Otorgados")]
        public virtual int D4Co { get; set; }

        [Display(Name = "Cupos Recibidos")]
        public virtual int D5Cr { get; set; }
        [Display(Name = "Cupos Otorgados")]
        public virtual int D5Co { get; set; }

        [Display(Name = "Hoy")]
        public virtual string Hoy { get; set; }
        [Display(Name = "Dia1")]
        public virtual string Dia1 { get; set; }
        [Display(Name = "Dia2")]
        public virtual string Dia2 { get; set; }
        [Display(Name = "Dia3")]
        public virtual string Dia3 { get; set; }
        [Display(Name = "Dia4")]
        public virtual string Dia4 { get; set; }
        [Display(Name = "Dia5")]
        public virtual string Dia5 { get; set; }
        public virtual int PendPdf { get; set; }

        public override bool Equals(object obj)
        {
            CuposAgrupadosPdf recievedObject = (CuposAgrupadosPdf)obj;

            if (ReferenceEquals(recievedObject, null)) return false;
            if (ReferenceEquals(recievedObject, this)) return true;

            if ((this.Compcta == recievedObject.Compcta)
                && (this.Grano == recievedObject.Grano)
                && (this.PuertoCta == recievedObject.PuertoCta)
                && (this.VendCta == recievedObject.VendCta)
                )
            {
                return (true);
            }
            return (false);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public virtual int D6Cr { get; set; }
        public virtual int D6Co { get; set; }
        public virtual int D7Cr { get; set; }
        public virtual int D7Co { get; set; }
        public virtual int D8Cr { get; set; }
        public virtual int D8Co { get; set; }
        public virtual int D9Cr { get; set; }
        public virtual int D9Co { get; set; }
        public virtual int D10Cr { get; set; }
        public virtual int D10Co { get; set; }
        public virtual int D11Cr { get; set; }
        public virtual int D11Co { get; set; }
        public virtual int D12Cr { get; set; }
        public virtual int D12Co { get; set; }
        public virtual int D13Cr { get; set; }
        public virtual int D13Co { get; set; }
        public virtual int D14Cr { get; set; }
        public virtual int D14Co { get; set; }
        public virtual int D15Cr { get; set; }
        public virtual int D15Co { get; set; }
        public virtual int D16Cr { get; set; }
        public virtual int D16Co { get; set; }
        public virtual int D17Cr { get; set; }
        public virtual int D17Co { get; set; }
        public virtual int D18Cr { get; set; }
        public virtual int D18Co { get; set; }
        public virtual int D19Cr { get; set; }
        public virtual int D19Co { get; set; }
        public virtual int D20Cr { get; set; }
        public virtual int D20Co { get; set; }
        public virtual string Dia6 { get; set; }
        public virtual string Dia7 { get; set; }
        public virtual string Dia8 { get; set; }
        public virtual string Dia9 { get; set; }
        public virtual string Dia10 { get; set; }
        public virtual string Dia11 { get; set; }
        public virtual string Dia12 { get; set; }
        public virtual string Dia13 { get; set; }
        public virtual string Dia14 { get; set; }
        public virtual string Dia15 { get; set; }
        public virtual string Dia16 { get; set; }
        public virtual string Dia17 { get; set; }
        public virtual string Dia18 { get; set; }
        public virtual string Dia19 { get; set; }
        public virtual string Dia20 { get; set; }
    }
}