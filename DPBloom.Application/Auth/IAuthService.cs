namespace DPBloom.Application.Auth;

public interface IAuthService
{
    Task<bool> RegisterAsync(string email, string password, string firstName, string lastName);
    Task<string?> LoginAsync(string email, string password); //TODO: add login. Return token???
}