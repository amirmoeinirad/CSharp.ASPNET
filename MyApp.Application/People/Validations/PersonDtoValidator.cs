using FluentValidation;

namespace MyApp.Application.People.Validations
{
    // The AbstractValidator class is part of the FluentValidation library.
    // It is used to create validation rules for a specific type.
    public class PersonDtoValidator : AbstractValidator<PersonDto>
    {
        public PersonDtoValidator()
        {
            // RuleFor() method is used to define validation rules for a specific property.
            // 'x' is of type PersonDto.
            // Here, we are defining rules for the FirstName and LastName properties.
            // FirstName and LastName must not be empty and have a maximum length of 100 characters.
            RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        }
    }
}