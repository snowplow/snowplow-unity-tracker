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

using System;
using System.Collections.Generic;
using System.Threading;
using SnowplowTracker.Payloads;
using SnowplowTracker.Enums;
using SnowplowTracker.Requests;
using SnowplowTracker.Storage;
using SnowplowTracker.Collections;

namespace SnowplowTracker.Emitters
{
    public class AsyncEmitter : AbstractEmitter {

		// Emitter loop variables
		private readonly object emitLock = new object ();
		private volatile bool sending = false;
		private Thread emitThread;

		// Emitter event store add variables
		private ConcurrentQueue<TrackerPayload> payloadQueue = new ConcurrentQueue<TrackerPayload>();
		private volatile bool consuming = false;
		private Thread payloadConsumer;

        /// <summary>
        /// Initializes a new instance of the <see cref="SnowplowTracker.Emitters.AsyncEmitter"/> class.
        /// </summary>
        /// <param name="endpoint">The collector endpoint uri</param>
        /// <param name="protocol">What protocol to send under</param>
        /// <param name="method">What method of sending to use</param>
        /// <param name="sendLimit">The amount of events to pull from the database per sending attempt</param>
        /// <param name="byteLimitGet">The byte limit for a GET request</param>
        /// <param name="byteLimitPost">The byte limit for a POST request</param>
        /// <param name="eventStore">Will default to new EventStore()</param>
        public AsyncEmitter(string endpoint, HttpProtocol protocol = HttpProtocol.HTTP, HttpMethod method = HttpMethod.POST,
                             int sendLimit = 500, long byteLimitGet = 52000, long byteLimitPost = 52000, IStore eventStore = null) {
			Utils.CheckArgument (!String.IsNullOrEmpty (endpoint), "Endpoint cannot be null or empty.");
			this.endpoint = endpoint;
			this.collectorUri = MakeCollectorUri(endpoint, protocol, method);
			this.httpProtocol = protocol;
			this.httpMethod = method;
			this.sendLimit = sendLimit;
			this.byteLimitGet = byteLimitGet;
			this.byteLimitPost = byteLimitPost;
			this.eventStore = eventStore ?? new EventStore();
		}

        /// <summary>
        /// Adds an event payload to the database.
        /// </summary>
        /// <param name="payload">Payload.</param>
        public override void Add(TrackerPayload payload) {
			payloadQueue.Enqueue (payload);
		}

		/// <summary>
		/// Starts the Emitter and the Event Consumer.
		/// - EmitLoop will send all events in the database and then
		///   wait for the event consumer to signal that there is work.
		/// - The event consumer waits for its queue to be signalled and
		///   will then add an event to the database; after which it 
		///   signals the emitloop of work to be done.
		/// </summary>
		public override void Start() {
			if (!sending) {
				// Start the emitter sending loop.
				emitThread = new Thread(new ThreadStart(EmitLoop));
				sending = true;
				emitThread.Start ();

				// Start the event consumer.
				payloadConsumer = new Thread(new ThreadStart(EventConsumer));
				consuming = true;
				payloadConsumer.Start();
			}
		}
		
		/// <summary>
		/// Stops the Emitter and the Event Consumer.
		/// </summary>
		public override void Stop() {
			if (sending) {
				// Stop the emitter sending loop.
				sending = false;
				lock (emitLock) {
					Monitor.Pulse(emitLock);
				}

				// Stops the event consumer.
				consuming = false;
				payloadQueue.Enqueue(null);
			}
		}

		// --- Consumers & Loopers

		/// <summary>
		/// Consumes events from the event queue and adds them to the database.
		/// </summary>
		private void EventConsumer() {
			Log.Debug("Emitter: Event consumer starting up");

			while (consuming) {
				TrackerPayload payload = payloadQueue.Dequeue ();

				// If the consumer was shutdown while waiting
				if (!consuming) {
					Log.Debug ("Emitter: Event consumer shutting down...");
					break;
				}

				// Signal emit loop
				lock (emitLock) {
					this.eventStore.AddEvent(payload);
					Monitor.Pulse(emitLock);
				}
			}
		}
	
		/// <summary>
		/// Performs and orchestrates all sending of events from the database
		/// and subsequent removal of events from the database.
		/// </summary>
		private void EmitLoop() {
			Log.Debug("Emitter: EmitLoop starting up");

			while (sending) {
				// Wait for something to be sent!
				lock (emitLock) {
					while (sending && eventStore.GetEventCount() == 0) {
						Monitor.Wait(emitLock);
					}
				}

				// If the emitter was shutdown while waiting
				if (!sending) {
					Log.Debug("Emitter: EmitLoop shutting down...");
					break;
				}
				List<EventRow> events = new List<EventRow>();

				if (emitLock != null) {
				    lock (emitLock) {
					    events = eventStore.GetEvents(sendLimit);
					    Monitor.Pulse(emitLock);
				    }
				}
				// Send events!
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
					if (emitLock != null) {
						lock (emitLock) {
					    	eventStore.DeleteEvents(eventsToDelete);
					    	Monitor.Pulse(emitLock);
						}
				    }
					
					Log.Debug("Emitter: event sending results.");
					Log.Debug(" + Successful: " + success);
					Log.Debug(" + Failure: " + failure);
					
					if (failure > 0 && success == 0) {
						Log.Error("Emitter: All events failed to send; pausing emitter for ten seconds...");
						Thread.Sleep(FAIL_INTERVAL);
					} else {
						Log.Debug("Emitter: All events sent successfully; waiting for more...");
					}
				}
			}
		}

		// --- Helpers

		/// <summary>
		/// Gets whether the emitter is currently sending.
		/// </summary>
		/// <returns><c>true</c>, if emitter is sending, <c>false</c> otherwise.</returns>
		public override bool IsSending() {
			return this.sending;
		}
	}
}
