using System.Text.Json.Serialization;

namespace ArchivistsDesktop.Contracts.ResponseClass;

public class CertificateEducationTypeReposponse
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("title")] public string Title { get; set; } = null!;
}