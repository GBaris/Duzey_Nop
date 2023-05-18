using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Stores;
using Nop.Plugin.Payments.BerkutPay.Models.YKBModels;
using Nop.Services.Payments;

namespace Nop.Plugin.Payments.BerkutPay.Services.IServices
{
    public interface IYKB_Service
    {
        Task<ProcessPaymentResult> ProcessPayment3DAuthAsync(ProcessPaymentRequest processPaymentRequest); // 3D ile Provizyon işlemi başlat
        Task<ProcessPaymentRequest> PrepareProcessPaymentRequest(FormResponseModel model, Customer customer, Store store);
        bool IsValidForm(IFormCollection form);
        string GetMacData(string xid, string amount);
        Task<bool> IsCheckoutDisabledOrCartEmpty(Customer customer, Store store);
        Task<bool> IsOnePageCheckoutEnabledOrGuestNotAllowedAsync(Customer customer);
        Task<bool> IsMinimumOrderPlacementIntervalValidAsync(Customer customer);
        Task HandleSuccessfulOrderAsync(Order placedOrder);
        Task<string> HandlePaymentErrorAsync();


        Task<ProcessPaymentResult> SendStandartSaleRequestAsync(ProcessPaymentRequest processPaymentRequest);
        Task<ProcessPaymentResult> SendStandarAuthRequestAsync(ProcessPaymentRequest processPaymentRequest);

        Task<RefundPaymentResult> RefundYKBAsync(RefundPaymentRequest refundPaymentRequest)
    }
}
