using System.Text.Json.Serialization;

namespace BankingSystem.Core.Response;

public record RegisterResponse
{
    public string? FirstName { get; init; }

    public string? Lastname { get; init; }

    public string? IdNumber { get; init; }

    public DateTime BirthDate { get; init; }

    public string? Email { get; init; }

    [JsonIgnore]
    public string Role { get; set; }
}