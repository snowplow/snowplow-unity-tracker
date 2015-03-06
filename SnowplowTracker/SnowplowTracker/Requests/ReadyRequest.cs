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
 * Authors: Joshua Beemster
 * Copyright: Copyright (c) 2015 Snowplow Analytics Ltd
 * License: Apache License Version 2.0
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityHTTP;
using SnowplowTracker.Payloads;
using SnowplowTracker.Collections;

namespace SnowplowTracker.Requests {
	public class ReadyRequest {

		private Request request;
		private List<int> rowIds;
		private bool oversize;
		private ConcurrentQueue<RequestResult> resultQueue;

		/// <summary>
		/// Initializes a new instance of the <see cref="SnowplowTracker.Requests.ReadyRequest"/> class.
		/// </summary>
		/// <param name="request">Request.</param>
		/// <param name="rowIds">Row identifiers.</param>
		/// <param name="oversize">If set to <c>true</c> oversize.</param>
		public ReadyRequest(Request request, List<int> rowIds, bool oversize, ConcurrentQueue<RequestResult> resultQueue) {
			this.rowIds = rowIds;
			this.oversize = oversize;
			this.resultQueue = resultQueue;
			this.request = request;
		}

		/// <summary>
		/// Send the request with the callback mechanism.
		/// </summary>
		public void Send() {
			request.Send ((Action<UnityHTTP.Request>) RequestCallback);
		}

		/// <summary>
		/// Waits for the result to be ready and sleeps for intervals of 1ms whilst waiting.
		/// </summary>
		public void RequestCallback(UnityHTTP.Request result) {
			if (this.oversize) {
				resultQueue.Enqueue(new RequestResult(true, this.rowIds));
			} else {
				if (result.response != null) {
					int code = result.response.status;
					bool success = Utils.IsSuccessfulRequest(code);
					resultQueue.Enqueue(new RequestResult(success, this.rowIds));
				} else {
					resultQueue.Enqueue(new RequestResult(false, this.rowIds));
				}
			}
		}
	}
}
