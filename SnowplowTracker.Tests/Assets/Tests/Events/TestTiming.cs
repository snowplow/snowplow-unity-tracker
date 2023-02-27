/*
 * TestTiming.cs
 * SnowplowTrackerTests.Events
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
using SnowplowTracker.Events;
using SnowplowTracker.Payloads;

namespace SnowplowTrackerTests.Events {
	
	[TestFixture()]
	public class TestTiming {
		
		[Test()]
		public void TestInitMinimal () {
			Timing timing = new Timing ().SetCategory("category").SetVariable("variable").SetTiming(5).Build ();
			Assert.NotNull (timing);
			Dictionary<string, object> payload = (Dictionary<string, object>)timing.GetPayload ().GetDictionary()["data"];
			Assert.AreEqual (3, payload.Count);
			Assert.AreEqual ("category", payload [Constants.UT_CATEGORY]);
			Assert.AreEqual ("variable", payload [Constants.UT_VARIABLE]);
			Assert.AreEqual (5, payload [Constants.UT_TIMING]);
			Assert.AreEqual ("iglu:com.snowplowanalytics.snowplow/timing/jsonschema/1-0-0", (string)timing.GetPayload ().GetDictionary()["schema"]);
		}
		
		[Test()]
		public void TestInitFull () {
			Timing timing = new Timing ().SetCategory("category").SetVariable("variable").SetTiming(5).SetLabel("label").Build ();
			Assert.NotNull (timing);
			Dictionary<string, object> payload = (Dictionary<string, object>)timing.GetPayload ().GetDictionary()["data"];
			Assert.AreEqual (4, payload.Count);
			Assert.AreEqual ("category", payload [Constants.UT_CATEGORY]);
			Assert.AreEqual ("variable", payload [Constants.UT_VARIABLE]);
			Assert.AreEqual (5, payload [Constants.UT_TIMING]);
			Assert.AreEqual ("label", payload [Constants.UT_LABEL]);
			Assert.AreEqual ("iglu:com.snowplowanalytics.snowplow/timing/jsonschema/1-0-0", (string)timing.GetPayload ().GetDictionary()["schema"]);
		}
		
		[Test()]
		public void TestInitException () {
			Timing timing = null;
			try {
				timing = new Timing ().Build ();
			} catch (Exception e) {
				Assert.AreEqual("Category cannot be null or empty.", e.Message);
			}
			Assert.IsNull (timing);

			try {
				timing = new Timing ().SetCategory("category").Build ();
			} catch (Exception e) {
				Assert.AreEqual("Variable cannot be null or empty.", e.Message);
			}
			Assert.IsNull (timing);

			try {
				timing = new Timing ().SetCategory("category").SetVariable("variable").Build ();
			} catch (Exception e) {
				Assert.AreEqual("Timing cannot be null.", e.Message);
			}
			Assert.IsNull (timing);
		}
	}
}
