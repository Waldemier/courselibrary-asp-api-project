﻿using AutoMapper;
using CourseLibrary.API.Helpers;

namespace CourseLibrary.API.Profiles
{
    public class AuthorProfile: Profile
    {
        public AuthorProfile()
        {
            //From / To
            CreateMap<Entities.Author, Models.AuthorDto>()
                .ForMember(
                    dest => dest.Name,
                    opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
                .ForMember(
                    dest => dest.Age,
                    opt => opt.MapFrom(src => src.DateOfBirthday.GetCurrentAge())
                );
            CreateMap<Models.AuthorForCreateDto, Entities.Author>();
        }        
    }
}