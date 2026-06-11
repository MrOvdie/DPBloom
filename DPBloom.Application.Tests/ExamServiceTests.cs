using AutoMapper;
using DPBloom.Application.Auth;
using DPBloom.Application.Enrollment;
using DPBloom.Application.Exam;
using DPBloom.Application.Exam.Contracts.Create;
using DPBloom.Application.Exam.Contracts.Update;
using DPBloom.Core.Exam;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;

namespace DPBloom.Application.Tests;

public class ExamServiceTests
{
    private readonly Mock<IExamRepository> _examRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IValidator<CreateExamDto>> _createValidatorMock;
    private readonly Mock<IValidator<UpdateExamDto>> _updateValidatorMock;
    private readonly Mock<IEnrollmentRepository> _enrollmentRepositoryMock;
    
    private readonly ExamService _sut; // System Under Test

    public ExamServiceTests()
    {
        _examRepositoryMock = new Mock<IExamRepository>();
        _mapperMock = new Mock<IMapper>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _createValidatorMock = new Mock<IValidator<CreateExamDto>>();
        _updateValidatorMock = new Mock<IValidator<UpdateExamDto>>();
        _enrollmentRepositoryMock = new Mock<IEnrollmentRepository>();

        _sut = new ExamService(
            _examRepositoryMock.Object,
            _mapperMock.Object,
            _createValidatorMock.Object,
            _updateValidatorMock.Object,
            _currentUserServiceMock.Object,
            _enrollmentRepositoryMock.Object
        );
    }

    [Fact]
    public async Task CreateExamAsync_WhenValidationFails_ThrowsValidationException()
    {
        // Arrange 
        var courseId = Guid.NewGuid();
        var createDto = new CreateExamDto();
        var validationFailures = new List<ValidationFailure> 
        { 
            new ValidationFailure("Title", "Title is required") 
        };
        var validationResult = new ValidationResult(validationFailures);

        _createValidatorMock
            .Setup(v => v.ValidateAsync(createDto, default))
            .ReturnsAsync(validationResult);

        // Act
        Func<Task> act = async () => await _sut.CreateExamAsync(courseId, createDto);

        // Assert 
        await act.Should().ThrowAsync<ValidationException>();
        
        _examRepositoryMock.Verify(
            repo => repo.AddExamWithDetailsAsync(It.IsAny<ExamAggregateModel>()), 
            Times.Never);
    }

    [Fact]
    public async Task UpdateExamAsync_WhenExamDoesNotExist_ThrowsKeyNotFoundException()
    {
        // Arrange 
        var examId = Guid.NewGuid();
        var updateDto = new UpdateExamDto();

        _updateValidatorMock
            .Setup(v => v.ValidateAsync(updateDto, default))
            .ReturnsAsync(new ValidationResult()); 

        _examRepositoryMock
            .Setup(repo => repo.GetWithQuestionsAsync(examId))
            .ReturnsAsync((ExamAggregateModel)null); 

        // Act 
        Func<Task> act = async () => await _sut.UpdateExamAsync(examId, updateDto);

        // Assert 
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Exam not found");
    }
}