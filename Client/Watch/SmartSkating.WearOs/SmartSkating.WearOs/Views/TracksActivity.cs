using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Android.App;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Sanet.SmartSkating.ViewModels;
using Sanet.SmartSkating.WearOs.Models;
using Sanet.SmartSkating.WearOs.Services;

namespace Sanet.SmartSkating.WearOs.Views
{
    [Activity]
    public class TracksActivity: BaseActivity<TracksViewModel>
    {
        private RecyclerView? _recyclerView;
        private Button? _confirmButton;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_tracks);
            
            _recyclerView = FindViewById<RecyclerView> (Resource.Id.recyclerView);
            _confirmButton = FindViewById<Button>(Resource.Id.confirmButton);
            
            _confirmButton.Click += ConfirmButtonOnClick;
            
            SetViewModel();
            
            var layoutManager = new LinearLayoutManager (this);
            _recyclerView.SetLayoutManager (layoutManager);
        }

        private void ConfirmButtonOnClick(object sender, EventArgs e)
        {
            ViewModel.ConfirmSelectionCommand.Execute(null);
        }

        private void SetViewModel()
        {
            ViewModel = AndroidNavigationService.SharedInstance.Container.GetInstance<TracksViewModel>();
            ViewModel.SetNavigationService(AndroidNavigationService.SharedInstance);
            ViewModel.PropertyChanged += ViewModelOnPropertyChanged;
            ViewModel.Tracks.CollectionChanged += TracksOnCollectionChanged;
            UpdateButtonsState();
        }

        private void TracksOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_recyclerView?.GetAdapter() is TracksAdapter tracksAdapter)
                tracksAdapter.ItemClick-= AdapterOnItemClick;
            var adapter = new TracksAdapter(ViewModel.Tracks.ToList());
            adapter.ItemClick+= AdapterOnItemClick;
            _recyclerView?.SetAdapter(adapter);
        }

        private void AdapterOnItemClick(object sender, int e)
        {
            ViewModel.SelectTrack(ViewModel.Tracks[e]);
        }

        private void ViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.HasSelectedTrack))
            {
                UpdateButtonsState();
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            ViewModel.Tracks.CollectionChanged -= TracksOnCollectionChanged;
            ViewModel.PropertyChanged -= ViewModelOnPropertyChanged;
        }
        
        private void UpdateButtonsState()
        {
            if (_confirmButton != null)
                _confirmButton.Enabled = ViewModel.HasSelectedTrack;
        }
    }
}