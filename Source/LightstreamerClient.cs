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
using System.Text;
using System.Net;

using Lightstreamer.DotNet.Client;

// This is the class handling the Lightstreamer Client,
// the entry point for Lightstreamer update events.

namespace WinRTStockListDemo
{
    public class LightstreamerClient
    {
        private string[] items;
        private string[] fields;

        private LSClient client;

        private Boolean _started = false;

        public Boolean started
        {
            get
            {
                return this._started;
            }
        }

        public LightstreamerClient(string[] items, string[] fields)
        {
            this.items = items;
            this.fields = fields;
            client = new LSClient();
           
        }

        public void Stop()
        {
            this._started = false;
            client.CloseConnection();
        }

        public void Start(string pushServerUrl, int phase, ILightstreamerListener listener)
        {
            this._started = true;
            StocklistConnectionListener ls = new StocklistConnectionListener(listener, phase);

            ConnectionInfo connInfo = new ConnectionInfo();
            connInfo.PushServerUrl = pushServerUrl;
            connInfo.Adapter = "DEMO";
            client.OpenConnection(connInfo, ls);

        }

        public void Subscribe(int phase, ILightstreamerListener listener) {

            StocklistHandyTableListener hl = new StocklistHandyTableListener(listener, phase);

            SimpleTableInfo tableInfo = new ExtendedTableInfo(
                items, "MERGE", fields, true);
            tableInfo.DataAdapter = "QUOTE_ADAPTER";
            client.SubscribeTable(tableInfo, hl, false);
        }



    }
}


