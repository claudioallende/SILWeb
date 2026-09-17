using System;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using Newtonsoft.Json;

namespace CuposCorretajeWeb.Models.Auth
{
    // Reemplazo temporal del SSO externo (acabase.com.ar, dado de baja): la "fuente de verdad"
    // de usuarios/permisos pasa a ser App_Data\usuarios.json en vez del servidor de autorizacion.
    // Se relee el archivo en cada login (sin cache) para que un reemplazo manual de un hash de
    // password en el archivo tome efecto sin reiniciar el sitio.
    public static class UsuariosLocalStore
    {
        private static string FilePath
        {
            get { return HostingEnvironment.MapPath("~/App_Data/usuarios.json"); }
        }

        public static UsuarioLocal FindByUsername(string usuario)
        {
            if (string.IsNullOrWhiteSpace(usuario))
            {
                return null;
            }

            var data = Load();
            return data.Usuarios.FirstOrDefault(u =>
                u.Activo &&
                string.Equals(u.Usuario, usuario.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        private static UsuariosLocalFile Load()
        {
            string path = FilePath;
            if (!File.Exists(path))
            {
                throw new InvalidOperationException("No se encontro App_Data/usuarios.json (fuente local de usuarios/permisos).");
            }

            string json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<UsuariosLocalFile>(json);
        }
    }
}
