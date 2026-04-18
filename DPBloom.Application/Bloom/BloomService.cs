using AutoMapper;
using DPBloom.Application.Bloom.Contracts;
using DPBloom.Application.Exam.Contracts;
using FluentValidation;
using TestOfTesting.DTOs;

namespace DPBloom.Application.Bloom;

public class BloomService : IBloomService
{
    /*private readonly IValidator<CreateExamDto> _createValidator;
    private readonly IValidator<UpdateExamDto> _updateValidator;*/
    private readonly IMapper _mapper;

    public BloomService(IMapper mapper)
    {
        _mapper = mapper;
    }

    public Task<BloomAnalysisDto> AnalyzeAndSaveAttemptAsync(Guid attemptResultId)
    {
        throw new NotImplementedException();
    }

    public Task<BloomAnalysisDto> AnalyzeAndSaveCourseAsync(Guid attemptResultId)
    {
        throw new NotImplementedException();
    }

    public Task<BloomAnalysisDto?> GetAnalysisByAttemptResultIdAsync(Guid attemptResultId)
    {
        throw new NotImplementedException();
    }
}