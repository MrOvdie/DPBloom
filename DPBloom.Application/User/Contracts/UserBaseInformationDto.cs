namespace DPBloom.Application.User.Contracts;

public class UserBaseInformationDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }    
    public string? MiddleName { get; set; }    
    public string LastName { get; set; }
    public string? Group { get; set; }
    public string? Faculty { get; set; }
}