/*
 * TestScreenView.cs
 * SnowplowTrackerTests.Events
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
using SnowplowTracker.Events;
using SnowplowTracker.Payloads;

namespace SnowplowTrackerTests.Events {
	
	[TestFixture()]
	public class TestScreenView {
		
		[Test()]
		public void TestInitMinimal () {
			ScreenView sv = new ScreenView ().SetId ("id").Build ();
			Assert.NotNull (sv);
			Dictionary<string, object> payload = (Dictionary<string, object>)sv.GetPayload ().GetDictionary()["data"];
			Assert.AreEqual (1, payload.Count);
			Assert.AreEqual ("id", payload [Constants.SV_ID]);
			Assert.AreEqual ("iglu:com.snowplowanalytics.snowplow/screen_view/jsonschema/1-0-0", (string)sv.GetPayload ().GetDictionary()["schema"]);
		}
		
		[Test()]
		public void TestInitFull () {
			ScreenView sv = new ScreenView ().SetName("name").SetId ("id").Build ();
			Dictionary<string, object> payload = (Dictionary<string, object>)sv.GetPayload ().GetDictionary()["data"];
			Assert.AreEqual (2, payload.Count);
			Assert.AreEqual ("id", payload [Constants.SV_ID]);
			Assert.AreEqual ("name", payload [Constants.SV_NAME]);
			Assert.AreEqual ("iglu:com.snowplowanalytics.snowplow/screen_view/jsonschema/1-0-0", (string)sv.GetPayload ().GetDictionary()["schema"]);
		}
		
		[Test()]
		public void TestInitException () {
			ScreenView sv = null;
			try {
				sv = new ScreenView ().Build ();
			} catch (Exception e) {
				Assert.AreEqual("Both Name and Id cannot be null or empty.", e.Message);
			}
			Assert.IsNull (sv);
		}
	}
}
