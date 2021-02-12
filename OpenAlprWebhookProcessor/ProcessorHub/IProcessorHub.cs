using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.ProcessorHub
{
    public interface IProcessorHub
    {
        Task LicensePlateRecorded(string plateNumber);

        Task LicensePlateAlerted(string plateNumber);
    }
}
