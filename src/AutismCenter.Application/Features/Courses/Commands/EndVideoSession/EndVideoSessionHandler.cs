using MediatR;
using Microsoft.Extensions.Logging;
using AutismCenter.Domain.Interfaces;

namespace AutismCenter.Application.Features.Courses.Commands.EndVideoSession;

public class EndVideoSessionHandler : IRequestHandler<EndVideoSessionCommand>
{
    private readonly IVideoAccessService _videoAccessService;
    private readonly ILogger<EndVideoSessionHandler> _logger;

    public EndVideoSessionHandler(
        IVideoAccessService videoAccessService,
        ILogger<EndVideoSessionHandler> logger)
    {
        _videoAccessService = videoAccessService;
        _logger = logger;
    }

    public async Task Handle(EndVideoSessionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.SessionId))
            {
                throw new ArgumentException("Session ID cannot be empty", nameof(request.SessionId));
            }

            await _videoAccessService.EndStreamingSessionAsync(request.SessionId);

            _logger.LogInformation("Successfully ended video streaming session {SessionId}", request.SessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ending video streaming session {SessionId}", request.SessionId);
            throw;
        }
    }
}