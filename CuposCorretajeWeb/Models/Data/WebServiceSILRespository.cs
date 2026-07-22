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
    // atributo para conexion con API SILData
    public override string GetWebServiceSILData { get { return ConfigurationManager.AppSettings["SILDataApi"]; } internal set { } }
    // atributo para conexion con API SILApi (Resource Server legacy: hosts CUPOSCORRE,
    // CUPOSDIST, SOLTURNOS, SOLTURNOS_DETALLE). Es el back-end del flujo
    // SolicitudMatch (AltaSolicitud).
    public override string GetApiBaseUrl { get { return ConfigurationManager.AppSettings["SILApi"]; } internal set { } }
  }
}