using System;
using System.Linq;
using System.Windows.Input;
using Sanet.SmartSkating.Models;
using Sanet.SmartSkating.Models.EventArgs;
using Sanet.SmartSkating.Models.Training;
using Sanet.SmartSkating.Services.Location;
using Sanet.SmartSkating.Services.Storage;
using Sanet.SmartSkating.Services.Tracking;
using Sanet.SmartSkating.ViewModels.Base;

namespace Sanet.SmartSkating.ViewModels
{
    public class LiveSessionViewModel:BaseViewModel
    {
        private readonly ILocationService _locationService;
        private readonly IStorageService _storageService;
        private readonly ITrackService _trackService;
        private readonly ISessionService _sessionService;
        private ISession? _currentSession;
        private bool _isRunning;
        private string _infoLabel = string.Empty;
        private string _currentSector = string.Empty;
        private string _lastLapTime = string.Empty;
        private string _laps = string.Empty;
        private string _lastSector = string.Empty;

        public LiveSessionViewModel(
            ILocationService locationService, 
            IStorageService storageService,
            ITrackService trackService, ISessionService sessionService)
        {
            _locationService = locationService;
            _storageService = storageService;
            _trackService = trackService;
            _sessionService = sessionService;
        }
        
        public ICommand StartCommand => new SimpleCommand( () =>
        {
            _locationService.LocationReceived+= LocationServiceOnLocationReceived;
            _locationService.StartFetchLocation();
            IsRunning = true;
        });

        private void LocationServiceOnLocationReceived(object sender, CoordinateEventArgs e)
        {
            LastCoordinate = e.Coordinate;
            _storageService.SaveCoordinateAsync(LastCoordinate);
            Session?.AddPoint(LastCoordinate,DateTime.Now);
            UpdateMetaData();
        }

        private void UpdateMetaData()
        {
            InfoLabel = LastCoordinate.ToString();
            if (Session != null)
            {
                LastLapTime = Session.LapsCount > 0 ? $"Last Lap: {Session.LastLapTime:h\\:m\\:s}":string.Empty;
                Laps = $"Laps: {Session.LapsCount}";
                if (Session.Sectors.Any())
                {
                    var lastSector = Session.Sectors.Last();
                    LastSector = $"Last Sector: {lastSector.Type.GetSectorName()}, {lastSector.Time:m\\:s}";
                }
                if (Session.WayPoints.Any())
                    CurrentSector = $"Currently in {Session.WayPoints.Last().Type.GetSectorName()} sector";
            }
        }

        public ICommand StopCommand => new SimpleCommand(StopLocationService);

        private void StopLocationService()
        {
            _locationService.LocationReceived -= LocationServiceOnLocationReceived;
            _locationService.StopFetchLocation();
            IsRunning = false;
            InfoLabel = string.Empty;
        }

        public bool IsRunning
        {
            get => _isRunning;
            private set => SetProperty(ref _isRunning, value);
        }

        public Coordinate LastCoordinate { get; private set; }

        public string InfoLabel
        {
            get => _infoLabel;
            private set => SetProperty(ref _infoLabel, value);
        }

        public string CurrentSector
        {
            get => _currentSector;
            set => SetProperty(ref _currentSector, value);
        }

        public ISession? Session
        {
            get => _currentSession;
            private set
            {
                SetProperty(ref _currentSession, value);
                NotifyPropertyChanged(nameof(CanStart));
            }
        }

        public bool CanStart => Session != null;

        public string LastLapTime
        {
            get => _lastLapTime;
            private set => SetProperty(ref _lastLapTime, value);
        }

        public string Laps    
        {
            get => _laps;
            private set => SetProperty(ref _laps, value);
        }

        public string LastSector
        {
            get => _lastSector;
            private set => SetProperty(ref _lastSector, value);
        }

        public override void DetachHandlers()
        {
            base.DetachHandlers();
            StopLocationService();
        }

        public override void AttachHandlers()
        {
            base.AttachHandlers();
            CreateSession();
        }

        private void CreateSession()
        {
            Session = _trackService.SelectedRink != null
                ? _sessionService.CreateSessionForRink(_trackService.SelectedRink)
                : null;
        }
    }
}