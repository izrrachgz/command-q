using Domain.Entities;
using Application.Interfaces;
using Riok.Mapperly.Abstractions;
using Application.HealthChecks.Dtos;

namespace Application.HealthChecks.Mappings
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
