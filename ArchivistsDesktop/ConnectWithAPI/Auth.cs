using System;
using System.Security.Cryptography;
using System.Text;

namespace ArchivistsDesktop
{
    internal static class Auth
    {
        /// <summary>
        /// Получить query-параметр для авторизации
        /// </summary>
        /// <param name="login">Логин пользователя</param>
        /// <param name="password">Пароль пользователя</param>
        /// <returns></returns>
        internal static string GetAuth(string login, string password)
        {
            var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var identifier = $"{(password + login).GetSha256()}{unixTimestamp}".GetSha256();
            return $"{login}:{identifier}:{unixTimestamp}";
        }

        /// <summary>
        /// Получить hex массива байтов.
        /// </summary>
        private static string GetHex(this byte[] bytes)
        {
            var sb = new StringBuilder();
            foreach (byte b in bytes)
                sb.AppendFormat("{0:x2}", b);
            return sb.ToString();
        }

        /// <summary>
        /// Получить Sha256 хэш.
        /// </summary>
        private static string GetSha256(this string s)
        {
            var sha256 = SHA256.Create();
            var bytes = Encoding.ASCII.GetBytes(s);
            return sha256.ComputeHash(bytes).GetHex();
        }

        internal static string AddOptionalParam(this string s, string titleParam, object? value)
        {
            if (value is null)
            {
                return s;
            }

            return s.Contains('?') ? $"{s}&{titleParam}={value}" : $"{s}?{titleParam}={value}";
        }
    }
}