using DPBloom.Application.Enrollment.Contracts;
using MediatR;

namespace DPBloom.Application.Course.Events;

public record UserEnrolledEvent(CreateEnrollmentDto CreateEnrollmentDto) : INotification;