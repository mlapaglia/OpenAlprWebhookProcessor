using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OpenAlprWebhookProcessor.WebhookProcessor.OpenAlprWebsocket
{
    public static class RequestType
    {
        public static string GetRequestType(OpenAlprRequestType type)
        {
            switch (type)
            {
                case OpenAlprRequestType.agent_status:
                    return "agent_status";
                case OpenAlprRequestType.config_info:
                    return "config_info";
                case OpenAlprRequestType.config_save_mask:
                    return "config_save_mask";
                case OpenAlprRequestType.image_download:
                    return "image_download";
                case OpenAlprRequestType.register:
                    return "register";
                case OpenAlprRequestType.site_id:
                    return "site_id";

                default:
                    throw new KeyNotFoundException("Unable to find string value for enum: " + type.ToString());
            }
        }
    }

    public enum OpenAlprRequestType
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        agent_status,

        [JsonConverter(typeof(JsonStringEnumConverter))]
        config_info,

        [JsonConverter(typeof(JsonStringEnumConverter))]
        config_save_mask,

        [JsonConverter(typeof(JsonStringEnumConverter))]
        image_download,

        [JsonConverter(typeof(JsonStringEnumConverter))]
        register,

        [JsonConverter(typeof(JsonStringEnumConverter))]
        site_id,
    }
}
