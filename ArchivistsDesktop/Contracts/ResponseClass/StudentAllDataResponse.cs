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

    [JsonPropertyName("type_certificate_education")]
    public CertificateEducationTypeReposponse? TypeCertificate { get; set; }

    [JsonPropertyName("is_released")] public bool IsReleased { get; set; }

    [JsonPropertyName("diploma")] public DiplomResponse? Diploma { get; set; }
}