using AutoMapper;
using Netzon.Api.DTOs;
using Netzon.Api.Entities;

namespace  Netzon.Api.Helpers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<User, UserDTO>();
            CreateMap<UserDTO, User>();
        }
    }
}