/*
 * TestPageView.cs
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
using SnowplowTracker.Payloads;
using SnowplowTracker.Events;

namespace SnowplowTrackerTests.Events {
	
	[TestFixture()]
	public class TestPageView {
		
		[Test()]
		public void TestInitMinimal () {
			PageView pv = new PageView ().SetPageUrl ("url").Build ();
			Assert.NotNull (pv);

			Dictionary<string, object> payload = pv.GetPayload ().GetDictionary();
			Assert.AreEqual (4, payload.Count);
			Assert.AreEqual ("pv", payload [Constants.EVENT]);
			Assert.AreEqual ("url", payload [Constants.PAGE_URL]);
		}

		[Test()]
		public void TestInitFull () {
			PageView pv = new PageView ().SetPageUrl ("url").SetPageTitle("title").SetReferrer("ref").Build ();
			Assert.NotNull (pv);
			
			Dictionary<string, object> payload = pv.GetPayload ().GetDictionary();
			Assert.AreEqual (6, payload.Count);
			Assert.AreEqual ("pv", payload [Constants.EVENT]);
			Assert.AreEqual ("url", payload [Constants.PAGE_URL]);
			Assert.AreEqual ("title", payload [Constants.PAGE_TITLE]);
			Assert.AreEqual ("ref", payload [Constants.PAGE_REFR]);
		}

		[Test()]
		public void TestInitException () {
			PageView pv = null;
			try {
				pv = new PageView ().Build ();
			} catch (Exception e) {
				Assert.AreEqual("PageUrl cannot be null or empty.", e.Message);
			}
			Assert.Null (pv);
		}
	}
}
