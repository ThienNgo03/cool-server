namespace BFF.Authentication.Register.Messager;

public record Message(Guid id, string? profilePicture, string name, string email, string phoneNumber);
