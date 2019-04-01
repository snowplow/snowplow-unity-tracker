/*
 * TestEmitter.cs
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
using SnowplowTracker.Emitters;
using SnowplowTracker.Enums;

namespace SnowplowTrackerTests {
	
	[TestFixture()]
	public class TestEmitter {
		
		[Test()]
		public void TestAsyncEmitterInit () {
			IEmitter e1 = new AsyncEmitter ("acme.com", HttpProtocol.HTTP, HttpMethod.POST, 500, 52000L, 52000L);

			Assert.NotNull (e1);
			Assert.AreEqual ("http://acme.com/com.snowplowanalytics.snowplow/tp2", e1.GetCollectorUri ());
			Assert.AreEqual (HttpProtocol.HTTP, e1.GetHttpProtocol());
			Assert.AreEqual (HttpMethod.POST, e1.GetHttpMethod());
			Assert.AreEqual (500, e1.GetSendLimit ());
			Assert.AreEqual (52000, e1.GetByteLimitGet ());
			Assert.AreEqual (52000, e1.GetByteLimitPost ());
			Assert.NotNull (e1.GetEventStore ());
			Assert.False (e1.IsSending ());
		}

		[Test()]
		public void TestAsyncEmitterInitException () {
			IEmitter e1 = null;
			try {
				e1 = new AsyncEmitter (null, HttpProtocol.HTTP, HttpMethod.POST, 500, 52000L, 52000L);
			} catch (Exception e) {
				Assert.AreEqual("Endpoint cannot be null or empty.", e.Message);
			}
			Assert.IsNull (e1);
		}

		[Test()]
		public void TestAsyncEmitterSetFunctions () {
			IEmitter e1 = new AsyncEmitter ("acme.com", HttpProtocol.HTTP, HttpMethod.POST, 500, 52000L, 52000L);

			Assert.AreEqual ("http://acme.com/com.snowplowanalytics.snowplow/tp2", e1.GetCollectorUri ());
			e1.SetCollectorUri("acme.com.au");
			Assert.AreEqual ("http://acme.com.au/com.snowplowanalytics.snowplow/tp2", e1.GetCollectorUri ());
			e1.SetHttpProtocol (HttpProtocol.HTTPS);
			Assert.AreEqual ("https://acme.com.au/com.snowplowanalytics.snowplow/tp2", e1.GetCollectorUri ());
			e1.SetHttpMethod (HttpMethod.GET);
			Assert.AreEqual ("https://acme.com.au/i", e1.GetCollectorUri ());
			Assert.AreEqual (500, e1.GetSendLimit ());
			e1.SetSendLimit (1000);
			Assert.AreEqual (1000, e1.GetSendLimit ());
			Assert.AreEqual (52000, e1.GetByteLimitGet ());
			e1.SetByteLimitGet (100000);
			Assert.AreEqual (100000, e1.GetByteLimitGet ());
			Assert.AreEqual (52000, e1.GetByteLimitPost ());
			e1.SetByteLimitPost (100000);
			Assert.AreEqual (100000, e1.GetByteLimitPost ());
		}
	}
}
