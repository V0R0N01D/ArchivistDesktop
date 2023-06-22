using System.Text.Json.Serialization;

namespace ArchivistsDesktop.Contracts.ResponseClass;

public class StudentOrderResponse
{
    [JsonPropertyName("id")] public int? Id { get; set; }
    [JsonPropertyName("order")] public OrderAllDataResponse Order { get; set; } = null!;
}