using AutoMapper;
using MyApp.Application.People;
using MyApp.Domain.Models;

namespace MyApp.Application.Mapping
{
    // The Profile class is part of AutoMapper library.
    // AutoMapper is a popular object-to-object mapping library that helps to map properties from one object to another.
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // The CreateMap method is used to define a mapping between two types.
            // The default mapping direction is from source (Person) to destination (PersonDto).
            // The ReverseMap method creates a two-way mapping.
            CreateMap<Person, PersonDto>().ReverseMap();
        }
    }
}