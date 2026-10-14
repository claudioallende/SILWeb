using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using Newtonsoft.Json;

namespace CuposCorretajeWeb.Models.Auth
{
    public class AdminUsuarioRequest
    {
        public string Usuario { get; set; }
        public string Nombre { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public List<string> Centros { get; set; }
        public string CentroPorDefecto { get; set; }
        public bool? AccesoCuposCorretaje { get; set; }
        public bool? AccesoAuditoria { get; set; }
        public bool? AccesoConfiguracion { get; set; }
        public bool? AccesoConfiguracionCYO { get; set; }
        public bool? Activo { get; set; }
    }

    public class AdminUsuarioResult
    {
        public bool Ok { get; set; }
        public string Usuario { get; set; }
        public string Accion { get; set; }
        public string Error { get; set; }
    }

    // Equivalente en C# de App_Data\Administrar-UsuarioLocal.ps1: da de alta o actualiza un
    // usuario en usuarios.json. Existe para no depender de conectarse (RDP/PS remoting) a un
    // servidor on-premise lento solo para correr el script - ver AdminController.UsuarioLocal.
    public static class UsuariosLocalAdmin
    {
        private static readonly object Gate = new object();

        private static string FilePath
        {
            get { return HostingEnvironment.MapPath("~/App_Data/usuarios.json"); }
        }

        private static string LogPath
        {
            get { return HostingEnvironment.MapPath("~/App_Data/admin-audit.log"); }
        }

        public static AdminUsuarioResult Upsert(AdminUsuarioRequest req, string origen)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Usuario))
            {
                return new AdminUsuarioResult { Ok = false, Error = "Falta 'usuario'." };
            }

            string path = FilePath;
            if (!File.Exists(path))
            {
                return new AdminUsuarioResult { Ok = false, Error = "No existe App_Data/usuarios.json en este servidor." };
            }

            string usuarioNorm = req.Usuario.Trim().ToUpperInvariant();

            lock (Gate)
            {
                var root = JsonConvert.DeserializeObject<UsuariosLocalFile>(File.ReadAllText(path));
                var existing = root.Usuarios.FirstOrDefault(u =>
                    string.Equals(u.Usuario, usuarioNorm, StringComparison.OrdinalIgnoreCase));

                bool esAlta = existing == null;
                if (esAlta && (string.IsNullOrEmpty(req.Password) || req.Centros == null || req.Centros.Count == 0))
                {
                    return new AdminUsuarioResult
                    {
                        Ok = false,
                        Error = $"Usuario '{usuarioNorm}' no existe. Para dar de alta son obligatorios 'password' y 'centros'."
                    };
                }

                UsuarioLocal target = existing;
                if (esAlta)
                {
                    target = new UsuarioLocal
                    {
                        Usuario = usuarioNorm,
                        Nombre = "",
                        Email = "",
                        Activo = true,
                        Centros = new List<string>(),
                        CentroPorDefecto = "",
                        AccesoCuposCorretaje = true,
                        PasswordIterations = LocalPasswordHasher.DefaultIterations
                    };
                    root.Usuarios.Add(target);
                }

                if (req.Nombre != null) target.Nombre = req.Nombre;
                if (req.Email != null) target.Email = req.Email;
                if (req.Centros != null) target.Centros = req.Centros;
                if (req.CentroPorDefecto != null) target.CentroPorDefecto = req.CentroPorDefecto;
                if (req.AccesoCuposCorretaje.HasValue) target.AccesoCuposCorretaje = req.AccesoCuposCorretaje.Value;
                if (req.AccesoAuditoria.HasValue) target.AccesoAuditoria = req.AccesoAuditoria.Value;
                if (req.AccesoConfiguracion.HasValue) target.AccesoConfiguracion = req.AccesoConfiguracion.Value;
                if (req.AccesoConfiguracionCYO.HasValue) target.AccesoConfiguracionCYO = req.AccesoConfiguracionCYO.Value;
                if (req.Activo.HasValue) target.Activo = req.Activo.Value;

                bool cambioPassword = !string.IsNullOrEmpty(req.Password);
                if (cambioPassword)
                {
                    string salt, hash;
                    LocalPasswordHasher.Hash(req.Password, LocalPasswordHasher.DefaultIterations, out salt, out hash);
                    target.PasswordSalt = salt;
                    target.PasswordHash = hash;
                    target.PasswordIterations = LocalPasswordHasher.DefaultIterations;
                }

                string backup = path + ".bak-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
                File.Copy(path, backup, true);

                File.WriteAllText(path, JsonConvert.SerializeObject(root, Formatting.Indented));

                Log(usuarioNorm, esAlta ? "alta" : "actualizado", origen, cambioPassword);

                return new AdminUsuarioResult { Ok = true, Usuario = usuarioNorm, Accion = esAlta ? "alta" : "actualizado" };
            }
        }

        private static void Log(string usuario, string accion, string origen, bool cambioPassword)
        {
            try
            {
                string line = string.Format(
                    "{0:o}\t{1}\t{2}\tpasswordCambiada={3}\torigen={4}{5}",
                    DateTime.UtcNow, accion, usuario, cambioPassword, origen, Environment.NewLine);
                File.AppendAllText(LogPath, line);
            }
            catch
            {
                // el log es best-effort: si falla, no debe tirar abajo el alta/actualizacion
            }
        }
    }
}
