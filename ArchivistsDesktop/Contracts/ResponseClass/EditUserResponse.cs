using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ArchivistsDesktop.Contracts.ResponseClass;

public class EditUserResponse
{
    [JsonPropertyName("user")] public UserAllDataResponse User { get; set; } = null!;
    [JsonPropertyName("user_roles")] public List<UserRoleResponse>? UserRoles { get; set; }
    [JsonPropertyName("remove_roles")] public List<int>? RemoveRoles { get; set; }
}