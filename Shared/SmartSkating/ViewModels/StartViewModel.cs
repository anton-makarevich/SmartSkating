using System.Threading.Tasks;
using System.Windows.Input;
using Sanet.SmartSkating.Models;
using Sanet.SmartSkating.Models.EventArgs;
using Sanet.SmartSkating.Models.Location;
using Sanet.SmartSkating.Services;
using Sanet.SmartSkating.Services.Api;
using Sanet.SmartSkating.Services.Hardware;
using Sanet.SmartSkating.Services.Location;
using Sanet.SmartSkating.Services.Tracking;
using Sanet.SmartSkating.ViewModels.Base;

namespace Sanet.SmartSkating.ViewModels
{
    public class StartViewModel:BaseViewModel
    {
        private readonly ILocationService _locationService;
        private readonly ITrackService _tracksService;
        private readonly IDataSyncService _dataSyncService;
        private readonly IBluetoothService _bluetoothService;
        private readonly ISettingsService _settingsService;
        private string _infoLabel = string.Empty;
        private bool _areGeoServicesInitialized;
        private bool _isInitializingGeoServices;

        private const int GeoServicesInitTimeoutInSeconds = 30;

        public StartViewModel(ILocationService locationService, ITrackService tracksService,
            IDataSyncService dataSyncService, IBluetoothService bluetoothService, ISettingsService settingsService)
        {
            _locationService = locationService;
            _tracksService = tracksService;
            _dataSyncService = dataSyncService;
            _bluetoothService = bluetoothService;
            _settingsService = settingsService;
        }

        public string InfoLabel
        {
            get => _infoLabel;
            private set => SetProperty(ref _infoLabel, value);
        }

        public bool AreGeoServicesInitialized
        {
            get => _areGeoServicesInitialized;
            private set => SetProperty(ref _areGeoServicesInitialized, value);
        }

        public bool IsRinkSelected => _tracksService.SelectedRink!=null;
        public string RinkName => _tracksService.SelectedRink!=null
            ?_tracksService.SelectedRink.Name
            :string.Empty;

        public bool CanStart => !IsInitializingGeoServices && IsRinkSelected;

        public ICommand StartCommand => new SimpleCommand(async () =>
        {
            if (!await EnableBluetooth())
                return;
            if (!CanStart) 
                return;
            await NavigationService.NavigateToViewModelAsync<SessionsViewModel>();
        });

        public bool IsInitializingGeoServices
        {
            get => _isInitializingGeoServices;
            private set => SetProperty(ref _isInitializingGeoServices, value);
        }

        public ICommand SelectRinkCommand => new SimpleCommand(async () =>
        {
            if (await EnableBluetooth())
                await NavigationService.NavigateToViewModelAsync<TracksViewModel>();
        });

        public override void AttachHandlers()
        {
            base.AttachHandlers();
#pragma warning disable 4014
            LoadTracksAndInitializeGeoServices();
#pragma warning restore 4014
            _locationService.LocationReceived+= LocationServiceOnLocationReceived;
            _dataSyncService.StartSyncing();
        }

        private async Task LoadTracksAndInitializeGeoServices()
        {
            InfoLabel = "Initializing GeoServices. Be sure you're in open air";
            await _tracksService.LoadTracksAsync();
            IsInitializingGeoServices = true;
            _locationService.StartFetchLocation();
            await Task.Delay(GeoServicesInitTimeoutInSeconds*30);
            if (IsInitializingGeoServices)
            {
                LocationServiceOnLocationReceived(this, new CoordinateEventArgs(new Coordinate()));
            }
        }

        private void LocationServiceOnLocationReceived(object sender, CoordinateEventArgs e)
        {
            IsInitializingGeoServices = false;
            AreGeoServicesInitialized = !e.Coordinate.Equals(default(Coordinate));
            InfoLabel = AreGeoServicesInitialized? string.Empty: "Geo services are not available. Please select rink manually";
            if (AreGeoServicesInitialized)
            {
                _tracksService.SelectRinkCloseTo(e.Coordinate);
                if (_tracksService.SelectedRink == null)
                    InfoLabel = "No known Rinks nearby, please select manually";
                NotifyPropertyChanged(nameof(IsRinkSelected));
                NotifyPropertyChanged(nameof(RinkName));
            }
            NotifyPropertyChanged(nameof(CanStart));
            _locationService.StopFetchLocation();
        }

        public override void DetachHandlers()
        {
            base.DetachHandlers();
            _locationService.LocationReceived-= LocationServiceOnLocationReceived;
        }

        private async ValueTask<bool> EnableBluetooth()
        {
            if (!_settingsService.UseBle)
                return true;
            
            if (!_bluetoothService.IsBluetoothAvailable())
                await _bluetoothService.EnableBluetoothAsync();
            return _bluetoothService.IsBluetoothAvailable();
        }
    }
}
