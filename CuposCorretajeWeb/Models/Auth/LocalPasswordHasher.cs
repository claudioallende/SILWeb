using System;
using System.Security.Cryptography;

namespace CuposCorretajeWeb.Models.Auth
{
    // PBKDF2-HMACSHA1 (Rfc2898DeriveBytes por defecto, disponible nativo en .NET Framework,
    // sin agregar paquetes). Mismo esquema usado por generar-usuarios-json.ps1 al crear
    // App_Data\usuarios.json, para que los hashes generados ahi verifiquen aca.
    // Nombre "Local" a proposito: Microsoft.AspNet.Identity ya define un PasswordHasher propio,
    // y SignInManager.cs importa ambos namespaces (choque de nombre ambiguo si se llama igual).
    public static class LocalPasswordHasher
    {
        public const int HashSizeBytes = 32;

        public static bool Verify(string password, string saltBase64, string hashBase64, int iterations)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(saltBase64) || string.IsNullOrEmpty(hashBase64))
            {
                return false;
            }

            byte[] salt = Convert.FromBase64String(saltBase64);
            byte[] expectedHash = Convert.FromBase64String(hashBase64);

            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations))
            {
                byte[] actualHash = pbkdf2.GetBytes(expectedHash.Length);
                return FixedTimeEquals(actualHash, expectedHash);
            }
        }

        private static bool FixedTimeEquals(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
            {
                return false;
            }

            int diff = 0;
            for (int i = 0; i < a.Length; i++)
            {
                diff |= a[i] ^ b[i];
            }
            return diff == 0;
        }
    }
}
