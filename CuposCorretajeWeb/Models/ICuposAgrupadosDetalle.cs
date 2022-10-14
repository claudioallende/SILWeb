using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CuposCorretajeWeb.Models
{
    public interface ICuposAgrupadosDetalle
    {
        int Id { get; set; }
        int Grano { get; set; }
        string Nomgrano { get; set; }
        long Compcta { get; set; }
        string Comprador { get; set; }
        long VendCta { get; set; }
        string Vendedor { get; set; }
        long PuertoCta { get; set; }
        string Puerto { get; set; }
        string CodCentro { get; set; }
        string Centro { get; set; }
        string CodCentroDist { get; set; }
        string CentroDist { get; set; }
        int D0Cr { get; set; }
        int D0Co { get; set; }
        int D1Cr { get; set; }
        int D1Co { get; set; }
        int D2Cr { get; set; }
        int D2Co { get; set; }
        int D3Cr { get; set; }
        int D3Co { get; set; }
        int D4Cr { get; set; }
        int D4Co { get; set; }
        int D5Cr { get; set; }
        int D5Co { get; set; }
        int D6Cr { get; set; }
        int D6Co { get; set; }
        int D7Cr { get; set; }
        int D7Co { get; set; }
        int D8Cr { get; set; }
        int D8Co { get; set; }
        int D9Cr { get; set; }
        int D9Co { get; set; }
        int D10Cr { get; set; }
        int D10Co { get; set; }
        int D11Cr { get; set; }
        int D11Co { get; set; }
        int D12Cr { get; set; }
        int D12Co { get; set; }
        int D13Cr { get; set; }
        int D13Co { get; set; }
        int D14Cr { get; set; }
        int D14Co { get; set; }
        int D15Cr { get; set; }
        int D15Co { get; set; }
        int D16Cr { get; set; }
        int D16Co { get; set; }
        int D17Cr { get; set; }
        int D17Co { get; set; }
        int D18Cr { get; set; }
        int D18Co { get; set; }
        int D19Cr { get; set; }
        int D19Co { get; set; }
        int D20Cr { get; set; }
        int D20Co { get; set; }
        string Hoy { get; set; }
        string Dia1 { get; set; }
        string Dia2 { get; set; }
        string Dia3 { get; set; }
        string Dia4 { get; set; }
        string Dia5 { get; set; }
        string Dia6 { get; set; }
        string Dia7 { get; set; }
        string Dia8 { get; set; }
        string Dia9 { get; set; }
        string Dia10 { get; set; }
        string Dia11 { get; set; }
        string Dia12 { get; set; }
        string Dia13 { get; set; }
        string Dia14 { get; set; }
        string Dia15 { get; set; }
        string Dia16 { get; set; }
        string Dia17 { get; set; }
        string Dia18 { get; set; }
        string Dia19 { get; set; }
        string Dia20 { get; set; }
        int PendPdf { get; set; }
        bool Equals(object obj);
        int GetHashCode();
        bool TieneCuposAsignados();
    }
}
