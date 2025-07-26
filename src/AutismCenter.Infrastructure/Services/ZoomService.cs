using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.ValueObjects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;

namespace AutismCenter.Infrastructure.Services;

public class ZoomService : IZoomService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ZoomService> _logger;
    private readonly string _apiKey;
    private readonly string _apiSecret;
    private readonly string _baseUrl = "https://api.zoom.us/v2";

    public ZoomService(HttpClient httpClient, IConfiguration configuration, ILogger<ZoomService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        _apiKey = _configuration["Zoom:ApiKey"] ?? throw new InvalidOperationException("Zoom API Key not configured");
        _apiSecret = _configuration["Zoom:ApiSecret"] ?? throw new InvalidOperationException("Zoom API Secret not configured");
    }

    public async Task<ZoomMeeting> CreateMeetingAsync(ZoomMeetingRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var token = GenerateJwtToken();
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var meetingData = new
            {
                topic = request.Topic,
                type = 2, // Scheduled meeting
                start_time = request.StartTime.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                duration = request.DurationInMinutes,
                timezone = "UTC",
                agenda = request.Agenda,
                settings = new
                {
                    host_video = true,
                    participant_video = true,
                    join_before_host = request.JoinBeforeHost,
                    mute_upon_entry = true,
                    watermark = false,
                    use_pmi = false,
                    approval_type = 2,
                    audio = "both",
                    auto_recording = "none",
                    waiting_room = request.WaitingRoom,
                    password = request.Password
                }
            };

            var json = JsonSerializer.Serialize(meetingData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/users/me/meetings", content, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to create Zoom meeting. Status: {StatusCode}, Content: {Content}", 
                    response.StatusCode, errorContent);
                throw new InvalidOperationException($"Failed to create Zoom meeting: {response.StatusCode}");
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var meetingResponse = JsonSerializer.Deserialize<ZoomMeetingResponse>(responseContent);

            if (meetingResponse == null)
                throw new InvalidOperationException("Invalid response from Zoom API");

            return new ZoomMeeting(
                meetingResponse.id.ToString(),
                meetingResponse.topic,
                meetingResponse.join_url,
                meetingResponse.start_url,
                DateTime.Parse(meetingResponse.start_time),
                meetingResponse.duration,
                meetingResponse.password,
                ZoomMeetingStatus.Waiting);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Zoom meeting");
            throw;
        }
    }

    public async Task<ZoomMeeting> GetMeetingAsync(string meetingId, CancellationToken cancellationToken = default)
    {
        try
        {
            var token = GenerateJwtToken();
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync($"{_baseUrl}/meetings/{meetingId}", cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    throw new InvalidOperationException($"Meeting with ID {meetingId} not found");
                
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to get Zoom meeting. Status: {StatusCode}, Content: {Content}", 
                    response.StatusCode, errorContent);
                throw new InvalidOperationException($"Failed to get Zoom meeting: {response.StatusCode}");
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var meetingResponse = JsonSerializer.Deserialize<ZoomMeetingResponse>(responseContent);

            if (meetingResponse == null)
                throw new InvalidOperationException("Invalid response from Zoom API");

            return new ZoomMeeting(
                meetingResponse.id.ToString(),
                meetingResponse.topic,
                meetingResponse.join_url,
                meetingResponse.start_url,
                DateTime.Parse(meetingResponse.start_time),
                meetingResponse.duration,
                meetingResponse.password,
                MapZoomStatus(meetingResponse.status));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Zoom meeting {MeetingId}", meetingId);
            throw;
        }
    }

    public async Task UpdateMeetingAsync(string meetingId, ZoomMeetingRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var token = GenerateJwtToken();
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var meetingData = new
            {
                topic = request.Topic,
                start_time = request.StartTime.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                duration = request.DurationInMinutes,
                timezone = "UTC",
                agenda = request.Agenda,
                settings = new
                {
                    host_video = true,
                    participant_video = true,
                    join_before_host = request.JoinBeforeHost,
                    mute_upon_entry = true,
                    watermark = false,
                    use_pmi = false,
                    approval_type = 2,
                    audio = "both",
                    auto_recording = "none",
                    waiting_room = request.WaitingRoom,
                    password = request.Password
                }
            };

            var json = JsonSerializer.Serialize(meetingData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PatchAsync($"{_baseUrl}/meetings/{meetingId}", content, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to update Zoom meeting. Status: {StatusCode}, Content: {Content}", 
                    response.StatusCode, errorContent);
                throw new InvalidOperationException($"Failed to update Zoom meeting: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating Zoom meeting {MeetingId}", meetingId);
            throw;
        }
    }

    public async Task DeleteMeetingAsync(string meetingId, CancellationToken cancellationToken = default)
    {
        try
        {
            var token = GenerateJwtToken();
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.DeleteAsync($"{_baseUrl}/meetings/{meetingId}", cancellationToken);
            
            if (!response.IsSuccessStatusCode && response.StatusCode != System.Net.HttpStatusCode.NotFound)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to delete Zoom meeting. Status: {StatusCode}, Content: {Content}", 
                    response.StatusCode, errorContent);
                throw new InvalidOperationException($"Failed to delete Zoom meeting: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting Zoom meeting {MeetingId}", meetingId);
            throw;
        }
    }

    public async Task<IEnumerable<ZoomMeeting>> GetUserMeetingsAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var token = GenerateJwtToken();
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync($"{_baseUrl}/users/{userId}/meetings", cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to get user meetings. Status: {StatusCode}, Content: {Content}", 
                    response.StatusCode, errorContent);
                throw new InvalidOperationException($"Failed to get user meetings: {response.StatusCode}");
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var meetingsResponse = JsonSerializer.Deserialize<ZoomMeetingsListResponse>(responseContent);

            if (meetingsResponse?.meetings == null)
                return Enumerable.Empty<ZoomMeeting>();

            return meetingsResponse.meetings.Select(m => new ZoomMeeting(
                m.id.ToString(),
                m.topic,
                m.join_url,
                m.start_url,
                DateTime.Parse(m.start_time),
                m.duration,
                m.password,
                MapZoomStatus(m.status)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user meetings for {UserId}", userId);
            throw;
        }
    }

    private string GenerateJwtToken()
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_apiSecret);
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("iss", _apiKey),
                new Claim("exp", DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds().ToString())
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private static ZoomMeetingStatus MapZoomStatus(string status)
    {
        return status?.ToLower() switch
        {
            "waiting" => ZoomMeetingStatus.Waiting,
            "started" => ZoomMeetingStatus.Started,
            "ended" => ZoomMeetingStatus.Ended,
            _ => ZoomMeetingStatus.Waiting
        };
    }

    // Internal DTOs for Zoom API responses
    private record ZoomMeetingResponse(
        long id,
        string topic,
        string join_url,
        string start_url,
        string start_time,
        int duration,
        string? password,
        string status
    );

    private record ZoomMeetingsListResponse(
        int page_count,
        int page_number,
        int page_size,
        int total_records,
        ZoomMeetingResponse[] meetings
    );
}