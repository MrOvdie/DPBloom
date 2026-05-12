using DPBloom.Application.Course.Events;
using DPBloom.Application.Enrollment;
using MediatR;

namespace DPBloom.Application.Course.Handlers;

public class UserEnrolledEventHandler : INotificationHandler<UserEnrolledEvent>
{
    private readonly IEnrollmentService _enrollmentService;

    public UserEnrolledEventHandler(IEnrollmentService enrollmentService)
    {
        _enrollmentService = enrollmentService;
    }

    public async Task Handle(UserEnrolledEvent notification, CancellationToken cancellationToken)
    {
        await _enrollmentService.CreateAsync(notification.CreateEnrollment);
    }
}