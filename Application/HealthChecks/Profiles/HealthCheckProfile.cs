#nullable enable
using AutoMapper;
using Domain.Entities;
using Application.HealthChecks.Dtos;

namespace Application.HealthChecks.Profiles
{
    internal sealed class HealthCheckProfile : Profile
    {
        public HealthCheckProfile()
        {
            CreateMap<HealthCheckLog, HealthCheckResponseModel>();
        }
    }
}
