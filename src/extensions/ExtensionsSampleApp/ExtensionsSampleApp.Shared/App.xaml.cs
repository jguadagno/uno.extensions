﻿using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.DependencyInjection;
using ExtensionsSampleApp.ViewModels;
using ExtensionsSampleApp.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Controls;
using Uno.Extensions.Hosting;
using Uno.Extensions.Logging;
using Uno.Extensions.Navigation;
using Uno.Extensions.Navigation.Controls;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;

#if WINDOWS_UWP || UNO_UWP_COMPATIBILITY
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
#endif

namespace ExtensionsSampleApp
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App : Application
    {
#if NET5_0 && WINDOWS
        private Window _window;

#else
        private Windows.UI.Xaml.Window _window;
#endif
        private IHost Host { get; }
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            Host = UnoHost
               .CreateDefaultBuilder(true)
               .UsePlatformLoggerProvider()
               .ConfigureLogging(logBuilder =>
               {
                   logBuilder
                        .SetMinimumLevel(LogLevel.Debug)
                        .XamlLogLevel(LogLevel.Information)
                        .XamlLayoutLogLevel(LogLevel.Information);
               })
               .ConfigureServices(services =>
               {
                   services
                   .AddScoped<MainViewModel>()
                   .AddScoped<SecondViewModel>()
                   .AddViewModelData<Widget>()
                   .AddScoped<ThirdViewModel>()
                   .AddScoped<FourthViewModel>()
                   .AddScoped<TabbedViewModel>()
                   .AddScoped<TabDoc0ViewModel>()
                   .AddScoped<TabDoc1ViewModel>();
               })
               /*
                * .ConfigureNavigationMapping(mapping=>{
                *   mapping
                *      .Register("path")                    // Path = path
                *      .ForView<TView>()                    // View = typeof(TView)
                *      .WithViewModel<TViewModel>()         // ViewModel = typeof(TViewModel) <-- also need to register with DI
                *      .HandlesData<TData>()                // Data = typeof(TData)  <-- also need to register with DI 
                * })
                */
               .UseNavigation()
               .Build()
               .EnableUnoLogging();
            Ioc.Default.ConfigureServices(Host.Services);

            var mapping = Host.Services.GetService< INavigationMapping>();
            mapping.Register(new NavigationMap(typeof(MainPage).Name, typeof(MainPage), typeof(MainViewModel)));
            mapping.Register(new NavigationMap(typeof(SecondPage).Name, typeof(SecondPage), typeof(SecondViewModel), typeof(Widget)));
            mapping.Register(new NavigationMap(typeof(ThirdPage).Name, typeof(ThirdPage), typeof(ThirdViewModel)));
            mapping.Register(new NavigationMap(typeof(FourthPage).Name, typeof(FourthPage), typeof(FourthViewModel)));
            mapping.Register(new NavigationMap(typeof(TabbedPage).Name, typeof(TabbedPage), typeof(TabbedViewModel)));
            mapping.Register(new NavigationMap("doc0", ViewModel:typeof(TabDoc0ViewModel)));
            mapping.Register(new NavigationMap("doc1", ViewModel: typeof(TabDoc1ViewModel)));
            mapping.Register(new NavigationMap(typeof(Content1).Name, typeof(Content1)));
            mapping.Register(new NavigationMap(typeof(Content2).Name, typeof(Content2)));
            mapping.Register(new NavigationMap(typeof(SimpleContentDialog).Name, typeof(SimpleContentDialog)));


            //InitializeLogging();

            this.InitializeComponent();

#if HAS_UNO || NETFX_CORE
            this.Suspending += OnSuspending;
#endif
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif

#if NET5_0 && WINDOWS
            _window = new Window();
            _window.Activate();
#else
            _window = Windows.UI.Xaml.Window.Current;
#endif

            var rootFrame = _window.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();
                rootFrame.NavigationFailed += OnNavigationFailed;

                // This would normally be done in XAML but since we're manually
                // creating the frame we need to hook it to the navigation service
                Navigation.SetIsEnabled(rootFrame, true);

                // We need to make sure the root frame is loaded before we attempt
                // to navigate otherwise the adapter won't be enabled
                rootFrame.Loaded += (fs, fe) =>
                {
                    var nav = Ioc.Default.GetService<INavigationService>();
                    var navResult = nav.NavigateToView<MainPage>(this);
                };

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                _window.Content = rootFrame;
            }

