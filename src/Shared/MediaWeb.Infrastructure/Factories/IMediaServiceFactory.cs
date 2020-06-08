using MediaWeb.Infrastructure.Model;
using Microsoft.Azure.Management.Media;
using System.Threading.Tasks;

namespace MediaWeb.Infrastructure.Factories
{
    public interface IMediaServiceFactory
    {
        Task<IAzureMediaServicesClient> CreateMediaServicesClientAsync(ConfigWrapper config);
    }
}