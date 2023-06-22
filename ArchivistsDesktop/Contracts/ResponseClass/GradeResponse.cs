using System.Text.Json.Serialization;

namespace ArchivistsDesktop.Contracts.ResponseClass;

public class GradeResponse
{
    [JsonPropertyName("id")] public int? Id { get; set; }
    [JsonPropertyName("lesson")] public LessonResponse Lesson { get; set; } = null!;
    [JsonPropertyName("score")] public short Score { get; set; }
}