using DPBloom.Application.Enrollment.Requests;
using MediatR;

namespace DPBloom.Application.Enrollment.Handlers;

public class UserEnrollmentCheckHandler : IRequestHandler<UserEnrollmentCheckCommand, bool>
{
    private readonly IEnrollmentService _enrollmentService;

    public UserEnrollmentCheckHandler(IEnrollmentService enrollmentService)
    {
        _enrollmentService = enrollmentService;
    }

    public async Task<bool> Handle(UserEnrollmentCheckCommand request, CancellationToken cancellationToken)
    {
        try
        {
            return await _enrollmentService.CheckUserEnrollment(request.UserId, request.CourseId);
            ;
        }
        catch
        {
            return false;
        }
    }
}