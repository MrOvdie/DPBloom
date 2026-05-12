using DPBloom.Application.Course.Events;
using DPBloom.Application.Enrollment;
using MediatR;

namespace DPBloom.Application.Course.Handlers;

public class UserDismissedEventHandler : INotificationHandler<UserDismissedEvent>
{
    private readonly IEnrollmentService _enrollmentService;

    public UserDismissedEventHandler(IEnrollmentService enrollmentService)
    {
        _enrollmentService = enrollmentService;
    }

    public async Task Handle(UserDismissedEvent notification, CancellationToken cancellationToken)
    {
        await _enrollmentService.UpdateAsync(notification.EnrollmentId, notification.UpdateEnrollment);
    }
}