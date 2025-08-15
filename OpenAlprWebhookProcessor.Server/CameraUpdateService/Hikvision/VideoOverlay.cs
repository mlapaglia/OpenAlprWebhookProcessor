using System.Xml.Serialization;

namespace OpenAlprWebhookProcessor.CameraUpdateService.Hikvision
{
	[XmlRoot(ElementName = "VideoOverlay")]
	public class VideoOverlay
	{
        public VideoOverlay()
        {
        }

		[XmlElement(ElementName = "alignment")]
		public string Alignment { get; set; }

		[XmlElement(ElementName = "TextOverlayList")]
		public TextOverlayList TextOverlayList { get; set; }
	}
}
