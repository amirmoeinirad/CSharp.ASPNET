using AutoMapper;
using MediatR;
using MyApp.Application.Interfaces;
using MyApp.Application.People.Queries;

namespace MyApp.Application.People.QueryHandlers
{
    public class GetAllPeopleQueryHandler : IRequestHandler<GetAllPeopleQuery, List<PersonDto>>
    {
        // This field holds a reference to the person repository.
        // Here, we are using the EF implementation of the repository.
        private readonly IPersonRepository _repo;

        // This field holds a reference to the AutoMapper instance.
        private readonly IMapper _mapper;

        // Constructor that takes the repository and mapper as parameters.
        // These are provided via dependency injection in ASP.NET.
        public GetAllPeopleQueryHandler(IPersonRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        // Handle method to process the GetAllPeopleQuery.
        public async Task<List<PersonDto>> Handle(GetAllPeopleQuery request, CancellationToken cancellationToken)
        {
            // Retrieve all people from the repository.
            // Here, the Query Handler is calling the Repository to get data.
            var people = await _repo.GetAllPeople();

            // The Map method is from AutoMapper.
            // It converts the list of Person entities to a list of PersonDto.
            return _mapper.Map<List<PersonDto>>(people);
        }
    }
}