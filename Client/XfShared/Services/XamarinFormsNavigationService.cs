using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Sanet.SmartSkating.Services;
using Sanet.SmartSkating.ViewModels.Base;
using Sanet.SmartSkating.Views;
using Sanet.SmartSkating.Xf.Views.Base;
using SimpleInjector;
using Xamarin.Forms;

namespace Sanet.SmartSkating.Xf.Services
{
    public class XamarinFormsNavigationService : INavigationService
    {
        private readonly List<BaseViewModel> _viewModels = new List<BaseViewModel>();

        private static INavigation FormsNavigation => Application.Current.MainPage.Navigation;

        private readonly Dictionary<Type, Type> _viewModelViewDictionary = new Dictionary<Type, Type>();
        private readonly Container _container;

        public XamarinFormsNavigationService(Container container)
        {
            _container = container;
            RegisterViewModels();
        }

        private void RegisterViewModels()
        {
            var assembly = typeof(XamarinFormsNavigationService).GetTypeInfo().Assembly;

            foreach (var type in assembly.DefinedTypes.Where(dt => !dt.IsAbstract &&
                                                                   dt.ImplementedInterfaces.Any(ii =>
                                                                       ii == typeof(IBaseView))))
            {
                var viewForType = type.ImplementedInterfaces.FirstOrDefault(
                    ii => ii.IsConstructedGenericType &&
                          ii.GetGenericTypeDefinition() == typeof(IBaseView<>));

                if (viewForType != null)
                    _viewModelViewDictionary.Add(viewForType.GenericTypeArguments[0], type.AsType());
            }
        }

        public T GetViewModel<T>() where T : BaseViewModel
        {
            var vm = (T) _viewModels.FirstOrDefault(f => f is T);
            if (vm != null) return vm;
            vm = CreateViewModel<T>();
            _viewModels.Add(vm);
            return vm;
        }

        public T GetNewViewModel<T>() where T : BaseViewModel
        {
            var vm = (T) _viewModels.FirstOrDefault(f => f is T);

            if (vm != null)
            {
                _viewModels.Remove(vm);
            }

            vm = CreateViewModel<T>();
            _viewModels.Add(vm);
            return vm;
        }

        private T CreateViewModel<T>() where T : BaseViewModel
        {
            var vm = _container.GetInstance<T>();
            vm.SetNavigationService(this);
            return vm;
        }

        public bool HasViewModel<T>() where T : BaseViewModel
        {
            var vm = (T) _viewModels.FirstOrDefault(f => f is T);
            return (vm != null);
        }

        public Task NavigateToViewModelAsync<T>(T viewModel) where T : BaseViewModel
        {
            return OpenViewModelAsync(viewModel);
        }

        public Task NavigateToViewModelAsync<T>() where T : BaseViewModel
        {
            var vm = GetViewModel<T>();
            return OpenViewModelAsync(vm);
        }

        private async Task OpenViewModelAsync<T>(T viewModel, bool modalPresentation = false)
            where T : BaseViewModel
        {
            if (viewModel == null)
                return;
            if (viewModel.NavigationService == null)
                viewModel.SetNavigationService(this);
            if (CreateView(viewModel) is BaseView<T> view)
            {
                if (modalPresentation)
                    await FormsNavigation.PushModalAsync(view);
                else
                    await FormsNavigation.PushAsync(view);
            }
        }

        private IBaseView CreateView(BaseViewModel viewModel)
        {
            var viewModelType = viewModel.GetType();

            var viewType = _viewModelViewDictionary[viewModelType];

            var view = (IBaseView) Activator.CreateInstance(viewType);

            view.ViewModel = viewModel;

            return view;
        }

        public async Task NavigateBackAsync()
        {
            await FormsNavigation.PopAsync();
        }

        public async Task CloseAsync()
        {
            await FormsNavigation.PopModalAsync();
        }

        public async Task NavigateToRootAsync()
        {
            try
            {
                await CloseAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            await FormsNavigation.PopToRootAsync();
        }

        public Task ShowViewModelAsync<T>(T viewModel) where T : BaseViewModel
        {
            return OpenViewModelAsync(viewModel, true);
        }

        public Task ShowViewModelAsync<T>() where T : BaseViewModel
        {
            var viewModel = GetViewModel<T>();
            return ShowViewModelAsync(viewModel);
        }

        public async Task<TResult> ShowViewModelForResultAsync<T, TResult>(T viewModel)
            where T : BaseViewModel
            where TResult : class
        {
            viewModel.ExpectsResult = true;

            var taskCompletionSource = new TaskCompletionSource<TResult>();

            void OnViewModelOnOnResult(object sender, object? o)
            {
                if (viewModel != null) 
                    viewModel.OnResult -= OnViewModelOnOnResult;
                if (o is TResult result) 
                    taskCompletionSource.TrySetResult(result);
            }

            viewModel.OnResult += OnViewModelOnOnResult;
            await OpenViewModelAsync(viewModel, true);

            return await taskCompletionSource.Task;
        }

        public async Task<TResult> ShowViewModelForResultAsync<T, TResult>()
            where T : BaseViewModel
            where TResult : class
        {
            var viewModel = GetViewModel<T>();
            return await ShowViewModelForResultAsync<T, TResult>(viewModel);
        }
    }
}