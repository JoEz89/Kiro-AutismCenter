namespace AutismCenter.Application.Features.Users.Queries.Admin.ExportUsers;

public record ExportUsersResponse(
    byte[] FileContent,
    string ContentType,
    string FileName
);