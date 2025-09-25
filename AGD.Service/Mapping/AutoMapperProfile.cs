using AGD.Repositories.Models;
using AGD.Service.DTOs.Request;
using AGD.Service.DTOs.Response;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGD.Service.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // map req with user
            CreateMap<User,RegisterUserRequest>().ReverseMap();

            // map res with user



            CreateMap<User, RegisterUserResponse>().ReverseMap();
        }
    }
}
