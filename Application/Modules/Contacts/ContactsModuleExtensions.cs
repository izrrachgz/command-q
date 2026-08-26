using FluentValidation;
using Common.Models.Response;
using Application.Extensions;
using Domain.Modules.Contacts;
using Application.Abstractions;
using Application.Modules.Contacts.Dtos;
using Application.Modules.Contacts.Queries;
using Application.Modules.Contacts.Commands;
using Application.Modules.Contacts.Mappings;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Modules.Contacts
{
    /// <summary>
    /// Provides dependency-injection registration for the Contacts module.
    /// </summary>
    public static class ContactsModuleExtensions
    {
        /// <summary>
        /// Registers the Contacts application handlers, validators and mappers.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        /// <returns>The configured service collection.</returns>
        public static IServiceCollection AddContactsModule(this IServiceCollection services)
        {
            services.AddApplicationRequest<CreateContactCommand, BasicResponse, CreateContactCommandHandler>();
            services.AddApplicationRequest<GetContactByAccountIdQuery, DetailResponse<ContactModel>, GetContactByAccountIdQueryHandler>();

            services.AddScoped<IValidator<CreateContactCommand>, CreateContactCommandValidator>();
            services.AddScoped<IValidator<GetContactByAccountIdQuery>, GetContactByAccountIdQueryValidator>();

            services.AddSingleton<IMapper<Contact, ContactModel>, ContactMapper>();

            return services;
        }
    }
}
