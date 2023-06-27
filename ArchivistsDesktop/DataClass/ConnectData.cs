using ArchivistsDesktop.Contracts.ResponseClass;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace ArchivistsDesktop.DataClass
{
    internal static class ConnectData
    {
        private static string DefaultAddress = "http://37.230.114.195:2665/api/";

        private static HttpClient _client { get; set; }
        
        internal static HttpClient Client
        {
            get
            {
                if (_client is null)
                {
                    _client = new HttpClient()
                    {
                        BaseAddress = GetUriFromFile()
                    };
                }
                return _client;
            }
            set
            {
                _client = value;
            }
        }

        internal static string Login = "";
        internal static string Password = "";
        internal static List<RoleResponse>? Roles = new ();

        internal static void ClearUserData()
        {
            Login = "";
            Password = "";
            Roles ??= new();
            Roles.Clear();
        }

        internal static Uri GetUriFromFile()
        {
            if (!File.Exists("./server_address.txt"))
            {
                using (var writer = new StreamWriter("./server_address.txt", false))
                {
                    writer.Write(DefaultAddress);
                }
                return new Uri(DefaultAddress);
            }
            using (var reader = new StreamReader("./server_address.txt"))
            {
                var address = reader.ReadToEnd();
                return new Uri(address);
            }
        }
    }
}