#if !(NET5_0 && WINDOWS)
            if (e.PrelaunchActivated == false)
#endif
            {
                if (rootFrame.Content == null)
                {
                    //// When the navigation stack isn't restored navigate to the first page,
                    //// configuring the new page by passing required information as a navigation
                    //// parameter
                    //rootFrame.Navigate(typeof(TabbedPage), e.Arguments);
                }
                // Ensure the current window is active
                _window.Activate();
            }

            _ = Task.Run(() =>
            {
                //Startup.Start();
                Host.Run();
            });
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception($"Failed to load {e.SourcePageType.FullName}: {e.Exception}");
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }

        /// <summary>
        /// Configures global Uno Platform logging
        /// </summary>
        private static void InitializeLogging()
        {
            var factory = LoggerFactory.Create(builder =>
            {
#if __WASM__
                builder.AddProvider(new global::Uno.Extensions.Logging.WebAssembly.WebAssemblyConsoleLoggerProvider());
#elif __IOS__
                builder.AddProvider(new global::Uno.Extensions.Logging.OSLogLoggerProvider());
#elif NETFX_CORE
                builder.AddDebug();
#else
                builder.AddConsole();
#endif

                // Exclude logs below this level
                builder.SetMinimumLevel(LogLevel.Information);

                // Default filters for Uno Platform namespaces
                builder.AddFilter("Uno", LogLevel.Warning);
                builder.AddFilter("Windows", LogLevel.Warning);
                builder.AddFilter("Microsoft", LogLevel.Warning);

                // Generic Xaml events
                // builder.AddFilter("Windows.UI.Xaml", LogLevel.Debug );
                // builder.AddFilter("Windows.UI.Xaml.VisualStateGroup", LogLevel.Debug );
                // builder.AddFilter("Windows.UI.Xaml.StateTriggerBase", LogLevel.Debug );
                // builder.AddFilter("Windows.UI.Xaml.UIElement", LogLevel.Debug );
                // builder.AddFilter("Windows.UI.Xaml.FrameworkElement", LogLevel.Trace );

                // Layouter specific messages
                // builder.AddFilter("Windows.UI.Xaml.Controls", LogLevel.Debug );
                // builder.AddFilter("Windows.UI.Xaml.Controls.Layouter", LogLevel.Debug );
                // builder.AddFilter("Windows.UI.Xaml.Controls.Panel", LogLevel.Debug );

                // builder.AddFilter("Windows.Storage", LogLevel.Debug );

                // Binding related messages
                // builder.AddFilter("Windows.UI.Xaml.Data", LogLevel.Debug );
                // builder.AddFilter("Windows.UI.Xaml.Data", LogLevel.Debug );

                // Binder memory references tracking
                // builder.AddFilter("Uno.UI.DataBinding.BinderReferenceHolder", LogLevel.Debug );

                // RemoteControl and HotReload related
                // builder.AddFilter("Uno.UI.RemoteControl", LogLevel.Information);

                // Debug JS interop
                // builder.AddFilter("Uno.Foundation.WebAssemblyRuntime", LogLevel.Debug );
            });

            global::Uno.Extensions.LogExtensionPoint.AmbientLoggerFactory = factory;
        }
    }

    public static class VisualTreeUtils
    {
        public static T FindVisualChildByType<T>(this DependencyObject element)
            where T : DependencyObject
        {
            if (element == null)
            {
                return default(T);
            }

            if (element is T elementAsT)
            {
                return elementAsT;
            }

            int childrenCount = VisualTreeHelper.GetChildrenCount(element);
            for (int i = 0; i < childrenCount; i++)
            {
                var result = VisualTreeHelper.GetChild(element, i).FindVisualChildByType<T>();
                if (result != null)
                {
                    return result;
                }
            }

            return default(T);
        }

        public static FrameworkElement FindVisualChildByName(this DependencyObject element, string name)
        {
            if (element == null || string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            if (element is FrameworkElement elementAsFE && elementAsFE.Name == name)
            {
                return elementAsFE;
            }

            int childrenCount = VisualTreeHelper.GetChildrenCount(element);
            for (int i = 0; i < childrenCount; i++)
            {
                var result = VisualTreeHelper.GetChild(element, i).FindVisualChildByName(name);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }
    }
}
