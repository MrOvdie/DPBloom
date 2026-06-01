using AutoMapper;
using DPBloom.Application.Lecture.Contracts;
using DPBloom.Core.Lecture;
using UUIDNext;

namespace DPBloom.Application.Lecture;

public class LectureDtoProfile : Profile
{
    public LectureDtoProfile()
    {
        CreateMap<LectureModel, LectureDto>();
        CreateMap<LectureModel, LectureDetailsDto>().ReverseMap();

        CreateMap<CreateLectureDto, LectureModel>();
        CreateMap<UpdateLectureDto, LectureModel>()
            .ForAllMembers(opt => 
                opt.Condition((_, _, srcMember) => 
                    srcMember is not null));
    }
}