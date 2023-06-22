using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace ArchivistsDesktop.Contracts.ResponseClass;

public class CertificateEducationResponse
{
    [JsonPropertyName("id")] public int? Id { get; set; }
    [JsonPropertyName("identify")] public string Identify { get; set; } = null!;
    [JsonPropertyName("type")] public CertificateEducationTypeReposponse Type { get; set; } = null!;
    [JsonPropertyName("date_issue")] public DateOnly? DateIssue { get; set; }

    [JsonPropertyName("average_grade")] public decimal? AverageGrade { get; set; }
    [JsonPropertyName("is_original")] public bool IsOriginal { get; set; }
    [JsonPropertyName("grades")] public List<GradeResponse>? Grades { get; set; }
    [JsonPropertyName("remove_grades")] public List<int>? RemoveGrades { get; set; }
}