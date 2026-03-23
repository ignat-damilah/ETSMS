using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Foundation.Application.DTOs;

namespace Foundation.Application.Interfaces;

public interface IEmployeeService
{
    Task<IEnumerable<EmployeeDto>> ListAsync(CancellationToken cancellationToken = default);
    Task<EmployeeDto?> GetAsync(Guid id, CancellationToken cancellationToken = default);
}