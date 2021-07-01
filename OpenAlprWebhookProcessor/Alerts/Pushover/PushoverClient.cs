using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpenAlprWebhookProcessor.Data;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Alerts.Pushover
{
    public class PushoverClient : IAlertClient
    {
        private const string PushOverApiUrl = "https://api.pushover.net/1/messages.json";

        private readonly HttpClient _httpClient;

        private readonly IServiceProvider _serviceProvider;

        public PushoverClient(IServiceProvider serviceProvider)
        {
            _httpClient = new HttpClient();
            _serviceProvider = serviceProvider;
        }

        public async Task SendAlertAsync(
            Alert alert,
            string base64PreviewJpeg,
            CancellationToken cancellationToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var processorContext = scope.ServiceProvider.GetRequiredService<ProcessorContext>();

                var clientSettings = await processorContext.PushoverAlertClients.FirstOrDefaultAsync(cancellationToken);

                var boundary = Guid.NewGuid().ToString();
                using (var content = new MultipartFormDataContent(boundary))
                {
                    content.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data; boundary=" + boundary);

                    content.Add(new StringContent(clientSettings.UserKey), "user");
                    content.Add(new StringContent(clientSettings.ApiToken), "token");
                    content.Add(new StringContent(alert.PlateNumber + " " + alert.Description), "message");
                    content.Add(new StringContent("openalpr alert"), "title");

                    if(clientSettings.SendPlatePreview)
                    {
                        content.Add(new ByteArrayContent(Convert.FromBase64String(base64PreviewJpeg)), "attachment", "attachment.jpg");
                    }

                    try
                    {
                        var response = await _httpClient.PostAsync(
                            PushOverApiUrl,
                            content,
                            cancellationToken);

                        if (!response.IsSuccessStatusCode)
                        {
                            throw new InvalidOperationException("failed");
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException("failed");
                    }
                }
            }
        }
    }
}
