namespace MedAssist.Application.Requests;

public record UpdateStaticContentRequest(
    string Code,
    string? Title,
    string Value);
