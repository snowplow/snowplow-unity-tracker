/*
 * IEmitter.cs
 * SnowplowTracker.Emitters
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
using SnowplowTracker.Payloads;
using SnowplowTracker.Enums;
using SnowplowTracker.Storage;

namespace SnowplowTracker.Emitters {
	public interface IEmitter {

		/// <summary>
		/// Adds an event payload to the database.
		/// </summary>
		/// <param name="payload">Payload.</param>
		void Add(TrackerPayload payload);

		/// <summary>
		/// Starts the Emitter and the Event Consumer.
		/// - EmitLoop will send all events in the database and then
		///   wait for the event consumer to signal that there is work.
		/// - The event consumer waits for its queue to be signalled and
		///   will then add an event to the database; after which it 
		///   signals the emitloop of work to be done.
		/// </summary>
		void Start();

		/// <summary>
		/// Stops the Emitter and the Event Consumer.
		/// </summary>
		void Stop();

		/// <summary>
		/// Gets whether the emitter is currently sending.
		/// </summary>
		/// <returns><c>true</c>, if emitter is sending, <c>false</c> otherwise.</returns>
		bool IsSending();

		// --- Setters

		/// <summary>
		/// Sets the collector URI.
		/// </summary>
		/// <param name="endpoint">Endpoint.</param>
		void SetCollectorUri (string endpoint);
		
		/// <summary>
		/// Sets the http protocol.
		/// </summary>
		/// <param name="httpProtocol">Http protocol.</param>
		void SetHttpProtocol (HttpProtocol httpProtocol);
		
		/// <summary>
		/// Sets the http method.
		/// </summary>
		/// <param name="httpMethod">Http method.</param>
		void SetHttpMethod (HttpMethod httpMethod);
		
		/// <summary>
		/// Sets the send limit; this controls how many events are grabbed out of the database at anytime.
		/// </summary>
		/// <param name="sendLimit">Send limit.</param>
		void SetSendLimit (int sendLimit);
		
		/// <summary>
		/// Sets the byte limit for get requests.
		/// </summary>
		/// <param name="byteLimitGet">Byte limit get.</param>
		void SetByteLimitGet (long byteLimitGet);
		
		/// <summary>
		/// Sets the byte limit for post requests.
		/// </summary>
		/// <param name="byteLimitPost">Byte limit post.</param>
		void SetByteLimitPost (long byteLimitPost);
		
		// --- Getters
		
		/// <summary>
		/// Gets the collector URI.
		/// </summary>
		/// <returns>The collector URI.</returns>
		string GetCollectorUri ();
		
		/// <summary>
		/// Gets the http protocol.
		/// </summary>
		/// <returns>The http protocol.</returns>
		HttpProtocol GetHttpProtocol ();
		
		/// <summary>
		/// Gets the http method.
		/// </summary>
		/// <returns>The http method.</returns>
		HttpMethod GetHttpMethod ();
		
		/// <summary>
		/// Gets the send limit.
		/// </summary>
		/// <returns>The send limit.</returns>
		int GetSendLimit ();
		
		/// <summary>
		/// Gets the byte limit for get requests.
		/// </summary>
		/// <returns>The byte limit get.</returns>
		long GetByteLimitGet ();
		
		/// <summary>
		/// Gets the byte limit for post requests.
		/// </summary>
		/// <returns>The byte limit post.</returns>
		long GetByteLimitPost ();

		/// <summary>
		/// Gets the event store.
		/// </summary>
		/// <returns>The event store.</returns>
		EventStore GetEventStore();
	}
}
