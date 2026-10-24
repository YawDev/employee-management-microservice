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
            CreateMap<Tenant, TenantDto>()
            .ForMember(dest => dest.OrganizationCount,
                            opt => opt.MapFrom(src => src.Organizations.Count()));
                            
            CreateMap<TenantDto, Tenant>().ForMember(dest => dest.Organizations, opt => opt.Ignore());

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
                .ForMember(dest => dest.Department, opt => opt.MapFrom(src => src.Department.Name))
                .ForMember(dest => dest.OrganizationId, opt => opt.MapFrom(src => src.Department.OrganizationId))
                .ForMember(dest => dest.ManagerId, opt => opt.MapFrom(src => src.Manager != null ? src.Manager.ManagerId : (Guid?)null));
            // HR fields come from the manager's own Employee row.
            CreateMap<Manager, ManagerInfoResponseDto>()
                .ForMember(dest => dest.Department, opt => opt.MapFrom(src => src.Department.Name))
                .ForMember(dest => dest.OrganizationId, opt => opt.MapFrom(src => src.Department.OrganizationId))
                .ForMember(dest => dest.JobTitle, opt => opt.MapFrom(src => src.Employee.JobTitle))
                .ForMember(dest => dest.HireDate, opt => opt.MapFrom(src => src.Employee.HireDate))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.Employee.EndDate))
                .ForMember(dest => dest.Salary, opt => opt.MapFrom(src => src.Employee.Salary))
                .ForMember(dest => dest.EmploymentStatus, opt => opt.MapFrom(src => src.Employee.EmploymentStatus));
            CreateMap<DomainUser, DomainUserResponseDto>()
                .ForMember(dest => dest.TenantName, opt => opt.MapFrom(src => src.Tenant.Name));
            CreateMap<ReportingLine, ReportLineResponseDto>()
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.Report.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.Report.LastName));

            // get-department-info: names come from the member's DomainUser (loaded by GetDepartmentInfoAsync).
            // A manager is also an employee; list them once, under Managers.
            CreateMap<Department, DepartmentDetailsResponseDto>()
                .ForMember(dest => dest.Employees, opt => opt.MapFrom(src =>
                    src.Employees.Where(e => !src.Managers.Any(m => m.DomainUserId == e.DomainUserId))));
            // EmployeeId / ManagerId / JobTitle map by name.
            CreateMap<EmployeeEntity, DepartmentEmployeeResponseDto>()
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.DomainUser.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.DomainUser.LastName));
            CreateMap<Manager, DepartmentManagerResponseDto>()
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.DomainUser.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.DomainUser.LastName));
            #endregion

            #region EntityDtos -> ResponseDtos
            CreateMap<OrganizationDto, OrganizationResponseDto>();
            CreateMap<DepartmentDto, DepartmentResponseDto>();
            CreateMap<EmployeeDto, EmployeeResponseDto>()
                .ForMember(dest => dest.OrganizationId, opt => opt.MapFrom(src => src.Department.OrganizationId));
            CreateMap<DomainUserDto, DomainUserResponseDto>()
                .ForMember(dest => dest.DomainUserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.TenantId, opt => opt.MapFrom(src => src.Tenant.TenantId))
                .ForMember(dest => dest.TenantName, opt => opt.MapFrom(src => src.Tenant.Name));
            CreateMap<ManagerDto, ManagerInfoResponseDto>()
                .ForMember(dest => dest.Department, opt => opt.MapFrom(src => src.Department.Name))
                .ForMember(dest => dest.OrganizationId, opt => opt.MapFrom(src => src.Department.OrganizationId));
            CreateMap<ReportingLineDto, ReportLineResponseDto>()
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.Report.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.Report.LastName));
            #endregion

        }
    }
}
