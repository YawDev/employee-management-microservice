using AutoMapper;
using Employee.Management.Core.Interfaces.Repositories;
using Employee.Management.Models.DatabaseModels;
using Employee.Management.Models.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Employee.Management.Infrastructure.Repositories
{
    public class ManagerRepository(EmployeeManagementDbContext context, IMapper mapper) : IManagerRepository
    {  
        private readonly EmployeeManagementDbContext _context = context;
        private readonly IMapper _mapper = mapper;

        public Task<bool> CheckForExistingDomainUser(Guid domainUserId, Guid? managerId = null)
        {
            return _context.Managers.AnyAsync(m =>
                m.DomainUserId == domainUserId
                && (!managerId.HasValue || m.ManagerId != managerId.Value));
        }

        public async Task<int> CreateManagerAsync(Manager manager)
        {
            await _context.AddAsync(manager);
            return await _context.SaveChangesAsync();
        }

        // Deleting a Manager removes the *designation* row only — the person
        // (DomainUser) is never hard-deleted.
        public async Task<int> DeleteManagerAsync(Guid managerId)
        {
            var manager = await _context.Managers.FindAsync(managerId);
            if (manager == null) return 0;
            _context.Managers.Remove(manager);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> EditManagerAsync(Manager manager)
        {
            _context.Managers.Update(manager);
            return await _context.SaveChangesAsync();
        }

        public Task<List<ManagerDto>> GetAllManagersAsync()
        {
            return _context.Managers.AsTracking()
                .Include(m => m.DomainUser).ThenInclude(d => d.Tenant)
                .Include(m => m.Department)
                .Include(m => m.ReportingLines)
                .ThenInclude(r => r.Report).ThenInclude(d => d.Tenant)
                .Select(m => _mapper.Map<ManagerDto>(m))
                .ToListAsync();
        }

        public async Task<Manager?> GetManagerInfoAsync(Guid managerId)
        {
            return await _context.Managers.AsNoTracking()
            .Include(m => m.DomainUser).ThenInclude(d => d.Tenant)
            .Include(m => m.Department)
            .Include(m => m.ReportingLines)
            .ThenInclude(r => r.Report).ThenInclude(d => d.Tenant)
            .FirstOrDefaultAsync(m => m.ManagerId == managerId);
        }

        public Task<bool> HasReportsAsync(Guid managerId)
        {
            return _context.ReportingLines.AnyAsync(r => r.ManagerId == managerId);
        }

        public Task<bool> AddReports(Guid managerId, List<Guid> reportIds)
        {
            var reportingLines = reportIds.Select(reportId => new ReportingLine
            {
                ManagerId = managerId,
                ReportId = reportId
            }).ToList();

            _context.ReportingLines.AddRange(reportingLines);
            return _context.SaveChangesAsync().ContinueWith(t => t.Result > 0);
        }

        public Task<bool> RemoveReports(Guid managerId, List<Guid> reportIds)
        {
            var reportingLines = _context.ReportingLines
                .Where(r => r.ManagerId == managerId && reportIds.Contains(r.ReportId))
                .ToList();

            _context.ReportingLines.RemoveRange(reportingLines);
            return _context.SaveChangesAsync().ContinueWith(t => t.Result > 0);
        }
    }
}
