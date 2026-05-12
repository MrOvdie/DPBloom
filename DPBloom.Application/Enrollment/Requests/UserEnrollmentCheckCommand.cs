using MediatR;

namespace DPBloom.Application.Enrollment.Requests;

public record UserEnrollmentCheckCommand(Guid UserId, Guid CourseId) : IRequest<bool>;