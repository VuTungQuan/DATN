using AutoMapper;
using DATN.DTO;
using DATN.Model;

namespace DATN.Helpers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // User mappings
            CreateMap<User, UserDTO>();
            CreateMap<UserCreateDTO, User>();
            CreateMap<UserUpdateDTO, User>()
                .ForAllMembers(opts => opts
                    .Condition((src, dest, srcMember) => srcMember != null));

            // Pitch mappings
            CreateMap<Pitch, PitchDTO>()
                .ForMember(dest => dest.PitchTypeName, 
                    opt => opt.MapFrom(src => src.PitchType != null ? src.PitchType.Name : string.Empty));
            CreateMap<PitchCreateDTO, Pitch>();
            CreateMap<PitchUpdateDTO, Pitch>()
                .ForAllMembers(opts => opts
                    .Condition((src, dest, srcMember) => srcMember != null));

            // PitchType mappings
            CreateMap<PitchType, PitchTypeDTO>();
            CreateMap<PitchTypeCreateDTO, PitchType>();
            CreateMap<PitchTypeUpdateDTO, PitchType>()
                .ForAllMembers(opts => opts
                    .Condition((src, dest, srcMember) => srcMember != null));

            // Booking mappings
            CreateMap<Booking, BookingDTO>()
                .ForMember(dest => dest.UserName, 
                    opt => opt.MapFrom(src => src.User != null ? src.User.FullName : string.Empty))
                .ForMember(dest => dest.PhoneNumber, 
                    opt => opt.MapFrom(src => src.User != null ? src.User.PhoneNumber : string.Empty))
                .ForMember(dest => dest.PitchName, 
                    opt => opt.MapFrom(src => src.Pitch != null ? src.Pitch.Name : string.Empty))
                .ForMember(dest => dest.PitchTypeName, 
                    opt => opt.MapFrom(src => src.Pitch != null && src.Pitch.PitchType != null ? 
                        src.Pitch.PitchType.Name : string.Empty));

            CreateMap<BookingCreateDTO, Booking>();
            CreateMap<BookingUpdateDTO, Booking>()
                .ForAllMembers(opts => opts
                    .Condition((src, dest, srcMember) => srcMember != null));

            // Payment mappings
            CreateMap<Payment, PaymentDTO>();
            CreateMap<PaymentCreateDTO, Payment>();
            CreateMap<PaymentUpdateDTO, Payment>()
                .ForAllMembers(opts => opts
                    .Condition((src, dest, srcMember) => srcMember != null));
        }
    }
} 