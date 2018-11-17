using SmartStore.Core.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Services.Agent
{
    public partial interface IBankUpdateRequestService
    {
        /// <summary>
        /// Insert a bank update request
        /// </summary>
        /// <param name="address">Address</param>
        void InsertBankUpdateRequest(BankUpdateRequest request);

        void DeleteBankUpdateRequest(BankUpdateRequest request);

        IEnumerable<BankUpdateRequest> GetAllBankUpdateRequestsByCustomerId(int customerId);

        IEnumerable<BankUpdateRequest> GetAllBankUpdateRequests();

        BankUpdateRequest GetBankUpdateRequestById(int id);

        void UpdateBankUpdateRequestStatus(int id, int statusId);
    }
}
