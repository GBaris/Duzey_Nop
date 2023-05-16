﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Stores;
using Nop.Core.Http.Extensions;
using Nop.Data;
using Nop.Plugin.Payments.BerkutPay.Models.YKB_Models;
using Nop.Plugin.Payments.BerkutPay.Models.YKBModels;
using Nop.Plugin.Payments.BerkutPay.Services.IServices;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
//using Task = System.Threading.Tasks.Task;

namespace Nop.Plugin.Payments.BerkutPay.Services
{
    public class YKB_Service : IYKB_Service
    {

        #region Fields

        private readonly BerkutPaySettings _berkutPaySettings;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly INotificationService _notificationService;
        private readonly IPaymentService _paymentService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IRepository<Order> _orderRepository;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly ICustomerService _customerService;
        private readonly OrderSettings _orderSettings;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly ILocalizationService _localizationService;
        private readonly IOrderService _orderService;

        #endregion

        #region Ctor

        public YKB_Service(BerkutPaySettings berkutPaySettings, IHttpContextAccessor httpContextAccessor,
            INotificationService notificationService, IPaymentService paymentService, 
            IGenericAttributeService genericAttributeService, IRepository<Order> orderRepository,
            IOrderProcessingService orderProcessingService, IShoppingCartService shoppingCartService,
            ICustomerService customerService, OrderSettings orderSettings,
            IStoreContext storeContext, IWorkContext workContext,
            ILocalizationService localizationService, IOrderService orderService
            )
        {
            _berkutPaySettings = berkutPaySettings;
            _httpContextAccessor = httpContextAccessor;
            _notificationService = notificationService;
            _paymentService = paymentService;
            _genericAttributeService = genericAttributeService;
            _orderRepository = orderRepository;
            _orderProcessingService = orderProcessingService;
            _shoppingCartService = shoppingCartService;
            _customerService = customerService;
            _orderSettings = orderSettings;
            _localizationService = localizationService;
            _orderService = orderService;
        }

        #endregion

        public async Task<ProcessPaymentResult> ProcessPayment3DAuthAsync(ProcessPaymentRequest processPaymentRequest)
        {
            var bankResult = await SendOOSRequest3DAuthAsync(processPaymentRequest);

            if (bankResult.Errors.Count > 0)
            {
                return bankResult;
            }

            bankResult.AddError("Doğrulama için bankanıza yönlendiriliyorsunuz.");



            await RedirectBankUrl(bankResult);

            processPaymentRequest.CustomValues.TryGetValue("BankPacket", out var bankPacket);

            processPaymentRequest.CustomValues.TryGetValue("MerchantPacket", out var merchantPacket);
            processPaymentRequest.CustomValues.TryGetValue("Sign", out var sign);
            processPaymentRequest.CustomValues.TryGetValue("Mac", out var mac);
            processPaymentRequest.CustomValues.TryGetValue("Xid", out var xid);
            processPaymentRequest.CustomValues.TryGetValue("Amount", out var amount);

            var parameters = CreateOOSResolveMerchantRequest(bankPacket.ToString(), merchantPacket.ToString(), sign.ToString(), mac.ToString());
            var resolveMerchantResult = await SendOOSResolveMerchantRequestAsync(parameters);

            var tempMac = GetMacData(xid.ToString(), amount.ToString());

            if (resolveMerchantResult.MdStatus != "1")
            {
                var result = new ProcessPaymentResult();
                _notificationService.ErrorNotification(BerkutPaymentHelper.GetMDStatusErrorMessage(resolveMerchantResult.MdStatus), true);

                result.AddError(resolveMerchantResult.MdErrorMessage);
                return result;
            }
            else
            {
                var tranDataResult = await SendOOSTranDataRequestAsync(processPaymentRequest, bankPacket.ToString(), tempMac, xid.ToString());
                if (tranDataResult.Errors.Count > 0)
                {
                    _notificationService.ErrorNotification(tranDataResult.Errors.First().ToString(), true);
                }
                return tranDataResult;
            }
        }

        #region I. Aşama

