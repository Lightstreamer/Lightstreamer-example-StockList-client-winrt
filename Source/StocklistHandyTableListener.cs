﻿#region License
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

using System;
using System.Text;
using System.Threading;
using Lightstreamer.DotNet.Client;

namespace WinRTStockListDemo
{
    class StocklistHandyTableListener : IHandyTableListener
    {
        private ILightstreamerListener listener = null;
        private const int lockt = 15000;
        private int phase;

        public StocklistHandyTableListener(ILightstreamerListener listener, int phase)
        {
            this.listener = listener;
            this.phase = phase;
        }

        public void OnUpdate(int itemPos, string itemName, IUpdateInfo update)
        {
            listener.OnItemUpdate(phase, itemPos, itemName, update);
        }

        public void OnRawUpdatesLost(int itemPos, string itemName, int lostUpdates)
        {
            listener.OnLostUpdate(phase, itemPos, itemName, lostUpdates);
        }

        public void OnSnapshotEnd(int itemPos, string itemName)
        {
        }

        public void OnUnsubscr(int itemPos, string itemName)
        {
        }

        public void OnUnsubscrAll()
        {
        }
    }
}