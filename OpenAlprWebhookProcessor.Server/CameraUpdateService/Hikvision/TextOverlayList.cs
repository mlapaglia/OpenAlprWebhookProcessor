using System.Collections.Generic;
using System.Xml.Serialization;

namespace OpenAlprWebhookProcessor.CameraUpdateService.Hikvision
{
	[XmlRoot(ElementName = "TextOverlayList")]
	public class TextOverlayList
	{
		public TextOverlayList()
        {
        }

		[XmlElement(ElementName = "TextOverlay")]
		public List<TextOverlay> TextOverlay { get; set; }
	}
}
