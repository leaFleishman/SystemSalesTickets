using AutoMapper;
using SystemSalesTickets.Core.DTOs;
using SystemSalesTickets.Core.Models;

namespace SystemSalesTickets.Core
{
    public class MappingProfile:Profile
    {
        public MappingProfile()
        {
            CreateMap<UserDTO, User>().ReverseMap();
            CreateMap<SeatDTO, Seat>().ReverseMap();
            CreateMap<EventDTO, Event>().ReverseMap();
            CreateMap<OrderDTO, Order>().ReverseMap();
            CreateMap<UserLogDTO, User>().ReverseMap();
            CreateMap<OrderLogDTO, Order>().ReverseMap();
            CreateMap<SeatLogDTO, Seat>().ReverseMap();
            CreateMap<EventSeat, EventSeatDTO>()
                .ForMember(dest => dest.Row, opt => opt.MapFrom(src => src.Seat.Row))
                .ForMember(dest => dest.Line, opt => opt.MapFrom(src => src.Seat.Line));
            CreateMap<EventSeatDTO, EventSeat>()
                .ForMember(dest => dest.Seat, opt => opt.Ignore());
            CreateMap<RegisterRequestDTO, User>();
        }
    }
}
