using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NSubstitute;
using Sanet.SmartSkating.Dto.Models;
using Sanet.SmartSkating.Models.Geometry;
using Sanet.SmartSkating.Services;
using Sanet.SmartSkating.Services.Tracking;
using Sanet.SmartSkating.Tests.Models.Geometry;
using Sanet.SmartSkating.Tests.Services.Tracking;
using Sanet.SmartSkating.ViewModels;
using Sanet.SmartSkating.ViewModels.Wrappers;
using Xunit;

namespace Sanet.SmartSkating.Tests.ViewModels
{
    public class TracksViewModelTests
    {
        private readonly ITrackService _trackServiceMock = Substitute.For<ITrackService>();
        private readonly INavigationService _navigationServiceMock = Substitute.For<INavigationService>();
        private readonly TracksViewModel _sut;
        private readonly List<TrackDto> _tracks;

        public TracksViewModelTests()
        {
            _sut = new TracksViewModel(_trackServiceMock);
            _sut.SetNavigationService(_navigationServiceMock);
            _tracks = JsonConvert.DeserializeObject<List<TrackDto>>(TrackServiceTests.TracksData);
            _trackServiceMock.Tracks.Returns(_tracks);
        }

        [Fact]
        public async Task LoadsTracksFromService()
        {
            await _sut.LoadTracksAsync();

            await _trackServiceMock.Received().LoadTracksAsync();
            Assert.NotEmpty(_sut.Tracks);
        }

        [Fact]
        public async Task LoadsTracksFromServiceOnPageAppear()
        {
            _sut.AttachHandlers();

            await _trackServiceMock.Received().LoadTracksAsync();
            Assert.NotEmpty(_sut.Tracks);
        }

        [Fact]
        public async Task HasSelectedTrackIsFalseIfNoneOfTracksIsSelected()
        {
            await _sut.LoadTracksAsync();

            Assert.False(_sut.HasSelectedTrack);
        }

        [Fact]
        public async Task SelectTrackSetsItsIsSelectedToTrue()
        {
            await _sut.LoadTracksAsync();
            var track = _sut.Tracks.First();

            _sut.SelectTrack(track);

            Assert.True(track.IsSelected);
        }

        [Fact]
        public async Task SelectTrackRemovesIsSelectedFromOtherTracks()
        {
            await _sut.LoadTracksAsync();
            var track = _sut.Tracks.First();
            track.IsSelected = true;
            var secondTrack = _sut.Tracks.Last();

            _sut.SelectTrack(secondTrack);

            Assert.False(track.IsSelected);
        }

        [Fact]
        public async Task SelectTrackDoesNotChangeIsSelectIsTrackIsNotPartOfViewModel()
        {
            await _sut.LoadTracksAsync();
            var trackVm = new TrackViewModel(new TrackDto
            {
                Name = "SomeTrack",
                Start = new CoordinateDto{Latitude = 11,Longitude = 45},
                Finish = new CoordinateDto{Latitude = 16,Longitude = 25},
            });

            _sut.SelectTrack(trackVm);

            Assert.False(trackVm.IsSelected);
        }

        [Fact]
        public async Task HasSelectedTrackIsTrueWhenOneTrackIsSelected()
        {
            await _sut.LoadTracksAsync();
            var track = _sut.Tracks.First();

            _sut.SelectTrack(track);

            Assert.True(_sut.HasSelectedTrack);
        }

        [Fact]
        public async Task SelectTrackRaisesPropertyChangedForHasSelectedTrack()
        {
            await _sut.LoadTracksAsync();
            var track = _sut.Tracks.First();
            var hasSelectedUpdatedTimes = 0;
            _sut.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(_sut.HasSelectedTrack))
                    hasSelectedUpdatedTimes++;
            };

            _sut.SelectTrack(track);

            Assert.Equal(1,hasSelectedUpdatedTimes);
        }

        [Fact]
        public async Task SelectTrackUpdatesCurrentRinkInService()
        {
            await _sut.LoadTracksAsync();
            var track = _sut.Tracks.First();

            _sut.SelectTrack(track);

            _trackServiceMock.Received().SelectRinkByName(track.Name);
        }

        [Fact]
        public async Task ConfirmingSelectionCallsNavigationIfTrackIsSelected()
        {
            await _sut.LoadTracksAsync();
            var track = _sut.Tracks.First();
            SelectRink();
            _sut.SelectTrack(track);

            _sut.ConfirmSelectionCommand.Execute(null);

            await _navigationServiceMock.Received().NavigateToViewModelAsync<SessionsViewModel>();
        }

        [Fact]
        public async Task ConfirmingSelectionDoesNotCallNavigationIfTrackIsNotSelected()
        {
            await _sut.LoadTracksAsync();

            _sut.ConfirmSelectionCommand.Execute(null);

            await _navigationServiceMock.DidNotReceive().NavigateToViewModelAsync<SessionsViewModel>();
        }

        [Fact]
        public void DoesNotCrashWhenAccessingVmBeforeLoad()
        {
            Assert.False(_sut.HasSelectedTrack);
        }

        [Fact]
        public async Task DoesNotAddDuplicatedTracks()
        {
            await _sut.LoadTracksAsync();
            await _sut.LoadTracksAsync();

            Assert.Equal(_trackServiceMock.Tracks.Count, _sut.Tracks.Count);
        }

        [Fact]
        public async Task SelectsTrackAutomaticallyAndGoesToSessionIfItIsTheOnlyOne()
        {
            const string name = "Eindhoven";
            _trackServiceMock.Tracks.Returns(_tracks.Where(t=>t.Name=="Eindhoven").ToList());
            SelectRink();

            await _sut.LoadTracksAsync();

            _trackServiceMock.Received().SelectRinkByName(name);
            await _navigationServiceMock.Received().NavigateToViewModelAsync<SessionsViewModel>();
        }

        private void SelectRink()
        {
            var rink = new Rink(RinkTests.EindhovenStart, RinkTests.EindhovenFinish, "RinkId");
            _trackServiceMock.SelectedRink.Returns(rink);
        }
    }
}
