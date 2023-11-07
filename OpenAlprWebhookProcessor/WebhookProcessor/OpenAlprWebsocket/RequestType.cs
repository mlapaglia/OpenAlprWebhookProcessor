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
                case OpenAlprRequestType.site_id:
                    return "site_id";
                case OpenAlprRequestType.config_info:
                    return "config_info";
                case OpenAlprRequestType.register:
                    return "register";
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
        site_id,

        [JsonConverter(typeof(JsonStringEnumConverter))]
        config_info,

        [JsonConverter(typeof(JsonStringEnumConverter))]
        register,
    }
}
