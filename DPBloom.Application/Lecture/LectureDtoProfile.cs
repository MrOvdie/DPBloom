using AutoMapper;
using DPBloom.Core.Lecture;

namespace DPBloom.Application.Lecture;

public class LectureDtoProfile : Profile
{
    public LectureDtoProfile()
    {
        CreateMap<LectureDto, LectureModel>().ReverseMap();
    }
}