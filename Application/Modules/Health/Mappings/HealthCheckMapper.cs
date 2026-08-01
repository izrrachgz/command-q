using Application.Interfaces;
using Riok.Mapperly.Abstractions;
using Application.Modules.Health.Dtos;

namespace Application.Modules.Health.Mappings
{
    [Mapper]
    public partial class HealthCheckMapper : IMapper<HealthCheckLog, HealthCheckResponseModel>
    {
        [MapperIgnoreTarget(nameof(HealthCheckLog.Id))]
        [MapperIgnoreTarget(nameof(HealthCheckLog.Deleted))]
        public partial HealthCheckLog Map(HealthCheckResponseModel model);

        [MapperIgnoreSource(nameof(HealthCheckLog.Id))]
        [MapperIgnoreSource(nameof(HealthCheckLog.Deleted))]
        public partial HealthCheckResponseModel Map(HealthCheckLog entity);
    }
}
