using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OutOut.Infrastructure.Services
{
    public class NotificationSenderService
    {
        private readonly ILogger<NotificationSenderService> _logger;
        public NotificationSenderService(ILogger<NotificationSenderService> logger)
        {
            _logger = logger;
        }

        public async Task<BatchResponse> Send(List<string> fcmTokens, string title, string body, Dictionary<string, string> data = null, string imageUrl = null, string notificationId = null)
        {
            data ??= new Dictionary<string, string>();
            data.Add("click_action", "FLUTTER_NOTIFICATION_CLICK");
            var tag = $"{title}, {body}, {DateTime.Now.Ticks}";
            var messages = fcmTokens.Select(fcmToken =>
            {
                return new Message()
                {
                    Token = fcmToken,
                    Notification = new Notification
                    {
                        Title = title,
                        Body = body,
                        ImageUrl = imageUrl,
                    },
                    Android = new AndroidConfig
                    {
                        Notification = new AndroidNotification
                        {
                            Title = title,
                            Body = body,
                            ImageUrl = imageUrl,
                            Tag = tag,
                        }
                    },
                    Apns = new ApnsConfig
                    {
                        FcmOptions = new ApnsFcmOptions
                        {
                            ImageUrl = imageUrl
                        },
                        Aps = new Aps
                        {
                            MutableContent = true,
                            ContentAvailable = true,
                            //Badge = 0,
                            Sound = "default"
                        },
                    },
                    Data = data,
                };
            });

            var firebaseApp = FirebaseApp.DefaultInstance;
            var messagesList = messages.ToList();
            var chunks = ChunkBy(messagesList, 99);

            BatchResponse batchResponse = null;

            foreach (var messagesChunk in chunks)
            {
                try
                {
                    batchResponse = await FirebaseMessaging.GetMessaging(firebaseApp).SendAllAsync(messagesChunk);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Firebase Exception : {ex}");
                }
                var failedResponses = batchResponse?.Responses?.Where(r => !r.IsSuccess);
                if (failedResponses != null && failedResponses.Any())
                {
                    foreach (var failedResponse in failedResponses)
                    {
                        _logger.LogWarning("Firebase Messaging failed to send , exception: {exception}, message: {message}",
                                         failedResponse.Exception,
                                         failedResponse.Exception.Message);
                    }
                }
            }
            return batchResponse;
        }

        public async Task<List<string>> SendSilent(List<string> fcmTokens, Dictionary<string, string> data = null)
        {
            data ??= new Dictionary<string, string>();
            //data.Add("click_action", "FLUTTER_NOTIFICATION_CLICK");
            var messages = fcmTokens.Select(fcmToken =>
            {
                return new Message()
                {
                    Token = fcmToken,
                    Data = data,
                };
            });
            var messagesList = messages.ToList();
            var chunks = SplitList(messagesList, 99);

            var firebaseApp = FirebaseApp.DefaultInstance;
            foreach (var messagesChunk in chunks)
            {
                var batchResponse = await FirebaseMessaging.GetMessaging(firebaseApp).SendAllAsync(messagesChunk);
                var failedResponses = batchResponse.Responses.Where(r => !r.IsSuccess);
                foreach (var failedResponse in failedResponses)
                {
                    _logger.LogWarning("Firebase Messaging failed to send , exception: {exception}, message: {message}",
                                     failedResponse.Exception,
                                     failedResponse.Exception.Message);
                }

            }
            return new List<string>();
        }

        public static List<List<Message>> SplitList(List<Message> messages, int nSize)
        {
            var list = new List<List<Message>>();

            for (int i = 0; i < messages.Count; i += nSize)
            {
                list.Add(messages.GetRange(i, Math.Min(nSize, messages.Count - i)));
            }
            return list;
        }

        public static List<List<Message>> ChunkBy<Message>(List<Message> source, int chunkSize)
        {
            return source
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / chunkSize)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
        }
    }
}
