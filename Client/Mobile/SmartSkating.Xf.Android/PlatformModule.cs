using Android.App;
using Sanet.SmartSkating.Droid.Services.Location;
using Sanet.SmartSkating.Services.Hardware;
using Sanet.SmartSkating.Services.Location;
using Sanet.SmartSkating.Xf.Droid.AndroidShared.Services.Hardware;
#if DEBUG
using Sanet.SmartSkating.Tizen.Services.Location;
#endif

using SimpleInjector;

namespace Sanet.SmartSkating.Xf.Droid
{
    public static class ContainerExtensions
    {
        public static void RegisterModules(this Container container, Activity activity)
        {
            container.RegisterAndroidModule(activity);
            container.RegisterMainModule();
        }

        private static void RegisterAndroidModule(this Container container, Activity activity)
        {
#if DEBUG
            container.RegisterInstance<ILocationService>(new DummyLocationService("Schaatsnaacht", 1000));
#else
            container.RegisterInstance<ILocationService>(new LocationManagerService(activity));
#endif
            container.RegisterSingleton<IBleLocationService,AndroidBleService>();
            container.RegisterInstance<IBluetoothService>(new AndroidBluetoothService(activity));
        }
    }
}