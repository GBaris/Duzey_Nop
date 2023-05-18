using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Payments.BerkutPay.Models;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.ScheduleTasks;

namespace Nop.Plugin.Payments.BerkutPay.Services
{
    public class OrderStatusUpdate : IScheduleTask
    {
        #region Fields

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;

        #endregion

        #region Ctor

        public OrderStatusUpdate(IHttpClientFactory httpClientFactory,
            IPaymentService paymentService,
            IOrderService orderService)
        {
            _httpClientFactory = httpClientFactory;
            _paymentService = paymentService;
            _orderService = orderService;
        }

        #endregion

        public async Task ExecuteAsync()
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync("http://localhost:12345"); //değiştiricez bunu

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                var orderData = JsonConvert.DeserializeObject<SAPResponceModelcs>(data);

                var order = await _orderService.GetOrderByIdAsync(orderData.OrderId);

                if (order != null)
                {
                    var processPaymentRequest = new ProcessPaymentRequest() { OrderGuid = order.OrderGuid };
                    await _paymentService.ProcessPaymentAsync(processPaymentRequest);

                    order.OrderStatus = OrderStatus.Pending;
                    await _orderService.UpdateOrderAsync(order);
                }
            }
        }
    }
}
