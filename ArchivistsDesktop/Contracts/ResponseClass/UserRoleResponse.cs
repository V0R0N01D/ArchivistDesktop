using System.Text.Json.Serialization;

namespace ArchivistsDesktop.Contracts.ResponseClass;

public class UserRoleResponse
{
    [JsonPropertyName("id")] public int? Id { get; set; }
    [JsonPropertyName("role")] public RoleResponse Role { get; set; } = null!;
}