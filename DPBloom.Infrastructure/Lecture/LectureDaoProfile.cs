using AutoMapper;
using DPBloom.Core;
using DPBloom.Core.Lecture;

namespace DPBloom.Infrastructure.Lecture;

public class LectureDaoProfile : Profile
{
    public LectureDaoProfile()
    {
        CreateMap<LectureDao, LectureModel>().ReverseMap();
    }
}