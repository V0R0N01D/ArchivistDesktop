using System;
using System.Text.Json.Serialization;

namespace ArchivistsDesktop.Contracts.ResponseClass;

public class UserAllDataResponse
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("login")] public string Login { get; set; } = null!;
    [JsonPropertyName("password")] public string? Password { get; set; }
    [JsonPropertyName("date_reg")] public DateTime? DateReg { get; set; }
    [JsonPropertyName("first_name")] public string? FirstName { get; set; }
    [JsonPropertyName("last_name")] public string? LastName { get; set; }
}