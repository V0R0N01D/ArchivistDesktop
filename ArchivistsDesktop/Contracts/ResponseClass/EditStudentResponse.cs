using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ArchivistsDesktop.Contracts.ResponseClass;

public class EditStudentResponse
{
    [JsonPropertyName("student")] public StudentAllDataResponse Student { get; set; } = null!;
    [JsonPropertyName("student_orders")] public List<StudentOrderResponse>? StudentOrders { get; set; }
    [JsonPropertyName("remove_orders")] public List<int>? RemoveOrders { get; set; }
}