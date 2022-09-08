using System.Text.Json.Serialization;

namespace GpaFightServerTester.Models
{
    internal record User(
        string Username,
        int YearOfEntrance,
        string Faculty,
        string Department,
        string Password,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)] string? Id = default
    );
}
