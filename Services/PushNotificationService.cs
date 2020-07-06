using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Domain;

namespace Services
{
    public class PushNotificationService : IPushNotificationService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<PushNotificationService> _logger;
        private readonly IPushLogService pushLogService;

        public PushNotificationService(IConfiguration configuration,
            ILogger<PushNotificationService> logger, IPushLogService pushLogService)
        {
            _config = configuration;
            _logger = logger;
            this.pushLogService = pushLogService;
        }

        /// <summary>
        /// Method to send notification to parent
        /// </summary>
        /// <returns>The boolean if the notification was sent</returns>
        /// <param name="parentCode">The id of a parent to send notification to</param>
        /// <param name="message">The message text that will be shown on the notification bar</param>
        /// <param name="studentCode">Id of a child, the notification is about, it can be NULL if the notification is not about Child</param>
        public async Task<bool> SendPush(Guid parentCode, string message, Guid studentCode)
        {
            return await Send(parentCode, message, studentCode);
        }

        public async Task<bool> SendPush(PushNotification push)
        {
            if (!await this.pushLogService.ShouldSendPushNotification(push))
                return false;

            var sent = await Send(push.ReceiverCode, push.Message, push.Student.Code);

            if (sent)
            {
                _logger.LogInformation("notification sender successfully sent the push the notification to user {0}", push.ReceiverCode);

                var rowsAffected = await this.pushLogService.UpsertPushLog(push);

                if (rowsAffected != 1)
                {
                    _logger.LogError("UpsertPushLog operation returned {0} rows affected", rowsAffected);
                }
            }

            return sent;
        }

        private async Task<bool> Send(Guid parentCode, string message, Guid studentCode)
        {
            var httpClient = new HttpClient();
            var key = _config["Firebase:ServerKey"];
            var url = _config["Firebase:PushNotificationUrl"];

            if (string.IsNullOrWhiteSpace(key))
            {
                throw new Exception("firebase server key was not found");
            }

            if (string.IsNullOrWhiteSpace(url))
            {
                throw new Exception("firebase push notification url was not found");
            }

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("key", key);

            var postObject = new NotificationRoot
            {
                To = $"/topics/{parentCode}",
                Body = new NotificationBody { Body = message },
                Data = new NotificationData { StudentCode = studentCode }
            };

            var json = JsonConvert.SerializeObject(postObject);

            var response = await httpClient.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("could not send push notification to user {0}", parentCode);
            }

            return response.IsSuccessStatusCode;
        }


        class NotificationRoot
        {
            [JsonProperty("to")]
            public string To { get; set; }

            [JsonProperty("notification")]
            public NotificationBody Body { get; set; }

            [JsonProperty("data")]
            public NotificationData Data { get; set; }
        }

        class NotificationBody
        {
            [JsonProperty("title")]
            public string Title { get; set; } = "School Time Tracker";

            [JsonProperty("body")]
            public string Body { get; set; }

            [JsonProperty("sound")]
            public string Sound { get; set; } = "default";
        }

        class NotificationData
        {
            public Guid StudentCode { get; set; }
        }
    }

    public interface IPushNotificationService
    {
        [Obsolete("Use SendPush(PushNotification push)")]
        Task<bool> SendPush(Guid parentId, string message, Guid studentCode);

        Task<bool> SendPush(PushNotification push);
    }
}