        private Dictionary<string, string> CreateOOSRequestAuth(ProcessPaymentRequest processPaymentRequest)
        {
            string amount = Math.Round(processPaymentRequest.OrderTotal * 100m).ToString("0", new CultureInfo("en-US"));
            string merchantId = _berkutPaySettings.YKB_MERCHANT_ID;
            string terminalId = _berkutPaySettings.YKB_TERMINAL_ID;
            string posnetId = _berkutPaySettings.YKB_POSNET_ID;
            string orderId = processPaymentRequest.OrderGuid.ToString("N").Substring(0, 20);

            _httpContextAccessor.HttpContext.Session.SetString("BankOrderId", orderId);

            string cardHolderName = processPaymentRequest.CreditCardName;
            string expireDate = (processPaymentRequest.CreditCardExpireYear % 1000) + "" + (processPaymentRequest.CreditCardExpireMonth.ToString().Count() == 1 ? "0" + processPaymentRequest.CreditCardExpireMonth.ToString() : processPaymentRequest.CreditCardExpireMonth.ToString());
            string ccno = processPaymentRequest.CreditCardNumber;
            string cvc = processPaymentRequest.CreditCardCvv2;

            string requestXml = $@"<?xml version=""1.0"" encoding=""utf-8""?>
                                        <posnetRequest>
                                            <mid>{merchantId}</mid>
                                            <tid>{terminalId}</tid>
                                            <oosRequestData>
                                                <posnetid>{posnetId}</posnetid>
                                                <XID>{orderId}</XID>
                                                <amount>{amount}</amount>
                                                <currencyCode>TL</currencyCode>
                                                <installment>00</installment>
                                                <tranType>Auth</tranType>
                                                <cardHolderName>{cardHolderName}</cardHolderName>
                                                <ccno>{ccno}</ccno>
                                                <expDate>{expireDate}</expDate>
                                                <cvc>{cvc}</cvc>
                                            </oosRequestData>
                                        </posnetRequest>";

            var httpParameters = new Dictionary<string, string>
            {
                { "xmldata", requestXml }
            };

            return httpParameters;
        }

