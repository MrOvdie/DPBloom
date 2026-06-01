using DPBloom.Application.Enrollment.Contracts;
using MediatR;

namespace DPBloom.Application.Course.Events;

public record UserDismissedEvent(Guid EnrollmentId, UpdateEnrollmentDto UpdateEnrollmentDto) : INotification
{
}