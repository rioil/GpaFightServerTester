using System.Text.Json.Serialization;

namespace GpaFightServerTester.Models
{
    internal record Ranking(string Gpa, int Rank, [property: JsonPropertyName("ranking")] RankingItem[] Rankings);

    internal record RankingItem(int Rank, string Username, string Faculty, string Department, string Gpa);
}