        private async Task<ProcessPaymentResult> SendOOSRequest3DAuthAsync(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult
            {
                AllowStoringCreditCardNumber = false
            };

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var oosRequest = CreateOOSRequestAuth(processPaymentRequest);

                    var response = await client.PostAsync(_berkutPaySettings.YKB_XML_SERVICE_URL, new FormUrlEncodedContent(oosRequest));
                    string responseContent = await response.Content.ReadAsStringAsync();

                    var xmlDocument = new XmlDocument();
                    xmlDocument.LoadXml(responseContent);

                    if (xmlDocument.SelectSingleNode("posnetResponse/approved") == null || xmlDocument.SelectSingleNode("posnetResponse/approved").InnerText != "1")
                    {
                        string errorMessage = xmlDocument.SelectSingleNode("posnetResponse/respText")?.InnerText ?? string.Empty;
                        if (string.IsNullOrEmpty(errorMessage))
                            errorMessage = "Ödeme sırasında bir hata oluştu. E01";

                        result.AddError(errorMessage);
                    }
                    else
                    {
                        result.AuthorizationTransactionResult = responseContent;
                        result.SubscriptionTransactionId = processPaymentRequest.OrderGuid.ToString("N").Substring(0, 20);

                        string sign = xmlDocument.SelectSingleNode("posnetResponse/oosRequestDataResponse/sign")?.InnerText ?? string.Empty;

                        if (sign == string.Empty)
                        {
                            result.AddError(responseContent);
                        }

                        return result;
                    }
                }
                catch (Exception ex)
                {
                    result.AddError(ex.Message);
                }
            }
            return result;
        }

        private async Task RedirectBankUrl(ProcessPaymentResult processPaymentResult)
        {
            string responseContent = processPaymentResult.AuthorizationTransactionResult;

            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(responseContent);

            string data1 = xmlDocument.SelectSingleNode("posnetResponse/oosRequestDataResponse/data1")?.InnerText ?? string.Empty;
            string data2 = xmlDocument.SelectSingleNode("posnetResponse/oosRequestDataResponse/data2")?.InnerText ?? string.Empty;
            string sign = xmlDocument.SelectSingleNode("posnetResponse/oosRequestDataResponse/sign")?.InnerText ?? string.Empty;

            var myForm = "<!DOCTYPE html><html><head><meta charset=\"utf-8\"/> </head><body>"
                  + "<form method=\"POST\" action=\"" + _berkutPaySettings.YKB_OOS_TDS_SERVICE_URL + "\"/>" +
                "<input type=\"hidden\" name=\"mid\" id=\"mid\" value=\"" + _berkutPaySettings.YKB_MERCHANT_ID + "\" />" +
                "<input type=\"hidden\" name=\"posnetID\" id=\"PosnetID\" value=\"" + _berkutPaySettings.YKB_POSNET_ID + "\" />" +
                "<input type=\"hidden\" name=\"posnetData\" id=\"posnetData\" value=\"" + data1 + "\" />" +
                "<input type=\"hidden\" name=\"posnetData2\" id=\"posnetData2\" value=\"" + data2 + "\" />" +
                "<input type=\"hidden\" name=\"digest\" id=\"sign\" value=\"" + sign + "\" />" +
                "<input type=\"hidden\" name=\"vftCode\" id=\"vftCode\" value=\"" + "" + "\" />" +
                "<input type=\"hidden\" name=\"useJokerVadaa\" id=\"useJokerVadaa\" value=\"" + "0" + "\" />" +
                "<input type=\"hidden\" name=\"merchantReturnURL\" id=\"merchantReturnURL\" value=\"" + _berkutPaySettings.YKB_MERCHANT_RETURN_URL + "\" />" +
                "<input type=\"hidden\" name=\"lang\" id=\"lang\" value=\"" + "tr" + "\" />" +
                "<input type=\"hidden\" name=\"openANewWindow\" id=\"openANewWindow\" value=\"" + "0" + "\" />" +
                "</form></body>" +
                "<script> document.forms[0].submit();</script></html>";

            byte[] data = System.Text.Encoding.UTF8.GetBytes(myForm);

            _httpContextAccessor.HttpContext.Response.Clear();
            await _httpContextAccessor.HttpContext.Response.WriteAsync(myForm);
            _httpContextAccessor.HttpContext.Response.Body.Close();

            return;
        }

        #endregion

        #region II. Aşama

        public ProcessPaymentRequest PrepareProcessPaymentRequest(FormResponseModel model, Customer customer, Store store)
        {
            var processPaymentRequest = _httpContextAccessor.HttpContext.Session.Get<ProcessPaymentRequest>("OrderPaymentInfo") ?? new ProcessPaymentRequest();
            processPaymentRequest.CustomValues.Add("BankPacket", model.BankPacket);
            processPaymentRequest.CustomValues.Add("MerchantPacket", model.MerchantPacket);
            processPaymentRequest.CustomValues.Add("Sign", model.Sign);
            processPaymentRequest.CustomValues.Add("Mac", model.Mac);
            processPaymentRequest.CustomValues.Add("Xid", model.Xid);
            processPaymentRequest.CustomValues.Add("Amount", model.Amount);
            _paymentService.GenerateOrderGuid(processPaymentRequest);
            processPaymentRequest.StoreId = store.Id;
            processPaymentRequest.CustomerId = customer.Id;
            processPaymentRequest.PaymentMethodSystemName = _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.SelectedPaymentMethodAttribute, store.Id).Result;
            _httpContextAccessor.HttpContext.Session.Set<ProcessPaymentRequest>("OrderPaymentInfo", processPaymentRequest);
            return processPaymentRequest;
        }

        public async Task<string> HandlePaymentErrorAsync()
        {
            await _notificationService.ErrorNotificationAsync(new Exception("Lütfen kart bilgilerinizi kontrol edip tekrar deneyiniz."), true);
            return "ShoppingCart";
        }

        public async Task HandleSuccessfulOrderAsync(Order placedOrder)
        {
            _httpContextAccessor.HttpContext.Session.Set<ProcessPaymentRequest>("OrderPaymentInfo", null);
            var postProcessPaymentRequest = new PostProcessPaymentRequest
            {
                Order = placedOrder
            };
            await _paymentService.PostProcessPaymentAsync(postProcessPaymentRequest);
            return;
        }

        public virtual async Task<bool> IsMinimumOrderPlacementIntervalValidAsync(Customer customer)
        {
            //prevent 2 orders being placed within an X seconds time frame
            if (_orderSettings.MinimumOrderPlacementInterval == 0)
                return true;

            var lastOrder = (await _orderService.SearchOrdersAsync(storeId: (await _storeContext.GetCurrentStoreAsync()).Id,
                customerId: (await _workContext.GetCurrentCustomerAsync()).Id, pageSize: 1))
                .FirstOrDefault();
            if (lastOrder == null)
                return true;

            var interval = DateTime.UtcNow - lastOrder.CreatedOnUtc;
            return interval.TotalSeconds > _orderSettings.MinimumOrderPlacementInterval;
        }

        public async Task<bool> IsCheckoutDisabledOrCartEmpty(Customer customer, Store store)
        {
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);
            return _orderSettings.CheckoutDisabled || !cart.Any();
        }

        public async Task<bool> IsOnePageCheckoutEnabledOrGuestNotAllowedAsync(Customer customer)
        {
            return _orderSettings.OnePageCheckoutEnabled || (await _customerService.IsGuestAsync(customer) && !_orderSettings.AnonymousCheckoutAllowed);
        }

        public string GetMacData(string xid, string amount)
        {
            string firstHash = HASH(_berkutPaySettings.YKB_ENCKEY + ';' + _berkutPaySettings.YKB_TERMINAL_ID);
            string mac = HASH(xid + ';' + amount + ';' + "TL" + ';' + _berkutPaySettings.YKB_MERCHANT_ID + ';' + firstHash);
            return mac;
        }

        private string HASH(string originalString)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(originalString));
                return Convert.ToBase64String(bytes);
            }
        }

        public bool IsValidForm(IFormCollection form)
        {
            return form != null && form.ContainsKey("BankPacket") && form.ContainsKey("MerchantPacket") && form.ContainsKey("Sign");
        }

        #endregion

        private Dictionary<string, string> CreateOOSResolveMerchantRequest(string bankData, string merchantData, string sign, string mac)
        {
            string merchantId = _berkutPaySettings.YKB_MERCHANT_ID;
            string terminalId = _berkutPaySettings.YKB_TERMINAL_ID;

            string requestXml = $@"<?xml version=""1.0"" encoding=""utf-8""?>
                                        <posnetRequest>
                                            <mid>{merchantId}</mid>
                                            <tid>{terminalId}</tid>
                                            <oosResolveMerchantData>
                                                <bankData>{bankData}</bankData>
                                                <merchantData>{merchantData}</merchantData>
                                                <sign>{sign}</sign>
                                                <mac>{mac}</mac>
                                            </oosResolveMerchantData>
                                        </posnetRequest>
            ";

            var httpParameters = new Dictionary<string, string>
            {
                { "xmldata", requestXml }
            };

            return httpParameters;
        }
        private async Task<ResolveMerchantModel> SendOOSResolveMerchantRequestAsync(Dictionary<string, string> parameters)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var response = await client.PostAsync(_berkutPaySettings.YKB_XML_SERVICE_URL, new FormUrlEncodedContent(parameters));
                    string responseContent = await response.Content.ReadAsStringAsync();

                    var xmlDocument = new XmlDocument();
                    xmlDocument.LoadXml(responseContent);

                    if (xmlDocument.SelectSingleNode("posnetResponse/approved") == null || xmlDocument.SelectSingleNode("posnetResponse/approved").InnerText != "1")
                    {
                        string errorMessage = xmlDocument.SelectSingleNode("posnetResponse/respText")?.InnerText ?? string.Empty;
                        if (string.IsNullOrEmpty(errorMessage))
                            errorMessage = "Ödeme sırasında bir hata oluştu. E01";
                        return null;
                    }
                    else
                    {
                        var result = new ResolveMerchantModel
                        {
                            Amount = xmlDocument.SelectSingleNode("posnetResponse/oosResolveMerchantDataResponse/amount")?.InnerText ?? string.Empty,
                            Currency = xmlDocument.SelectSingleNode("posnetResponse/oosResolveMerchantDataResponse/currency")?.InnerText ?? string.Empty,
                            Xid = xmlDocument.SelectSingleNode("posnetResponse/oosResolveMerchantDataResponse/xid")?.InnerText ?? string.Empty,
                            Installment = xmlDocument.SelectSingleNode("posnetResponse/oosResolveMerchantDataResponse/installment")?.InnerText ?? string.Empty,
                            Point = xmlDocument.SelectSingleNode("posnetResponse/oosResolveMerchantDataResponse/point")?.InnerText ?? string.Empty,
                            PointAmount = xmlDocument.SelectSingleNode("posnetResponse/oosResolveMerchantDataResponse/pointAmount")?.InnerText ?? string.Empty,
                            TxStatus = xmlDocument.SelectSingleNode("posnetResponse/oosResolveMerchantDataResponse/txStatus")?.InnerText ?? string.Empty,
                            MdStatus = xmlDocument.SelectSingleNode("posnetResponse/oosResolveMerchantDataResponse/mdStatus")?.InnerText ?? string.Empty,
                            MdErrorMessage = xmlDocument.SelectSingleNode("posnetResponse/oosResolveMerchantDataResponse/mdErrorMessage")?.InnerText ?? string.Empty,
                            Mac = xmlDocument.SelectSingleNode("posnetResponse/oosResolveMerchantDataResponse/mac")?.InnerText ?? string.Empty,
                        };

                        return result;
                    }

                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }
        private Dictionary<string, string> CreateOOSTranDataRequest(string bankData, string mac)
        {
            string merchantId = _berkutPaySettings.YKB_MERCHANT_ID;
            string terminalId = _berkutPaySettings.YKB_TERMINAL_ID;

            string requestXml = $@"<?xml version=""1.0"" encoding=""utf-8""?>
                                        <posnetRequest>
                                            <mid>{merchantId}</mid>
                                            <tid>{terminalId}</tid>
                                            <oosTranData>
                                                <bankData>{bankData}</bankData>
                                                <mac>{mac}</mac>
                                            </oosTranData>
                                        </posnetRequest>";

            var httpParameters = new Dictionary<string, string>
            {
                { "xmldata", requestXml }
            };

            return httpParameters;
        }
        private async Task<ProcessPaymentResult> SendOOSTranDataRequestAsync(ProcessPaymentRequest processPaymentRequest, string bankData, string mac, string bankOrderId)
        {
            var result = new ProcessPaymentResult
            {
                AllowStoringCreditCardNumber = false
            };

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var oosRequest = CreateOOSTranDataRequest(bankData, mac);

                    var response = await client.PostAsync(_berkutPaySettings.YKB_XML_SERVICE_URL, new FormUrlEncodedContent(oosRequest));
                    string responseContent = await response.Content.ReadAsStringAsync();

                    var xmlDocument = new XmlDocument();
                    xmlDocument.LoadXml(responseContent);

                    if (xmlDocument.SelectSingleNode("posnetResponse/approved") == null || xmlDocument.SelectSingleNode("posnetResponse/approved").InnerText != "1")
                    {
                        string errorMessage = xmlDocument.SelectSingleNode("posnetResponse/respText")?.InnerText ?? string.Empty;
                        if (string.IsNullOrEmpty(errorMessage))
                            errorMessage = "Ödeme sırasında bir hata oluştu. E01";

                        result.AddError(errorMessage);
                    }
                    else
                    {
                        string hostlogkey = xmlDocument.SelectSingleNode("posnetResponse/hostlogkey")?.InnerText ?? string.Empty;
                        string authCode = xmlDocument.SelectSingleNode("posnetResponse/authCode")?.InnerText ?? string.Empty;

                        result.AuthorizationTransactionCode = authCode;
                        result.AuthorizationTransactionId = hostlogkey;
                        result.SubscriptionTransactionId = bankOrderId;
                        result.AuthorizationTransactionResult = responseContent;

                        return result;
                    }
                }
                catch (Exception ex)
                {
                    result.AddError(ex.Message);
                }
            }

            return result;
        }


    }
}
