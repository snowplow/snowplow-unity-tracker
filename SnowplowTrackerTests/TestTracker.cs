/*
 * TestTracker.cs
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
using SnowplowTrackerTests.TestHelpers;
using SnowplowTracker;
using SnowplowTracker.Emitters;
using SnowplowTracker.Enums;
using SnowplowTracker.Events;
using SnowplowTracker.Payloads;

namespace SnowplowTrackerTests {

	[TestFixture()]
	public class TestTracker {

		[Test()]
		public void TestTrackerInitMinimal () {
			Tracker t = new Tracker (new AsyncEmitter("acme.com", HttpProtocol.HTTP, HttpMethod.POST, 500, 52000L, 52000L), "aNamespace", "aAppId");

			Assert.NotNull (t);
			Assert.NotNull (t.GetEmitter());
			Assert.Null (t.GetSubject ());
			Assert.Null (t.GetSession ());
			Assert.AreEqual ("aNamespace", t.GetTrackerNamespace ());
			Assert.AreEqual ("aAppId", t.GetAppId ());
			Assert.AreEqual (true, t.GetBase64Encoded ());
			Assert.AreEqual (DevicePlatforms.Mobile.Value, t.GetPlatform ().Value);
		}

		[Test()]
		public void TestTrackerInitException () {
			Tracker t = null;
			try {
				t = new Tracker (null, "aNamespace", "aAppId");
			} catch (Exception e) {
				Assert.AreEqual("Emitter cannot be null.", e.Message);
			}
			Assert.Null (t);
		}

		[Test()]
		public void TestTrackerSetterFunctions () {
			Subject s1 = new Subject ();
			Session sess1 = new Session (null);
			IEmitter e1 = new AsyncEmitter("acme.com", HttpProtocol.HTTP, HttpMethod.POST, 500, 52000L, 52000L);
			Tracker t = new Tracker (e1, "aNamespace", "aAppId", s1, sess1);

			Assert.NotNull (t.GetEmitter());
			Assert.AreEqual ("http://acme.com/com.snowplowanalytics.snowplow/tp2", t.GetEmitter ().GetCollectorUri ());
			Assert.NotNull (t.GetSubject());
			Assert.NotNull (t.GetSession ());
			Assert.AreEqual ("aNamespace", t.GetTrackerNamespace ());
			Assert.AreEqual ("aAppId", t.GetAppId ());
			Assert.AreEqual (true, t.GetBase64Encoded ());
			Assert.AreEqual (DevicePlatforms.Mobile.Value, t.GetPlatform ().Value);

			IEmitter e2 = new AsyncEmitter("acme.com.au", HttpProtocol.HTTP, HttpMethod.POST, 500, 52000L, 52000L);

			t.SetEmitter(e2);
			Assert.AreEqual ("http://acme.com.au/com.snowplowanalytics.snowplow/tp2", t.GetEmitter ().GetCollectorUri ());
			t.SetSession (null);
			Assert.Null (t.GetSession());
			t.SetSubject (null);
			Assert.Null (t.GetSubject());
			t.SetTrackerNamespace("newNamespace");
			Assert.AreEqual ("newNamespace", t.GetTrackerNamespace ());
			t.SetAppId("newAppId");
			Assert.AreEqual ("newAppId", t.GetAppId ());
			t.SetBase64Encoded (false);
			Assert.AreEqual (false, t.GetBase64Encoded ());
			t.SetPlatform (DevicePlatforms.Desktop);
			Assert.AreEqual (DevicePlatforms.Desktop.Value, t.GetPlatform ().Value);
		}

		[Test()]
		public void TestTrackerSendEvent () {
			IEmitter e1 = new BaseEmitter();
			Tracker t = new Tracker (e1, "aNamespace", "aAppId");
			t.StartEventTracking ();
			
			t.Track (new PageView ().SetPageTitle ("title").SetPageUrl ("url").SetReferrer ("ref").SetTimestamp(1234567890).SetEventId("event-id-custom").Build ());
			t.Track (new PageView ().SetPageTitle ("title").SetPageUrl ("url").SetReferrer ("ref").SetTimestamp(1234567890).SetEventId("event-id-custom").Build ());

			BaseEmitter te1 = (BaseEmitter)t.GetEmitter ();
			Assert.AreEqual (2, te1.payloads.Count);

			foreach (TrackerPayload payload in te1.payloads) {
				Dictionary<string, object> dict = payload.GetDictionary();
				Assert.AreEqual(SnowplowTracker.Version.VERSION, dict[Constants.TRACKER_VERSION]);
				Assert.AreEqual("1234567890", dict[Constants.TIMESTAMP]);
				Assert.AreEqual("event-id-custom", dict[Constants.EID]);
				Assert.AreEqual("aNamespace", dict[Constants.NAMESPACE]);
				Assert.AreEqual("aAppId", dict[Constants.APP_ID]);
				Assert.AreEqual("mob", dict[Constants.PLATFORM]);
				Assert.AreEqual(Constants.EVENT_PAGE_VIEW, dict[Constants.EVENT]);
				Assert.AreEqual("title", dict[Constants.PAGE_TITLE]);
				Assert.AreEqual("url", dict[Constants.PAGE_URL]);
				Assert.AreEqual("ref", dict[Constants.PAGE_REFR]);
			}
		}
	}
}
