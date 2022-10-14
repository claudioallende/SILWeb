using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CuposCorretajeWeb.Models.Email
{
    public class MailPdf
    {
        public Cupos Consignacion { get; set; }
        public string Pdf { get; set; }
        public IList<Cupos> CuposAInformar { get; set; }
    }
}