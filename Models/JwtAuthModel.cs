using System.Text.Json.Serialization;

namespace KnowledgeFlowApi.Models;

public class JwtAuthModel {
    public string Message { get; set; }
    [JsonIgnore]
    public int UserId { get; set; }
    public bool IsAuthenticated { get; set; } = false;
    public string? AccessToken { get; set; }
    [JsonIgnore]
    public string? RefreshToken { get; set; }   // save it in Cookies
    public DateTime RefreshTokenExpiration { get; set; }
}