using System;
using System.Text.Json.Serialization;

namespace ArchivistsDesktop.Contracts.ResponseClass;

public class GroupResponse
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("group_number")] public string GroupNumber { get; set; } = null!;

    [JsonPropertyName("date_end_education")]
    public DateOnly? DateEndEducation { get; set; }

    [JsonPropertyName("type_education")] public TypeEducationResponse TypeEducation { get; set; } = null!;
    [JsonPropertyName("speciality")] public SpecialityResponse Speciality { get; set; } = null!;
    [JsonPropertyName("year")] public short Year { get; set; }
}