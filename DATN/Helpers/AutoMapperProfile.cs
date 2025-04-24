using AutoMapper;
using DATN.DTO;
using DATN.Model;
using System;

namespace DATN.Helpers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // User mappings
            CreateMap<User, UserDTO>().ReverseMap();
            CreateMap<UserCreateDTO, User>();
            CreateMap<UserUpdateDTO, User>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Pitch mappings
            CreateMap<Pitch, PitchDTO>()
                .ForMember(dest => dest.PitchTypeName, opt => opt.MapFrom(src => src.PitchType.Name));
            CreateMap<PitchCreateDTO, Pitch>();
            CreateMap<PitchUpdateDTO, Pitch>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // PitchType mappings
            CreateMap<PitchType, PitchTypeDTO>().ReverseMap();
            CreateMap<PitchTypeCreateDTO, PitchType>();
            CreateMap<PitchTypeUpdateDTO, PitchType>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Booking mappings
            CreateMap<Booking, BookingDTO>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(dest => dest.PitchName, opt => opt.MapFrom(src => src.Pitch.Name));
                
            
            CreateMap<BookingCreateDTO, Booking>();
            CreateMap<BookingUpdateDTO, Booking>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Payment mappings
            CreateMap<Payment, PaymentDTO>().ReverseMap();
            CreateMap<PaymentCreateDTO, Payment>();

            // Booking -> BookedTimeSlotDTO mapping
            CreateMap<Booking, BookedTimeSlotDTO>()
                .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.StartTime.ToString(@"hh\:mm\:ss")))
                .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.EndTime.ToString(@"hh\:mm\:ss")));
        }
    }
}
