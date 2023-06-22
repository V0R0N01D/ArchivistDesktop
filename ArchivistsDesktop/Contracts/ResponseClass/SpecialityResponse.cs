using System.Text.Json.Serialization;

namespace ArchivistsDesktop.Contracts.ResponseClass;

public class SpecialityResponse
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("fgos")] public string Fgos { get; set; } = null!;
    [JsonPropertyName("title")] public string Title { get; set; } = null!;
}