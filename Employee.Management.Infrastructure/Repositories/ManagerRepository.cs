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
            return _context.Managers
                .Select(m => _mapper.Map<ManagerDto>(m))
                .ToListAsync();
        }

        public async Task<Manager?> GetManagerInfoAsync(Guid managerId)
        {
            return await _context.Managers.FindAsync(managerId);
        }

        public Task<bool> HasReportsAsync(Guid managerId)
        {
            return _context.ReportingLines.AnyAsync(r => r.ManagerId == managerId);
        }
    }
}
