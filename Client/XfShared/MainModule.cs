using Sanet.SmartSkating.Dto.Services;
using Sanet.SmartSkating.Services.Account;
using Sanet.SmartSkating.Services.Api;
using Sanet.SmartSkating.Services.Storage;
using Sanet.SmartSkating.Services.Tracking;
using Sanet.SmartSkating.ViewModels;
using SimpleInjector;

namespace Sanet.SmartSkating.Xf
{
    public static class ContainerExtensions
    {
        public static void RegisterMainModule(this Container container)
        {
            // Register app start viewmodel
            container.Register<StartViewModel>();
            
            // Register services
            container.RegisterSingleton<IAccountService,EssentialsAccountService>();
            container.RegisterSingleton<IDataSyncService,DataSyncService>();
            container.RegisterSingleton<IDataService, JsonStorageService>();
            container.RegisterSingleton<ITrackProvider,LocalTrackProvider>();
            container.RegisterSingleton<ITrackService, TrackService>();
            container.RegisterSingleton<ISessionService,SessionService>();
        }
    }
}