using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Sanet.SmartSkating.Models;
using Sanet.SmartSkating.Models.Training;
using Sanet.SmartSkating.Services.Tracking;
using Sanet.SmartSkating.ViewModels.Base;

namespace Sanet.SmartSkating.ViewModels
{
    public class LiveSessionViewModel:BaseViewModel
    {
        public const string NoValue = "- - -";
        public const string TotalTimeFormat = "h\\:mm\\:ss";

        private readonly ISessionManager _sessionManager;
        private string _infoLabel = "";
        private string _currentSector = NoValue;
        private string _lastLapTime = NoValue;
        private string _laps = "0";
        private string _lastSectorTime = NoValue;
        private string _distance = "";
        private string _totalTime = "";
        private string _bestLapTime = NoValue;
        private string _bestSectorTime = NoValue;
        private bool _isRunning;

        public LiveSessionViewModel( ISessionManager sessionManager)
        {
            _sessionManager = sessionManager;

            TotalTime = new TimeSpan().ToString(TotalTimeFormat);
        }

        public ICommand StartCommand => new SimpleCommand(async() =>
        {
            await _sessionManager.StartSession();
            _sessionManager.CurrentSession?.SetStartTime(DateTime.UtcNow);
#pragma warning disable 4014
            TrackTime();
#pragma warning restore 4014
        });

        private async Task TrackTime()
        {
            while (_sessionManager.IsRunning)
            {
                await Task.Delay(1000);
                if (_sessionManager.CurrentSession == null) continue;
                UpdateUi();
            } 
        }

        public string TotalTime
        {
            get => _totalTime;
            private set => SetProperty(ref _totalTime, value);
        }

        public void UpdateUi()
        {
            IsRunning = _sessionManager.IsRunning;
            if (!IsRunning)
                return;
            if (_sessionManager.CurrentSession == null) return;
            
            var time = DateTime.UtcNow.Subtract(_sessionManager.CurrentSession.StartTime);
            TotalTime = time.ToString(TotalTimeFormat);
            
            InfoLabel = _sessionManager.CurrentSession.LastCoordinate.ToString();

            if (_sessionManager.CurrentSession.LapsCount > 0)
            {
                LastLapTime = _sessionManager.CurrentSession.LastLapTime.ToString(TotalTimeFormat);
                BestLapTime = _sessionManager.CurrentSession.BestLapTime.ToString(TotalTimeFormat);
            }
            else
            {
                LastLapTime = NoValue;
                BestLapTime = NoValue;
            }

            Laps = _sessionManager.CurrentSession.LapsCount.ToString();
            if (_sessionManager.CurrentSession.Sectors.Count > 0)
            {
                var lastSector = _sessionManager.CurrentSession.Sectors.Last();
                LastSectorTime = lastSector.Time.ToString("mm\\:ss");
                Distance = $"{Math.Round(_sessionManager.CurrentSession.Sectors.Count * 0.1f, 1)}Km";
                if (_sessionManager.CurrentSession.BestSector != null)
                    BestSectorTime = _sessionManager.CurrentSession.BestSector.Value.Time.ToString("mm\\:ss");
            }
            else
            {
                LastSectorTime = NoValue;
                BestSectorTime = NoValue;
            }

            if (_sessionManager.CurrentSession.WayPoints.Count > 0)
                CurrentSector =
                    $"Currently in {_sessionManager.CurrentSession.WayPoints.Last().Type.GetSectorName()} sector";
        }

        public ICommand StopCommand => new SimpleCommand(() =>
        {
            _sessionManager.StopSession();
            InfoLabel = "";
        });

        public bool IsActive { get; private set; }

        public string InfoLabel
        {
            get => _infoLabel;
            private set => SetProperty(ref _infoLabel, value);
        }

        public string CurrentSector
        {
            get => _currentSector;
            private set => SetProperty(ref _currentSector, value);
        }

        public bool CanStart => _sessionManager.CurrentSession != null;

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

        public string LastSectorTime
        {
            get => _lastSectorTime;
            private set => SetProperty(ref _lastSectorTime, value);
        }

        public string Distance
        {
            get => _distance;
            private set => SetProperty(ref _distance, value);
        }

        public string BestLapTime
        {
            get => _bestLapTime;
            private set => SetProperty(ref _bestLapTime, value);
        }

        public string BestSectorTime
        {
            get => _bestSectorTime;
            private set => SetProperty(ref _bestSectorTime, value);
        }

        public bool IsRunning
        {
            get => _isRunning;
            private set => SetProperty(ref _isRunning, value);
        }

        public override void AttachHandlers()
        {
            base.AttachHandlers();
            IsActive = true;
        }

        public override void DetachHandlers()
        {
            base.DetachHandlers();
            IsActive = false;
        }
    }
}
