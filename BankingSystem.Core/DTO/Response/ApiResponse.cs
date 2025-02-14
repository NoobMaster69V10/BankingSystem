using System.Net;

namespace BankingSystem.Core.DTO.Response;

public class ApiResponse
{
    public HttpStatusCode StatusCode { get; set; }
    public bool IsSuccess { get; set; } = true;
    public List<String> ErrorMessages { get; set; } = new();
    public Object  Result { get; set; }
}