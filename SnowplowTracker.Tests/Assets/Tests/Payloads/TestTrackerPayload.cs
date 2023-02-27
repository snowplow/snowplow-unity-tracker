/*
 * TestTrackerPayload.cs
 * SnowplowTrackerTests.Payloads
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
 * 
 * Authors: Joshua Beemster
 * Copyright: Copyright (c) 2015-2023 Snowplow Analytics Ltd
 * License: Apache License Version 2.0
 */

using System;
using System.Collections.Generic;
using NUnit.Framework;
using SnowplowTracker;
using SnowplowTracker.Payloads;

namespace SnowplowTrackerTests.Payloads {
	
	[TestFixture()]
	public class TestTrackerPayload {
		
		[Test()]
		public void TestInit () {
			TrackerPayload payload = new TrackerPayload ();
			Assert.NotNull (payload);
			Assert.AreEqual (0, payload.GetDictionary ().Count);
		}

		[Test()]
		public void TestAddFunction () {
			TrackerPayload payload = new TrackerPayload ();
			payload.Add ("demo", "application");

			Assert.AreEqual (1, payload.GetDictionary ().Count);
			Assert.AreEqual ("application", payload.GetDictionary ()["demo"]);

			payload.Add ("demo", null);
			Assert.AreEqual (1, payload.GetDictionary ().Count);
			payload.Add (null, "demo");
			Assert.AreEqual (1, payload.GetDictionary ().Count);
		}

		[Test()]
		public void TestAddDictFunction () {
			TrackerPayload payload = new TrackerPayload ();
			Dictionary<string, object> dict = new Dictionary<string, object> ();
			dict.Add ("hello", "world");
			dict.Add ("demo", 10);

			Assert.AreEqual (0, payload.GetDictionary ().Count);
			payload.AddDict (dict);
			Assert.AreEqual (1, payload.GetDictionary ().Count);
			Assert.AreEqual ("world", payload.GetDictionary ()["hello"]);
		}

		[Test()]
		public void TestAddJsonFunction () {
			TrackerPayload payload = new TrackerPayload ();
			Dictionary<string, object> dict = new Dictionary<string, object> ();
			dict.Add ("hello", "world");
			
			Assert.AreEqual (0, payload.GetDictionary ().Count);
			payload.AddJson (dict, false, "encoded", "not_encoded");
			Assert.AreEqual (1, payload.GetDictionary ().Count);
			Assert.AreEqual ("{\"hello\":\"world\"}", payload.GetDictionary ()["not_encoded"]);
			Assert.AreEqual (39, payload.GetByteSize ());
			Assert.AreEqual ("{\"not_encoded\":\"{\\\"hello\\\":\\\"world\\\"}\"}", payload.ToString ());
		}
	}
}
