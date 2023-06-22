using System;
using System.Text.Json.Serialization;

namespace ArchivistsDesktop.Contracts.ResponseClass
{
    public class StudentsResponse
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("firstname")]
        public string FirstName { get; set; } = null!;
        [JsonPropertyName("lastname")]
        public string LastName { get; set; } = null!;
        [JsonPropertyName("patronymic")]
        public string? Patronymic { get; set; }
        [JsonPropertyName("date_birthday")]
        public DateOnly DateBirthday { get; set; }
        [JsonPropertyName("is_studing")]
        public bool? IsStuding { get; set; }
        [JsonPropertyName("averange_grade")]
        public decimal? AverageGrade { get; set; }
        [JsonPropertyName("group")]
        public string? GroupTitle { get; set; }
        [JsonPropertyName("speciality")]
        public string? SpecialityTitle { get; set; }

        public string IsStudingTitle => IsStuding is true ? "Да" : "Нет";
    }
}
