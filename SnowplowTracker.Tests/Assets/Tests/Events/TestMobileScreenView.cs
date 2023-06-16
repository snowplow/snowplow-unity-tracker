/*
 * TestMobileScreenView.cs
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
	public class TestMobileScreenView {
		
		[Test()]
		public void TestInitMinimal () {
			MobileScreenView sv = new MobileScreenView ("id", "name").Build ();
			Assert.NotNull (sv);
			Dictionary<string, object> payload = (Dictionary<string, object>)sv.GetPayload ().GetDictionary()["data"];
			Assert.AreEqual (2, payload.Count);
			Assert.AreEqual ("id", payload [Constants.SV_ID]);
			Assert.AreEqual ("name", payload [Constants.SV_NAME]);
			Assert.AreEqual ("iglu:com.snowplowanalytics.mobile/screen_view/jsonschema/1-0-0", (string)sv.GetPayload ().GetDictionary()["schema"]);
		}
		
		[Test()]
		public void TestInitFull () {
			MobileScreenView sv = new MobileScreenView ("id", "name")
				.SetType ("type")
				.SetPreviousName ("previousName")
				.SetPreviousId ("previousId")
				.SetPreviousType ("previousType")
				.SetTransitionType ("transitionType")
				.Build ();
			Dictionary<string, object> payload = (Dictionary<string, object>)sv.GetPayload ().GetDictionary()["data"];
			Assert.AreEqual (7, payload.Count);
			Assert.AreEqual ("id", payload [Constants.SV_ID]);
			Assert.AreEqual ("name", payload [Constants.SV_NAME]);
			Assert.AreEqual ("type", payload [Constants.SV_TYPE]);
			Assert.AreEqual ("previousId", payload [Constants.SV_PREVIOUS_ID]);
			Assert.AreEqual ("previousName", payload [Constants.SV_PREVIOUS_NAME]);
			Assert.AreEqual ("previousType", payload [Constants.SV_PREVIOUS_TYPE]);
			Assert.AreEqual ("transitionType", payload [Constants.SV_TRANSITION_TYPE]);
			Assert.AreEqual ("iglu:com.snowplowanalytics.mobile/screen_view/jsonschema/1-0-0", (string)sv.GetPayload ().GetDictionary()["schema"]);
		}
		
		[Test()]
		public void TestInitException () {
			MobileScreenView sv = null;
			try {
				sv = new MobileScreenView (null, null).Build ();
			} catch (Exception e) {
				Assert.AreEqual("Name cannot be null or empty.", e.Message);
			}
			Assert.IsNull (sv);
		}
	}
}
