/*
 * TestEventStore.cs
 * SnowplowTrackerTests.Storage
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
using NUnit.Framework;
using SnowplowTracker;
using SnowplowTracker.Storage;
using SnowplowTracker.Payloads;

namespace SnowplowTrackerTests.Storage {
	
	[TestFixture()]
	public class TestEventStore {
		
		[Test()]
		public void TestEventStoreInit () {
			EventStore es = new EventStore ();
			Assert.NotNull (es);
			Assert.IsTrue (es.IsDatabaseOpen ());
			es.Close ();
			Assert.IsFalse (es.IsDatabaseOpen ());
		}

		[Test()]
		public void TestEventStoreFunctions () {
			EventStore es = new EventStore ();
			Assert.AreEqual (0, es.GetEventCount ());

			TrackerPayload payload = new TrackerPayload ();
			payload.Add("hello", "world");

			Assert.IsTrue(es.AddEvent (payload));
			Assert.IsTrue(es.AddEvent (payload));
			Assert.IsTrue(es.AddEvent (payload));
			Assert.IsTrue(es.AddEvent (payload));
			Assert.AreEqual (4, es.GetEventCount ());
			Assert.IsTrue(es.DeleteEvent (4));
			Assert.AreEqual (3, es.GetEventCount ());
			Assert.IsTrue(es.DeleteEvents (new List<int>{3,2}));
			Assert.AreEqual (1, es.GetEventCount ());
			Assert.IsTrue(es.DeleteAllEvents ());
			Assert.AreEqual (0, es.GetEventCount ());

			Assert.IsTrue(es.AddEvent (payload));
			Assert.IsTrue(es.AddEvent (payload));
			Assert.IsTrue(es.AddEvent (payload));
			Assert.IsTrue(es.AddEvent (payload));
			Assert.AreEqual (4, es.GetEventCount ());
			Assert.AreEqual (4, es.GetAllEvents ().Count);
			Assert.AreEqual (2, es.GetDescEventRange (2).Count);
			Assert.AreEqual (payload.GetDictionary (), es.GetEvent (1).GetPayload ().GetDictionary ());

			Assert.IsTrue(es.DeleteAllEvents ());
		}
	}
}
