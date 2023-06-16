/*
 * Unstructured.cs
 * SnowplowTracker.Events
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
using SnowplowTracker.Payloads;

namespace SnowplowTracker.Events
{
    /// <summary>
	/// A self-describing event.
	/// </summary>
    public class SelfDescribing : AbstractEvent<SelfDescribing> {
		
		private SelfDescribingJson eventData;
		private bool base64Encode = false;

		/// <summary>
		/// Create a self-describing event using a SelfDescribingJson object.
		/// </summary>
		/// <param name="eventData">A SelfDescribingJson instance with the schema and data.</param>
		public SelfDescribing(SelfDescribingJson eventData)
		{
			SetEventData(eventData);
		}

		/// <summary>
		/// Create a self-describing event with a given event schema and data.
		/// </summary>
		/// <param name="schema">A schema URI against which the data is validated.</param>
		/// <param name="data">A data object containing the event properties.</param>
        public SelfDescribing(String schema, Object data)
        {
            SetEventData(new SelfDescribingJson(schema, data));
        }

        /// <summary>
        /// Sets the event data.
        /// </summary>
        /// <returns>The event.</returns>
        /// <param name="eventData">Event data.</param>
        public SelfDescribing SetEventData(SelfDescribingJson eventData) {
			this.eventData = eventData;
			return this;
		}
		
		public override SelfDescribing Self() {
			return this;
		}
		
		public override SelfDescribing Build() {
			Utils.CheckArgument (eventData != null, "EventData cannot be null.");
			return this;
		}

		public void SetBase64Encode(bool base64Encode) {
			this.base64Encode = base64Encode;
		}
		
		// --- Interface Methods

		/// <summary>
		/// Gets the event payload.
		/// </summary>
		/// <returns>The event payload</returns>
		public override IPayload GetPayload() {
			TrackerPayload payload = new TrackerPayload();
			payload.Add (Constants.EVENT, Constants.EVENT_UNSTRUCTURED);
			SelfDescribingJson envelope = new SelfDescribingJson(Constants.SCHEMA_UNSTRUCT_EVENT, this.eventData.GetDictionary());
			payload.AddJson(envelope.GetDictionary(), this.base64Encode, Constants.UNSTRUCTURED_ENCODED, Constants.UNSTRUCTURED);
			return AddDefaultPairs (payload);
		}
	}
}
