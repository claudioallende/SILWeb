using System.Collections.Generic;
using Newtonsoft.Json;

namespace CuposCorretajeWeb.Models.Auth
{
    public class UsuarioLocal
    {
        public string Usuario { get; set; }
        public string Nombre { get; set; }
        public string Email { get; set; }
        public bool Activo { get; set; }

        public string PasswordSalt { get; set; }
        public string PasswordHash { get; set; }
        public int PasswordIterations { get; set; }

        public bool AccesoCuposCorretaje { get; set; }
        public IList<string> Centros { get; set; }
        public string CentroPorDefecto { get; set; }
        public bool AccesoAuditoria { get; set; }
        public bool AccesoConfiguracion { get; set; }
        public bool AccesoConfiguracionCYO { get; set; }
    }

    public class UsuariosLocalFile
    {
        public string GeneradoUtc { get; set; }
        public string Fuente { get; set; }
        public string HashAlgoritmo { get; set; }
        public string Nota { get; set; }
        public IList<UsuarioLocal> Usuarios { get; set; }
    }
}
