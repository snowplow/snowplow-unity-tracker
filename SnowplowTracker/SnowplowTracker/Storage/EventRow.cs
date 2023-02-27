/*
 * EventRow.cs
 * SnowplowTracker.Storage
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

namespace SnowplowTracker.Storage
{
    public class EventRow {

		private Guid rowId;
		private TrackerPayload payload;

		/// <summary>
		/// Initializes a new instance of the <see cref="SnowplowTracker.Storage.EventRow"/> class.
		/// </summary>
		/// <param name="rowId">The row number of the event in the database</param>
		/// <param name="payload">The event payload that needs to be sent</param>
		public EventRow (Guid rowId, TrackerPayload payload) {
			this.rowId = rowId;
			this.payload = payload;
		}

		/// <summary>
		/// Gets the row identifier.
		/// </summary>
		/// <returns>The row identifier.</returns>
		public Guid GetRowId() {
			return rowId;
		}

		/// <summary>
		/// Gets the payload.
		/// </summary>
		/// <returns>The payload.</returns>
		public TrackerPayload GetPayload() {
			return payload;
		}
	}
}
