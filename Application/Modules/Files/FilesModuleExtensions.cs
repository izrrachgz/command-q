using FluentValidation;
using Domain.Modules.Files;
using Common.Models.Response;
using Application.Extensions;
using Application.Abstractions;
using Application.Modules.Files.Dtos;
using Application.Modules.Files.Queries;
using Application.Modules.Files.Commands;
using Application.Modules.Files.Mappings;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Modules.Files
{
    /// <summary>
    /// Provides dependency-injection registration for the Files module.
    /// </summary>
    public static class FilesModuleExtensions
    {
        /// <summary>
        /// Registers the Files application handlers, validators and mappers.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        /// <returns>The configured service collection.</returns>
        public static IServiceCollection AddFilesModule(this IServiceCollection services)
        {
            services.AddApplicationRequest<UploadFileCommand, BasicResponse, UploadFileCommandHandler>();
            services.AddApplicationRequest<GetAccountFilesQuery, PagedResponse<AccountFileModel>, GetAccountFilesQueryHandler>();

            services.AddScoped<IValidator<UploadFileCommand>, UploadFileCommandValidator>();
            services.AddScoped<IValidator<GetAccountFilesQuery>, GetAccountFilesQueryValidator>();

            services.AddSingleton<IMapper<AccountFile, AccountFileModel>, AccountFileMapper>();

            return services;
        }
    }
}
