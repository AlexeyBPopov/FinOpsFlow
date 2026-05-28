using FinOpsFlow.Core.DTOs;

namespace FinOpsFlow.Core.Interfaces;

public interface IDashboardService
{
    Task<DashboardDto> GetAsync(string userId, bool isManager);
}