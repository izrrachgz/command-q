using Application.Abstractions;
using Riok.Mapperly.Abstractions;
using Application.Modules.Configuration.Dtos;

namespace Application.Modules.Configuration.Mappings
{
    [Mapper]
    internal partial class ApplicationConfigurationMapper
        : IMapper<Domain.Modules.Configuration.ApplicationConfiguration, ApplicationConfigurationModel>
    {
        [MapperIgnoreTarget(nameof(Domain.Modules.Configuration.ApplicationConfiguration.Modified))]
        [MapperIgnoreTarget(nameof(Domain.Modules.Configuration.ApplicationConfiguration.Deleted))]
        public partial Domain.Modules.Configuration.ApplicationConfiguration Map(ApplicationConfigurationModel model);

        [MapperIgnoreSource(nameof(Domain.Modules.Configuration.ApplicationConfiguration.Modified))]
        [MapperIgnoreSource(nameof(Domain.Modules.Configuration.ApplicationConfiguration.Deleted))]
        public partial ApplicationConfigurationModel Map(Domain.Modules.Configuration.ApplicationConfiguration entity);
    }
}
