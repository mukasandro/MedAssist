namespace MedAssist.Application.Requests;

public record CreateStaticContentRequest(
    string Code,
    string? Title,
    string Value);
