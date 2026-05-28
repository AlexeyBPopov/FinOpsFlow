using FinOpsFlow.Core.Enums;

namespace FinOpsFlow.Core.DTOs;

public record CreateRequestDto(
    string Title,
    string Description,
    RequestPriority Priority,
    int CategoryId,
    DateTime? DueDate
);

public record UpdateRequestDto(
    string Title,
    string Description,
    RequestPriority Priority,
    int CategoryId,
    DateTime? DueDate,
    string? AssignedToId
);

public record RequestFilterDto(
    RequestStatus? Status = null,
    RequestPriority? Priority = null,
    string? Keyword = null,
    string? AssignedToId = null,
    int? CategoryId = null,
    DateTime? CreatedFrom = null,
    DateTime? CreatedTo = null,
    bool OverdueOnly = false
);