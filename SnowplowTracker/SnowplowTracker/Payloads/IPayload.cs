/*
 * IPayload.cs
 * SnowplowTracker.Payloads
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
using System.Collections.Generic;

namespace SnowplowTracker.Payloads {
	public interface IPayload {

		/// <summary>
		/// Gets the dictionary within the Payload
		/// </summary>
		/// <returns>The payload</returns>
		Dictionary<string, object> GetDictionary ();

		/// <summary>
		/// Gets the byte size of the key-value pairs in the payload
		/// </summary>
		/// <returns>The total byte size</returns>
		long GetByteSize ();

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current <see cref="SnowplowTracker.Payload.IPayload"/>.
		/// </summary>
		/// <returns>A <see cref="System.String"/> that represents the current <see cref="SnowplowTracker.Payload.IPayload"/>.</returns>
		String ToString ();
	}
}
