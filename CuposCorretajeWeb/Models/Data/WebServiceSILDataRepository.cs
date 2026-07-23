using System.Configuration;

namespace CuposCorretajeWeb.Models.Data
{
    public class WebServiceSILDataRepository : Util
    {
        public override string GetWebSerive { get { return ConfigurationManager.AppSettings["SILDataApi"]; } internal set { } }
        // atributo para conexion con API SILData
        public override string GetWebServiceSILData { get { return ConfigurationManager.AppSettings["SILDataApi"]; } internal set { } }
    }
}