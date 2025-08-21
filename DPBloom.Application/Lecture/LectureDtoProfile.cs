using AutoMapper;
using DPBloom.Application.Lecture.Contracts;
using DPBloom.Core.Lecture;

namespace DPBloom.Application.Lecture;

public class LectureDtoProfile : Profile
{
    public LectureDtoProfile()
    {
        CreateMap<LectureDto, LectureModel>().ReverseMap();
        CreateMap<UpdateLecture, LectureModel>()
            .ForAllMembers(opt => 
                opt.Condition((src, dest, srcMember) => 
                    srcMember != null));
        CreateMap<CreateLecture, LectureModel>();
    }
}