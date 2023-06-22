using System;
using System.Text.Json.Serialization;

namespace ArchivistsDesktop.Contracts.ResponseClass;

public class OrderAllDataResponse
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("number")] public string Number { get; set; } = null!;
    [JsonPropertyName("order_date")] public DateTime OrderDate { get; set; }
    [JsonPropertyName("group")] public GroupResponse? Group { get; set; }
    [JsonPropertyName("date_start")] public DateOnly? DateStartOrder { get; set; }
    [JsonPropertyName("type_order")] public TypeOrderResponse TypeOrder { get; set; } = null!;

    public string OrderDateString => OrderDate.ToString("d");
}