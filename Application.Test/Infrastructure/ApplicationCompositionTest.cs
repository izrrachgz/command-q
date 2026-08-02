using MediatR;
using Application.Modules.Identity.Accounts.Services;
using Application.Modules.Identity.AccountActivity.Services;
using Localization.Abstractions;
using Persistence.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Test.Infrastructure
{
    public sealed class ApplicationCompositionTest(IServiceProvider serviceProvider)
    {
        [Fact]
        public void Application_Composition_Should_Resolve_Core_Services()
        {
            _ = serviceProvider.GetRequiredService<ISender>();
            _ = serviceProvider.GetRequiredService<ITextProvider>();
            _ = serviceProvider.GetRequiredService<IReadOnlyRepository>();
            _ = serviceProvider.GetRequiredService<IWritableRepository>();
            _ = serviceProvider.GetRequiredService<AccountService>();
            _ = serviceProvider.GetRequiredService<IAccountActivityRecorder>();
        }

        [Fact]
        public void Writable_Repository_Should_Resolve_Existing_Migration_Path()
        {
            var repository = serviceProvider.GetRequiredService<IWritableRepository>();

            Assert.True(Directory.Exists(repository.MigrationsPath), repository.MigrationsPath);
        }
    }
}
