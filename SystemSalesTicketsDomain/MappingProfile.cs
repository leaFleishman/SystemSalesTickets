using AutoMapper;
using SystemSalesTickets.Core.DTOs;
using SystemSalesTicketsDomain.models;


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
        }
    }
}
