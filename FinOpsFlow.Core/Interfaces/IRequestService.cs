using FinOpsFlow.Core.DTOs;
using FinOpsFlow.Core.Entities;
using FinOpsFlow.Core.Enums;

namespace FinOpsFlow.Core.Interfaces;

public interface IRequestService
{
    Task<List<Request>> GetAllAsync(RequestFilterDto? filter = null);
    Task<Request?> GetByIdAsync(int id);
    Task<Request> CreateAsync(CreateRequestDto dto, string userId);
    Task UpdateAsync(int id, UpdateRequestDto dto, string userId);
    Task ChangeStatusAsync(int id, RequestStatus newStatus, string userId);
}