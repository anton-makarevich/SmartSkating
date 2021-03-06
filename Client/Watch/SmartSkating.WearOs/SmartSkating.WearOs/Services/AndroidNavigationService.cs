using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Sanet.SmartSkating.Services;
using Sanet.SmartSkating.ViewModels;
using Sanet.SmartSkating.ViewModels.Base;
using Sanet.SmartSkating.ViewModels.Wrappers;
using Sanet.SmartSkating.WearOs.Views;
using SimpleInjector;

namespace Sanet.SmartSkating.WearOs.Services
{
    public class AndroidNavigationService:INavigationService
    {
        private readonly Activity _activity;

        public static AndroidNavigationService? SharedInstance { get; private set; }
        
        private readonly Dictionary<Type, Type> _viewModelViewDictionary = new Dictionary<Type, Type>();

        public AndroidNavigationService(Activity activity, Container container)
        {
            _activity = activity;
            Container = container;

            SharedInstance = this;
            RegisterViewModels();
        }
        
        private void RegisterViewModels()
        {
            //For now jist manual registration
            _viewModelViewDictionary.Add(typeof(TracksViewModel), typeof(TracksActivity));
            _viewModelViewDictionary.Add(typeof(SessionsViewModel), typeof(SessionsActivity));
            _viewModelViewDictionary.Add(typeof(LiveSessionViewModel), typeof(LiveSessionActivity));
        }

        public Container Container { get; } 

        public T GetNewViewModel<T>() where T : BaseViewModel
        {
            throw new System.NotImplementedException();
        }

        public T GetViewModel<T>() where T : BaseViewModel
        {
            throw new System.NotImplementedException();
        }

        public bool HasViewModel<T>() where T : BaseViewModel
        {
            throw new System.NotImplementedException();
        }

        public Task NavigateToViewModelAsync<T>(T viewModel) where T : BaseViewModel
        {
            throw new System.NotImplementedException();
        }

        public Task NavigateToViewModelAsync<T>() where T : BaseViewModel
        {
            return Task.Run(() =>
            {
                Intent intent = new Intent(_activity, _viewModelViewDictionary[typeof(T)]);
                
                _activity.StartActivity(intent);
            }); 
        }

        public Task ShowViewModelAsync<T>(T viewModel) where T : BaseViewModel
        {
            throw new System.NotImplementedException();
        }

        public Task ShowViewModelAsync<T>() where T : BaseViewModel
        {
            throw new System.NotImplementedException();
        }

        public Task<TResult> ShowViewModelForResultAsync<T, TResult>(T viewModel) where T : BaseViewModel where TResult : class
        {
            throw new System.NotImplementedException();
        }

        public Task<TResult> ShowViewModelForResultAsync<T, TResult>() where T : BaseViewModel where TResult : class
        {
            throw new System.NotImplementedException();
        }

        public Task NavigateBackAsync()
        {
            throw new System.NotImplementedException();
        }

        public Task CloseAsync()
        {
            throw new System.NotImplementedException();
        }

        public Task NavigateToRootAsync()
        {
            throw new System.NotImplementedException();
        }
    }
}