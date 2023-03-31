/*
 * AsyncEmitter.cs
 * SnowplowTracker.Emitters
 * 
 * Copyright (c) 2015-2023 Snowplow Analytics Ltd. All rights reserved.
 *
 * This program is licensed to you under the Apache License Version 2.0,
 * and you may not use this file except in compliance with the Apache License Version 2.0.
 * You may obtain a copy of the Apache License Version 2.0 at http://www.apache.org/licenses/LICENSE-2.0.
 *
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the Apache License Version 2.0 is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the Apache License Version 2.0 for the specific language governing permissions and limitations there under.
 */

using System.Collections.Generic;
using SnowplowTracker.Payloads;
using SnowplowTracker.Enums;
using SnowplowTracker.Requests;
using SnowplowTracker.Storage;
using System;
using UnityEngine;

namespace SnowplowTracker.Emitters
{
    public class SyncEmitter : AbstractEmitter {

        /// <summary>
        /// Initializes a new instance of the <see cref="SnowplowTracker.Emitters.SyncEmitter"/> class.
        /// </summary>
        /// <param name="endpoint">The collector endpoint uri</param>
        /// <param name="protocol">What protocol to send under</param>
        /// <param name="method">What method of sending to use</param>
        /// <param name="sendLimit">The amount of events to pull from the database per sending attempt</param>
        /// <param name="byteLimitGet">The byte limit for a GET request</param>
        /// <param name="byteLimitPost">The byte limit for a POST request</param>
        /// <param name="eventStore">Will default to new EventStore()</param>
        public SyncEmitter (string endpoint, HttpProtocol protocol = HttpProtocol.HTTP, HttpMethod method = HttpMethod.POST, 
		                    int sendLimit = 10, long byteLimitGet = 52000, long byteLimitPost = 52000, IStore eventStore = null) {
			Utils.CheckArgument(!string.IsNullOrEmpty (endpoint), "Endpoint cannot be null or empty.");
			this.endpoint = endpoint;
			this.collectorUri = MakeCollectorUri(endpoint, protocol, method);
			this.httpProtocol = protocol;
			this.httpMethod = method;
			this.sendLimit = sendLimit;
			this.byteLimitGet = byteLimitGet;
			this.byteLimitPost = byteLimitPost;
			this.eventStore = eventStore ?? (
                Application.platform == RuntimePlatform.tvOS ?
                (IStore) new InMemoryEventStore() :
                (IStore) new EventStore()
            );
		}
		
		/// <summary>
		/// Adds an event payload to the database.
		/// </summary>
		/// <param name="payload">Payload.</param>
		public override void Add(TrackerPayload payload) {
			eventStore.AddEvent(payload);

			if (eventStore.GetEventCount() >= sendLimit) {
				EmitLoop();
			}
		}
		
		/// <summary>
		/// Starts an Emit() Loop which will attempt to send the events in the database.
		/// </summary>
		public override void Start() {
			EmitLoop();
		}
		
		/// <summary>
		/// Does nothing in the SyncEmitter.
		/// </summary>
		public override void Stop() {
			return;
		}
		
		// --- Event Senders
		
		/// <summary>
		/// Will send events until either everything fails or the database is empty.
		/// </summary>
		private void EmitLoop() {
			Log.Debug("Emitter: EmitLoop starting...");
			while (eventStore.GetEventCount() != 0) {
				List<EventRow> events = eventStore.GetEvents(sendLimit);
				if (events.Count != 0) {
					Log.Debug("Emitter: Event count: " + events.Count);
					List<RequestResult> results = SendRequests(events);
					events = null;
					
					int success = 0;
					int failure = 0;
					
					List<Guid> eventsToDelete = new List<Guid>();
					
					foreach (RequestResult result in results) {
						if (result.success) {
							eventsToDelete.AddRange(result.rowIds);
							success += result.rowIds.Count;
						} else {
							failure += result.rowIds.Count;
						}
					}
					
					eventStore.DeleteEvents(eventsToDelete);
					
					Log.Debug("Emitter: event sending results.");
					Log.Debug(" + Successful: " + success);
					Log.Debug(" + Failure: " + failure);
					
					if (failure > 0 && success == 0) {
						Log.Error("Emitter: All events failed to send; exiting loop.");
						break;
					} else {
						Log.Debug("Emitter: All events sent successfully, checking for more...");
					}
				}
			}
			Log.Debug("Emitter: EmitLoop ended.");
		}
		
		// --- Helpers
		
		/// <summary>
		/// Gets whether the emitter is currently sending.
		/// </summary>
		/// <returns><c>true</c>, if emitter is sending, <c>false</c> otherwise.</returns>
		public override bool IsSending() {
			return false;
		}
	}
}
