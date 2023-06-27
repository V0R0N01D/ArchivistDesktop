using System;
using System.Text.Json.Serialization;

namespace ArchivistsDesktop.Contracts.ResponseClass;

public class DiplomResponse
{
    [JsonPropertyName("id")] public int? Id { get; set; }

    [JsonPropertyName("date_issue")] public DateOnly? DateIssue { get; set; }

    [JsonPropertyName("serial")] public string? Serial { get; set; }

    [JsonPropertyName("number")] public string Number { get; set; } = null!;
}