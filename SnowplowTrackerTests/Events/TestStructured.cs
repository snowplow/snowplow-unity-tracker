/*
 * TestStructured.cs
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
	public class TestStructured {
		
		[Test()]
		public void TestInitMinimal () {
			Structured se = new Structured ().SetCategory ("category").SetAction ("action").Build ();
			Assert.NotNull (se);
			
			Dictionary<string, object> payload = se.GetPayload ().GetDictionary();
			Assert.AreEqual (5, payload.Count);
			Assert.AreEqual ("se", payload [Constants.EVENT]);
			Assert.AreEqual ("category", payload [Constants.SE_CATEGORY]);
			Assert.AreEqual ("action", payload [Constants.SE_ACTION]);
		}
		
		[Test()]
		public void TestInitFull () {
			Structured se = new Structured ().SetCategory ("category").SetAction ("action").SetLabel("label").SetProperty("property").SetValue(5.2).Build ();
			Assert.NotNull (se);
			
			Dictionary<string, object> payload = se.GetPayload ().GetDictionary();
			Assert.AreEqual (8, payload.Count);
			Assert.AreEqual ("se", payload [Constants.EVENT]);
			Assert.AreEqual ("category", payload [Constants.SE_CATEGORY]);
			Assert.AreEqual ("action", payload [Constants.SE_ACTION]);
			Assert.AreEqual ("label", payload [Constants.SE_LABEL]);
			Assert.AreEqual ("property", payload [Constants.SE_PROPERTY]);
			Assert.AreEqual ("5.2", payload [Constants.SE_VALUE]);
		}
		
		[Test()]
		public void TestInitException () {
			Structured se = null;
			try {
				se = new Structured ().Build ();
			} catch (Exception e) {
				Assert.AreEqual("Category cannot be null or empty.", e.Message);
			}
			Assert.Null (se);

			try {
				se = new Structured ().SetCategory ("category").Build ();
			} catch (Exception e) {
				Assert.AreEqual("Action cannot be null or empty.", e.Message);
			}
			Assert.Null (se);
		}
	}
}
