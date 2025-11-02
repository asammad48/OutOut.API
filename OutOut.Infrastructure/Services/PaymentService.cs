using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OutOut.Constants.Errors;
using OutOut.Models;
using OutOut.Models.Domains;
using OutOut.Models.Exceptions;
using OutOut.Models.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace OutOut.Infrastructure.Services
{
    public class PaymentService
    {
        private readonly AppSettings _appSettings;
        public PaymentService(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }
        public async Task<string> MakeTelrTransaction(EventBooking eventBooking = null, string method = "create", string orderRef = "")
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.ExpectContinue = false;
                HttpResponseMessage response = new HttpResponseMessage();
                var tokenSource = new CancellationTokenSource();
                using (var task = Task.Run(async () => response = await httpClient.PostAsync("https://secure.telr.com/gateway/order.json",
                new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("ivp_method", method),
                    new KeyValuePair<string, string>("ivp_store", _appSettings.TelrConfigurations.StoreId),
                    new KeyValuePair<string, string>("ivp_authkey", _appSettings.TelrConfigurations.AuthKey),
                    new KeyValuePair<string, string>("ivp_cart", DateTime.Now.Ticks.ToString() + new Random().Next(100000, 999999).ToString()),
                    new KeyValuePair<string, string>("ivp_desc", eventBooking?.Description),
                    new KeyValuePair<string, string>("ivp_test", "1"),
                    new KeyValuePair<string, string>("ivp_amount", eventBooking?.TotalAmount.ToString(CultureInfo.InvariantCulture)),
                    new KeyValuePair<string, string>("ivp_currency", eventBooking?.Currency),
                    new KeyValuePair<string, string>("ivp_framed", "0"),
                    new KeyValuePair<string, string>("return_auth", _appSettings.BackendOrigin + $"/api/Payment/Paid?id={eventBooking?.Id}"),
                    new KeyValuePair<string, string>("return_can", _appSettings.BackendOrigin + $"/api/Payment/Cancelled?id={eventBooking?.Id}"),
                    new KeyValuePair<string, string>("return_decl", _appSettings.BackendOrigin + $"/api/Payment/Declined?id={eventBooking?.Id}"),
                    new KeyValuePair<string, string>("bill_custref", eventBooking?.User?.Id),
                    new KeyValuePair<string, string>("order_ref", orderRef)
                })), tokenSource.Token))
                {
                    {
                        if (!task.Wait(TimeSpan.FromSeconds(10)))
                        {
                            await Task.Delay(8000);
                            if (!task.IsCompleted)
                            {
                                tokenSource.Cancel();
                                throw new OutOutException(ErrorCodes.Telr_RequestTimeOut);
                            }
                        }

                        return response.StatusCode != System.Net.HttpStatusCode.OK ?
                            throw new OutOutException(ErrorCodes.Telr_TransactionError) :
                            await response.Content.ReadAsStringAsync();
                    }
                }
            }
        }

        public async Task<TelrCreateResponse> CreateTelrTransaction(EventBooking eventBooking)
        {
            var apiResponse = await MakeTelrTransaction(eventBooking);
            return JsonConvert.DeserializeObject<TelrCreateResponse>(apiResponse);
        }

        public async Task<TelrCheckResponse> CheckTelrTransaction(string orderRef)
        {
            var apiResponse = await MakeTelrTransaction(null, "check", orderRef);
            return JsonConvert.DeserializeObject<TelrCheckResponse>(apiResponse);
        }
    }
}
