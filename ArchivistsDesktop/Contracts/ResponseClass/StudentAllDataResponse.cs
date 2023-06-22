using System;
using System.Text.Json.Serialization;

namespace ArchivistsDesktop.Contracts.ResponseClass;

public class StudentAllDataResponse
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("firstname")] public string FirstName { get; set; } = null!;
    [JsonPropertyName("lastname")] public string LastName { get; set; } = null!;
    [JsonPropertyName("patronymic")] public string? Patronymic { get; set; }
    [JsonPropertyName("date_birthday")] public DateOnly DateBirthday { get; set; }
    [JsonPropertyName("passport_serial")] public string? PassportSerial { get; set; }
    [JsonPropertyName("passport_number")] public string? PassportNumber { get; set; }
    [JsonPropertyName("document_place")] public string? DocumentPlace { get; set; }
    [JsonPropertyName("is_studing")] public bool IsStuding { get; set; }
}