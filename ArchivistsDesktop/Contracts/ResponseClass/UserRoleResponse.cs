using System.Text.Json.Serialization;

namespace ArchivistsDesktop.Contracts.ResponseClass
{
    public class UserRoleResponse
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("title")]
        public string? Title { get; set; }
        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }
}
