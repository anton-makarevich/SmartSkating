using System.Threading.Tasks;
using Sanet.SmartSkating.Models.Geometry;
using Sanet.SmartSkating.Models.Training;

namespace Sanet.SmartSkating.Services.Tracking
{
    public interface ISessionService
    {
        ISession? CurrentSession { get; }

        Task StartSession();
        void StopSession();
    }
}
