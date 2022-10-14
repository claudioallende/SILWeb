using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace CuposCorretajeWeb.Models.Data
{
    public class WebServiceSILRespository : Util
    {
        public override string GetWebSerive { get { return ConfigurationManager.AppSettings["WebServiceCuposCorretaje"]; } internal set { } }
    }
}