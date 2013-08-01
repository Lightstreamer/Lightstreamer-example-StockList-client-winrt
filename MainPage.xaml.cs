#region License
/*
* Copyright 2013 Weswit Srl
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

using System;
using System.Collections.Generic;
using System.IO;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Media.Animation;
using Lightstreamer.DotNet.Client;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;
using Windows.UI.ApplicationSettings;

namespace WinRTStockListDemo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, ILightstreamerListener
    {

        class UpdateCell {

            TextBlock tb;
            Storyboard sbUp, sbDown, sbSame;

            public UpdateCell(TextBlock tb, Storyboard sbUp, Storyboard sbDown, Storyboard sbSame)
            {
                this.tb = tb;
                this.sbUp = sbUp;
                this.sbDown = sbDown;
                this.sbSame = sbSame;
            }

            public async void SetText(string message)
            {
                await tb.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    tb.Text = message;
                });
            }

            public async void AnimateUp()
            {
                await sbUp.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    sbUp.Begin();
                });
            }

            public async void AnimateDown()
            {
                await sbDown.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    sbDown.Begin();
                });
            }

            public async void AnimateSame()
            {
                await sbSame.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    sbSame.Begin();
                });
            }

            public void Animate(IUpdateInfo update, string field, Dictionary<int, UpdateCell> TextMap)
            {
                string oldval = update.GetOldValue(field);
                string newval = update.GetNewValue(field);
                int action = 0;
                try
                {
                    if (Convert.ToDouble(oldval) > Convert.ToDouble(newval))
                        action = -1;
                    else
                        action = 1;
                } catch (FormatException) {
                    // ignore
                }
                foreach (UpdateCell cell in TextMap.Values)
                {
                    if (action > 0)
                        cell.AnimateUp();
                    else if (action < 0)
                        cell.AnimateDown();
                    else
                        cell.AnimateSame();
                }
            }

        }

    
   
        private Dictionary<int, Dictionary<int, UpdateCell>> RowMap = new Dictionary<int, Dictionary<int, UpdateCell>>();

        public static Boolean wantsConnection = true;

        XmlDocument tileXmlLarge = TileUpdateManager.GetTemplateContent(TileTemplateType.TileWideText01);
        XmlDocument tileXmlSquare = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquareText03);

        TileUpdater tileUpdater = TileUpdateManager.CreateTileUpdaterForApplication();

        public MainPage()
        {
            InitializeComponent();
            InitializeTable();

            var node = tileXmlLarge.ImportNode(tileXmlSquare.GetElementsByTagName("binding").Item(0), true);
            tileXmlLarge.GetElementsByTagName("visual").Item(0).AppendChild(node);


            SettingsPane.GetForCurrentView().CommandsRequested += CommandsRequested;

            App.SetListener(this);

            



        }

        private void CommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
        {
            args.Request.ApplicationCommands.Clear();
            SettingsCommand pref = new SettingsCommand("privacy", "Privacy Policy", (uiCommand) =>
            {
                Windows.System.Launcher.LaunchUriAsync(new Uri("http://www.lightstreamer.com/privacy-policy"));
            });
            args.Request.ApplicationCommands.Add(pref);
        }

        private void InitializeTable()
        {

            Color toUse = Colors.White;
           

            // Add TextBlocks to Cells
            for (int i = 0; i < ContentPanel.RowDefinitions.Count; i++)
            {
                if (i == 0)
                    // title row
                    continue;


                Dictionary<int, UpdateCell> TextMap = new Dictionary<int, UpdateCell>();
                RowMap.Add(i - 1, TextMap);

                // for each column
                for (int y = 0; y < ContentPanel.ColumnDefinitions.Count; y++)
                {
                    TextBlock tb = new TextBlock();
                    if (i > 1)
                        tb.Text = "--";
                    SolidColorBrush colorB = new SolidColorBrush(toUse);
                    tb.Foreground = colorB;
                    Grid.SetRow(tb, i - 1);
                    Grid.SetColumn(tb, y);
                    Storyboard sbUp = new Storyboard();
                    Storyboard sbDown = new Storyboard();
                    Storyboard sbSame = new Storyboard();

                    // highlight color animation (in case of positive update)
                    ColorAnimation colorUp = new ColorAnimation();
                    colorUp.From = toUse;
                    colorUp.To = Colors.Green;
                    colorUp.AutoReverse = true;
                    colorUp.Duration = new Duration(TimeSpan.FromSeconds(0.6));
                    Storyboard.SetTarget(colorUp, tb.Foreground);
                    Storyboard.SetTargetProperty(colorUp, "Color");
                    sbUp.Children.Add(colorUp);

                    // highlight color animation (in case of negative update)
                    ColorAnimation colorDown = new ColorAnimation();
                    colorDown.From = toUse;
                    colorDown.To = Colors.Red;
                    colorDown.AutoReverse = true;
                    colorDown.Duration = new Duration(TimeSpan.FromSeconds(0.6));
                    Storyboard.SetTarget(colorDown, tb.Foreground);
                    Storyboard.SetTargetProperty(colorDown, "Color");
                    sbDown.Children.Add(colorDown);

                    // highlight color animation (in case of stable)
                    ColorAnimation colorSame = new ColorAnimation();
                    colorSame.From = toUse;
                    colorSame.To = Colors.Orange;
                    colorSame.AutoReverse = true;
                    colorSame.Duration = new Duration(TimeSpan.FromSeconds(0.6));
                    Storyboard.SetTarget(colorSame, tb.Foreground);
                    Storyboard.SetTargetProperty(colorSame, "Color");
                    sbSame.Children.Add(colorSame);

                    UpdateCell cell = new UpdateCell(tb, sbUp, sbDown, sbSame);
                    TextMap.Add(y, cell);
                    ContentPanel.Children.Add(tb);

                }
            }
        }

        public void UpdateStatus(int status, string message)
        {
            string icon = null;
            if (status == LightstreamerConnectionHandler.DISCONNECTED)
            {
                icon = "status_disconnected.png";
            }
            else if (status == LightstreamerConnectionHandler.STALLED)
            {
                icon = "status_stalled.png";
            }
            else if (status == LightstreamerConnectionHandler.STREAMING)
            {
                icon = "status_connected_streaming.png";
            }
            else if (status == LightstreamerConnectionHandler.POLLING)
            {
                icon = "status_connected_polling.png";
            }
            else if (status == LightstreamerConnectionHandler.CONNECTING)
            {
                icon = "status_disconnected.png";
            }
            else if (status == LightstreamerConnectionHandler.ERROR)
            {
                //we may show it somewhere
            }
            if (icon != null)
            {

                StatusImage.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    Uri iconUri = new Uri("ms-appx:/Assets/" + icon,
                        UriKind.Absolute);



                    BitmapImage iconSource = new BitmapImage(iconUri);
                    StatusImage.Source = iconSource;
                });
            }

            if (message != null)
            {
                StatusLabel.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    StatusLabel.Text = message;
                });
            }
        }

        public void OnStatusChange(int phase, int status, string message)
        {
            if (!App.checkPhase(phase))
            {
                return;
            }
            UpdateStatus(status, message);
        }

        public void OnItemUpdate(int phase, int itemPos, string itemName, IUpdateInfo update)
        {
            if (!App.checkPhase(phase))
            {
                return;
            }
            Dictionary<int, UpdateCell> TextMap;
            if (RowMap.TryGetValue(itemPos, out TextMap))
            {

                for (int i = 0; i < App.fields.Length; i++)
                {
                    string field = App.fields[i];
                    if (update.IsValueChanged(field) || update.Snapshot)
                    {
                        UpdateCell cell;
                        if (TextMap.TryGetValue(i, out cell))
                        {
                            cell.SetText(update.GetNewValue(field));
                            if (field.Equals("last_price"))
                                cell.Animate(update, field, TextMap);
                        }
                    }

                }

                if (itemPos == 2)
                {
                    if (update.IsValueChanged("last_price")) 
                    {

                        XmlNodeList tileTextAttributes = tileXmlLarge.GetElementsByTagName("text");
                        tileTextAttributes[0].InnerText = update.GetNewValue("stock_name");
                        tileTextAttributes[1].InnerText = update.GetNewValue("last_price");
                        tileTextAttributes[2].InnerText = update.GetNewValue("time");
                        tileTextAttributes[3].InnerText = update.GetNewValue("pct_change")+"%";

                        tileTextAttributes[5].InnerText = update.GetNewValue("stock_name");
                        tileTextAttributes[6].InnerText = update.GetNewValue("last_price");
                        tileTextAttributes[7].InnerText = update.GetNewValue("time");
                        tileTextAttributes[8].InnerText = update.GetNewValue("pct_change") + "%";
              
                        TileNotification updateNotification = new TileNotification(tileXmlLarge);
                        updateNotification.ExpirationTime = DateTimeOffset.Now.AddMinutes(1);
                        tileUpdater.Update(updateNotification);

                    }
                }

            }

        }

        public void OnLostUpdate(int phase, int itemPos, string itemName, int lostUpdates)
        {
            if (!App.checkPhase(phase))
            {
                return;
            }
        }

        public void OnReconnectRequest(int phase)
        {
            if (!App.checkPhase(phase))
            {
                return;
            }
          
            App.SpawnLightstreamerClientStart();
        }

        private void AnimateCellsStalling()
        {
            foreach (Dictionary<int, UpdateCell> map in RowMap.Values)
            {
                foreach (UpdateCell cell in map.Values)
                {
                    cell.AnimateSame();
                }
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            wantsConnection = !wantsConnection;
            App.StartStop(wantsConnection,false);
            if (wantsConnection)
            {
                button1.Content = "Stop";
            }
            else
            {
                button1.Content = "Start";
            }
            AnimateCellsStalling();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

            Windows.Storage.ApplicationDataContainer settings = Windows.Storage.ApplicationData.Current.LocalSettings;
            if (settings.Values.ContainsKey("started") && settings.Values["started"].ToString() == "false")
            {
                button1.Content = "Start";
                wantsConnection = false;
            }
            else
            {
                button1.Content = "Stop";
                wantsConnection = true;
            }
            App.StartStop(wantsConnection,true);

            AnimateCellsStalling();

        }

      
    }
}
