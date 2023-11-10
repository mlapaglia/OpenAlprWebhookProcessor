using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OpenAlprWebhookProcessor.WebhookProcessor.OpenAlprWebsocket
{
    public static class RequestType
    {
        public static string GetRequestType(OpenAlprRequestType type)
        {
            return type switch
            {
                OpenAlprRequestType.AgentStatus => "agent_status",
                OpenAlprRequestType.ConfigAgentOperation => "config_agent_operation",
                OpenAlprRequestType.ConfigInfo => "config_info",
                OpenAlprRequestType.ConfigSaveMask => "config_save_mask",
                OpenAlprRequestType.ImageDownload => "image_download",
                OpenAlprRequestType.Register => "register",
                OpenAlprRequestType.SiteId => "site_id",
                _ => throw new KeyNotFoundException("Unable to find string value for enum: " + type.ToString()),
            };
        }
    }

    public enum OpenAlprRequestType
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        AgentStatus,

        [JsonConverter(typeof(JsonStringEnumConverter))]
        ConfigAgentOperation,

        [JsonConverter(typeof(JsonStringEnumConverter))]
        ConfigInfo,

        [JsonConverter(typeof(JsonStringEnumConverter))]
        ConfigSaveMask,

        [JsonConverter(typeof(JsonStringEnumConverter))]
        ImageDownload,

        [JsonConverter(typeof(JsonStringEnumConverter))]
        Register,

        [JsonConverter(typeof(JsonStringEnumConverter))]
        SiteId,
    }
}
