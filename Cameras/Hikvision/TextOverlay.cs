using System.Xml.Serialization;

namespace OpenAlprWebhookProcessor.Cameras.Hikvision
{
	[XmlRoot(ElementName = "TextOverlay")]
	public class TextOverlay
	{
		public TextOverlay()
        {
        }

		[XmlElement(ElementName = "id")]
		public string Id { get; set; }

		[XmlElement(ElementName = "enabled")]
		public string Enabled { get; set; }

		[XmlElement(ElementName = "alignment")]
		public string Alignment { get; set; }

		[XmlElement(ElementName = "positionX")]
		public string PositionX { get; set; }

		[XmlElement(ElementName = "positionY")]
		public string PositionY { get; set; }

		[XmlElement(ElementName = "displayText")]
		public string DisplayText { get; set; }
	}
}
