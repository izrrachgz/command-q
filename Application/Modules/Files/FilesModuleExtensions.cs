using Microsoft.Extensions.DependencyInjection;

namespace Application.Modules.Files
{
    public static class FilesModuleExtensions
    {
        public static IServiceCollection AddFilesModule(this IServiceCollection services)
        {
            return services;
        }
    }
}
