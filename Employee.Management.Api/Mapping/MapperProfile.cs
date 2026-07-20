using AutoMapper;
using Employee.Management.Models.DatabaseModels;
using Employee.Management.Models.Dtos;
using Employee.Management.Models.Dtos.RequestDtos;
using Employee.Management.Models.Dtos.ResponseDtos;

// 'Employee' is both a namespace root and an entity type — alias the entity to disambiguate.
using EmployeeEntity = Employee.Management.Models.DatabaseModels.Employee;

namespace Employee.Management.Api.Mapping
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            #region DatabaseModels <-> DTOs
            // TenantDto is a read-facing subset of Tenant (no TimeZone / audit / nav collections).
            CreateMap<Tenant, TenantDto>().ReverseMap();

            CreateMap<Organization, OrganizationDto>().ReverseMap();

            CreateMap<Department, DepartmentDto>().ReverseMap();

            CreateMap<EmployeeEntity, EmployeeDto>().ReverseMap();

            CreateMap<Manager, ManagerDto>().ReverseMap();

            CreateMap<ReportingLine, ReportingLineDto>().ReverseMap();

            // ---- DomainUser ----
            // UserId maps to the entity's DomainUserId. JobTitle / DepartmentId /
            // ManagerId / SupervisorId have no source on DomainUser (they derive from
            // Employee / ReportingLine) — left at default for the service layer to fill in.
            CreateMap<DomainUser, DomainUserDto>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.DomainUserId))
                .ReverseMap()
                .ForMember(dest => dest.DomainUserId, opt => opt.MapFrom(src => src.UserId));

            // ---- ApplicationUser (ASP.NET Identity) ----
            // IdentityUserDTO exposes non-sensitive identity fields only (no password/concurrency stamps).
            CreateMap<ApplicationUser, IdentityUserDTO>().ReverseMap();

            // ---- Request DTOs -> entities ----
            // Save*Dto carry only the create-relevant columns; PKs, Uid, audit
            // timestamps and nav properties are left at default for the DB / service layer.
            CreateMap<CreateTenantDto, Tenant>();
            CreateMap<SaveOrganizationDto, Organization>();
            CreateMap<SaveDepartmentDto, Department>();
            CreateMap<SaveEmployeeDto, EmployeeEntity>();
            CreateMap<SaveManagerDto, Manager>();
            CreateMap<SaveReportingLineDto, ReportingLine>();

            // ---- Entities -> Response DTOs (one-way, flat — no back-navigation) ----
            // Entity <-> {Entity}Dto pairs above are ReverseMap()'d, so mapping a nested
            // nav property (e.g. Department.Organization) reuses AutoMapper's per-request
            // instance cache and reproduces the entity graph's real reference cycles
            // (Org -> Departments[i].Organization === the same Org instance), which
            // System.Text.Json then refuses to serialize. These Response DTOs omit the
            // upward nav property entirely so the tree only ever grows downward.
            CreateMap<Organization, OrganizationResponseDto>();
            CreateMap<Department, DepartmentResponseDto>();
            // Department is still the parent when nested (Organization -> Department ->
            // Employee/Manager); Ignore() keeps that direction one-way so re-including
            // Employees/Managers later (e.g. .ThenInclude(d => d.Employees)) can't
            // reopen the same cycle through Employee.Department / Manager.Department.
            CreateMap<EmployeeEntity, EmployeeResponseDto>()
                .ForMember(dest => dest.Department, opt => opt.Ignore());
            CreateMap<Manager, ManagerResponseDto>()
                .ForMember(dest => dest.Department, opt => opt.Ignore());
            CreateMap<DomainUser, DomainUserResponseDto>();
            #endregion
        }
    }
}
