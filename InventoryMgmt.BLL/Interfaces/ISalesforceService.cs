using InventoryMgmt.BLL.DTOs;
using System;
using System.Threading.Tasks;

namespace InventoryMgmt.BLL.Interfaces
{
    public interface ISalesforceService
    {
        Task<(bool Success, string ErrorMessage)> CreateAccountAndContactAsync(SalesforceAccountDto accountDto);
        Task<(bool Success, string ErrorMessage)> TestAuthenticationAsync();
    }
}
