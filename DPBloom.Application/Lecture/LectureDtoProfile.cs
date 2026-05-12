using AutoMapper;
using DPBloom.Application.Lecture.Contracts;
using DPBloom.Core.Lecture;
using UUIDNext;

namespace DPBloom.Application.Lecture;

public class LectureDtoProfile : Profile
{
    public LectureDtoProfile()
    {
        CreateMap<LectureDto, LectureModel>().ReverseMap();
        CreateMap<CreateLecture, LectureModel>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Uuid.NewDatabaseFriendly(Database.SqlServer)))
            .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedOn, opt => opt.MapFrom(src => DateTime.UtcNow));
        CreateMap<UpdateLecture, LectureModel>()
            .ForAllMembers(opt => 
                opt.Condition((_, _, srcMember) => 
                    srcMember is not null));
    }
}