#region License
/*
* Copyright (c) Lightstreamer Srl
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/
#endregion License

using Lightstreamer.DotNet.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System.Threading;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace WinRTStockListDemo
{

    sealed partial class App : Application
    {

        private const string pushServerHost = "http://push.lightstreamer.com"; //internal note: switching to SSL requires changing the cryptography declaration on the windows store
        //private const string pushServerHost = "http://localhost:8080";
        public static string[] items = {"item1", "item2", "item3", "item4", "item5",
        "item6", "item7", "item8", "item9", "item10", "item11", "item12", "item13",
        "item14", "item15"};
        public static string[] fields = { "stock_name", "last_price", "time", "pct_change", "bid_quantity", "bid", "ask", "ask_quantity", "min", "max", "ref_price", "open_price" };

        private static Object ConnLock = new Object();
        private static int phase = 0;
        private static int lastDelay = 1;

        private static ILightstreamerListener listener = null;

        public static LightstreamerClient client = new LightstreamerClient(items, fields);



        public App()
        {
            InitializeComponent();
            this.Suspending += OnResuming;
        }


        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;

            }

            if (rootFrame.Content == null)
            {
                // This application has only one page, we always navigate to it
                if (!rootFrame.Navigate(typeof(MainPage), args.Arguments))
                {
                    throw new Exception("Failed to create initial page");
                }
            }
            // Ensure the current window is active
            Window.Current.Activate();

        }

        private void OnResuming(object sender, SuspendingEventArgs e) {

        }

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            
            //we only need to store one single boolean about the status of the application, We do so in the Start and Stop method each time such methods are called

            deferral.Complete();

            
        }


        // HANDLE CONNECTION

        public static Boolean checkPhase(int ph)
        {
            lock (ConnLock)
            {
                return ph == phase;
            }
        }


        private async static void PauseAndRetry(int ph, Exception ee)
        {
            Boolean waitingNet = false;
            lastDelay *= 2;
            // Probably a connection issue, ask myself to respawn
            for (int i = lastDelay; i > 0; i--)
            {

                if (!checkPhase(ph))
                {
                    return;
                }

                if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                {
                    waitingNet = true;
                    listener.OnStatusChange(ph, LightstreamerConnectionHandler.CONNECTING, "Network unavailble, next check in " + i + " seconds");
                }
                else if (waitingNet)
                {
                    listener.OnReconnectRequest(ph);
                    return;
                } 
                else 
                {
                    listener.OnStatusChange(ph, LightstreamerConnectionHandler.CONNECTING, "Connection failed, retrying in " + i + " seconds");
                }

                await Task.Delay(1000);

            }

            listener.OnReconnectRequest(ph);
           
        }

        internal static void SetListener(ILightstreamerListener _listener)
        {
            listener = _listener;
        }

        private async static void Start(int ph)
        {

            if (!checkPhase(ph))
            {
                return;
            }

            Windows.Storage.ApplicationDataContainer settings = Windows.Storage.ApplicationData.Current.LocalSettings;
            settings.Values["started"] = "true";


            while (listener == null)
            {
                //or we may use a different listener that will pass the received values to
                //the front-end once the front-end is ready
                await Task.Delay(500);

                if (!checkPhase(ph))
                {
                    return;
                }

            }

            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                PauseAndRetry(ph, null);
                return;
            }

            try
            {
                if (!checkPhase(ph))
                {
                    return;
                }

                listener.OnStatusChange(ph, LightstreamerConnectionHandler.CONNECTING, "Connecting to " + pushServerHost);

                client.Start(pushServerHost, phase, listener);
                lastDelay = 1;

                if (!checkPhase(ph))
                {
                    return;
                }

                client.Subscribe(ph, listener);


            }

            catch (PushConnException pce)
            {
                PauseAndRetry(ph, pce);
            }
            catch (PushUserException pce)
            {
                PauseAndRetry(ph, pce);
            }
            catch (SubscrException se)
            {
                PauseAndRetry(ph, se);
            }

        }

        private static void Stop(int ph)
        {

           
            if (!checkPhase(ph))
            {
                return;
            }


            Windows.Storage.ApplicationDataContainer settings = Windows.Storage.ApplicationData.Current.LocalSettings;
            settings.Values["started"] = "false";

            client.Stop();
            if (listener != null)
            {
                listener.OnStatusChange(ph, LightstreamerConnectionHandler.DISCONNECTED, "Disconnected");
            }

        }

        async public static void SpawnLightstreamerClientStart()
        {
            int tup;
            lock (ConnLock)
            {
                tup = ++phase;
            }
          
            ThreadPool.RunAsync((IAsyncAction operation) =>
            {
                Start(tup);
            });
           
        }

        async public static void SpawnLightstreamerClientStop()
        {
            int tup;
            lock (ConnLock)
            {
                tup = ++phase;
            }
          
            ThreadPool.RunAsync((IAsyncAction operation) =>
            {
                Stop(tup);
            });

        }



        public static void StartStop(Boolean wantsConnection, Boolean startup)
        {
            lastDelay = 1;

            // This event triggers LightStreamer Client start/stop.
            if (!wantsConnection)
            {
                // stop
                App.SpawnLightstreamerClientStop();
            }
            else
            {
                // start
                App.SpawnLightstreamerClientStart();
            }
        }

    }
}
