using System;
using System.Threading.Tasks;
using Sanet.SmartSkating.Models.Training;

namespace Sanet.SmartSkating.Services.Tracking
{
    public interface ISessionManager
    {
        ISession? CurrentSession { get; }

        ValueTask StartSession();
        void StopSession();

        void CheckSession();

        bool IsRunning { get; }
        
        bool CanStart { get; }

        bool IsRemote { get; }
        
        event EventHandler? SessionUpdated;
    }
}
