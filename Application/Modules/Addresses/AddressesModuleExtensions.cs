using FluentValidation;
using Common.Models.Response;
using Application.Extensions;
using Application.Abstractions;
using Domain.Modules.Addresses;
using Application.Modules.Addresses.Dtos;
using Application.Modules.Addresses.Queries;
using Application.Modules.Addresses.Commands;
using Application.Modules.Addresses.Mappings;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Modules.Addresses
{
    /// <summary>
    /// Provides dependency-injection registration for the Addresses module.
    /// </summary>
    public static class AddressesModuleExtensions
    {
        /// <summary>
        /// Registers the Addresses application handlers, validators and mappers.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        /// <returns>The configured service collection.</returns>
        public static IServiceCollection AddAddressesModule(this IServiceCollection services)
        {
            services.AddApplicationRequest<CreateAddressCommand, BasicResponse, CreateAddressCommandHandler>();
            services.AddApplicationRequest<GetAddressByAccountIdQuery, DetailResponse<AddressModel>, GetAddressByAccountIdQueryHandler>();

            services.AddScoped<IValidator<CreateAddressCommand>, CreateAddressCommandValidator>();
            services.AddScoped<IValidator<GetAddressByAccountIdQuery>, GetAddressByAccountIdQueryValidator>();

            services.AddSingleton<IMapper<Address, AddressModel>, AddressMapper>();

            return services;
        }
    }
}
