using ArchivistsDesktop.Contracts.ResponseClass;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace ArchivistsDesktop.DataClass
{
    internal static class ConnectData
    {
        internal static HttpClient Client = new HttpClient()
        {
            BaseAddress = new Uri("http://localhost:5031/api/")
        };

        internal static string Login = "";
        internal static string Password = "";
        internal static List<UserRoleResponse> Roles = new ();

        internal static void ClearUserData()
        {
            Login = "";
            Password = "";
            Roles.Clear();
        }
    }
}
