/*
 * TestSession.cs
 * SnowplowTrackerTests
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
using SnowplowTracker.Payloads.Contexts;

namespace SnowplowTrackerTests {
	
	[TestFixture()]
	public class TestSession {
		
		[Test()]
		public void TestSessionInit () {
			Session s1 = new Session (null);
			Assert.NotNull (s1);
			SessionContext c1 = s1.GetSessionContext ("first-event-id-0000");
			Dictionary<string, object> data = c1.GetData ();
			Assert.AreEqual ("first-event-id-0000", data[Constants.SESSION_FIRST_ID]);
		}

		[Test()]
		public void TestSessionSetFunctions () {
			Session s1 = new Session (null);
			SessionContext c1 = s1.GetSessionContext ("first-event-id-0000");
			Dictionary<string, object> data = c1.GetData ();

			Assert.AreEqual ("first-event-id-0000", data[Constants.SESSION_FIRST_ID]);
			Assert.AreEqual ("SQLITE", data[Constants.SESSION_STORAGE]);
			Assert.True (data.ContainsKey(Constants.SESSION_PREVIOUS_ID));
			Assert.True (data.ContainsKey(Constants.SESSION_INDEX));
			Assert.True (data.ContainsKey(Constants.SESSION_ID));
			Assert.True (data.ContainsKey(Constants.SESSION_USER_ID));

			Assert.False (s1.GetBackground ());
			Assert.AreEqual (600, s1.GetForegroundTimeout ());
			Assert.AreEqual (300, s1.GetBackgroundTimeout ());
			Assert.AreEqual (15, s1.GetCheckInterval ());
			
			s1.SetBackground (true);
			s1.SetForegroundTimeoutSeconds (300);
			s1.SetBackgroundTimeoutSeconds (150);
			s1.SetCheckIntervalSeconds (30);
			
			Assert.True (s1.GetBackground ());
			Assert.AreEqual (300, s1.GetForegroundTimeout ());
			Assert.AreEqual (150, s1.GetBackgroundTimeout ());
			Assert.AreEqual (30, s1.GetCheckInterval ());
		}
	}
}
