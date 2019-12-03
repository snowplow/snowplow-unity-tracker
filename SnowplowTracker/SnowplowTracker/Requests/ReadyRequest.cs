/*
 * GetRequest.cs
 * SnowplowTracker.Requests
 * 
 * Copyright (c) 2015 Snowplow Analytics Ltd. All rights reserved.
 *
 * This program is licensed to you under the Apache License Version 2.0,
 * and you may not use this file except in compliance with the Apache License Version 2.0.
 * You may obtain a copy of the Apache License Version 2.0 at http://www.apache.org/licenses/LICENSE-2.0.
 *
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the Apache License Version 2.0 is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the Apache License Version 2.0 for the specific language governing permissions and limitations there under.
 * 
 * Authors: Joshua Beemster, Paul Boocock
 * Copyright: Copyright (c) 2015-2019 Snowplow Analytics Ltd
 * License: Apache License Version 2.0
 */

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using SnowplowTracker.Collections;

namespace SnowplowTracker.Requests
{
    public class ReadyRequest
    {
        private static readonly HttpClient client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(10)
        };

        private readonly HttpRequest request;
        private readonly List<Guid> rowIds;
        private readonly bool oversize;
        private readonly ConcurrentQueue<RequestResult> resultQueue;

        /// <summary>
        /// Initializes a new instance of the <see cref="SnowplowTracker.Requests.ReadyRequest"/> class.
        /// </summary>
        /// <param name="request">Request.</param>
        /// <param name="rowIds">Row identifiers.</param>
        /// <param name="oversize">If set to <c>true</c> oversize.</param>
        /// <param name="resultQueue">The queue that sent events will be added to</param>
        public ReadyRequest(HttpRequest request, List<Guid> rowIds, bool oversize, ConcurrentQueue<RequestResult> resultQueue)
        {
            this.request = request;
            this.rowIds = rowIds;
            this.oversize = oversize;
            this.resultQueue = resultQueue;
        }

        /// <summary>
        /// Send a blocking request.
        /// </summary>
        public void Send()
        {
            try {
                Task<HttpResponseMessage> response = null;

                switch (request.Method)
                {
                    case Enums.HttpMethod.POST:
                        response = client.PostAsync(request.CollectorUri, request.Content);
                        break;
                    case Enums.HttpMethod.GET:
                        response = client.GetAsync(request.CollectorUri);
                        break;
                }

                AddToResultQueue(response.Result);
            }
            catch (Exception)
            { 
                AddToResultQueue(new HttpResponseMessage(HttpStatusCode.BadRequest));
            }
        }

        private void AddToResultQueue(HttpResponseMessage response)
        {
            var success = oversize || response.IsSuccessStatusCode;
            resultQueue.Enqueue(new RequestResult(success, rowIds));
        }
    }
}
