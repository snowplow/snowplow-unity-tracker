/*
 * TestDesktopContext.cs
 * SnowplowTrackerTests.Payloads.Contexts
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
using SnowplowTracker.Payloads;
using SnowplowTracker.Payloads.Contexts;

namespace SnowplowTrackerTests.Payloads.Contexts {
	
	[TestFixture()]
	public class TestDesktopContext {
		
		[Test()]
		public void TestInitMinimal () {
			DesktopContext context = new DesktopContext ().SetOsType("OS-X").SetOsVersion("10.10.5").Build ();
			Assert.NotNull (context);
			
			Dictionary<string, object> dict = context.GetData ();
			Assert.AreEqual (2, dict.Count);
			Assert.AreEqual ("OS-X", dict [Constants.PLAT_OS_TYPE]);
			Assert.AreEqual ("10.10.5", dict [Constants.PLAT_OS_VERSION]);

			Assert.AreEqual ("iglu:com.snowplowanalytics.snowplow/desktop_context/jsonschema/1-0-0", context.GetSchema());
			Assert.AreEqual ("{\"data\":{\"osVersion\":\"10.10.5\", \"osType\":\"OS-X\"}, \"schema\":\"iglu:com.snowplowanalytics.snowplow/desktop_context/jsonschema/1-0-0\"}", context.GetJson().ToString());
		}

		[Test()]
		public void TestInitFull () {
			DesktopContext context = new DesktopContext ()
				.SetOsType("OS-X")
				.SetOsVersion("10.10.5")
				.SetOsServicePack("Yosemite")
				.SetOsIs64Bit(true)
				.SetDeviceManufacturer("Apple")
				.SetDeviceModel("Macbook Pro")
				.SetDeviceProcessorCount(4)
				.Build ();
			Assert.NotNull (context);
			
			Dictionary<string, object> dict = context.GetData ();
			Assert.AreEqual (7, dict.Count);
			Assert.AreEqual ("OS-X", dict [Constants.PLAT_OS_TYPE]);
			Assert.AreEqual ("10.10.5", dict [Constants.PLAT_OS_VERSION]);
			Assert.AreEqual ("Yosemite", dict [Constants.DESKTOP_SERVICE_PACK]);
			Assert.AreEqual (true, dict [Constants.DESKTOP_IS_64_BIT]);
			Assert.AreEqual ("Apple", dict [Constants.PLAT_DEVICE_MANU]);
			Assert.AreEqual ("Macbook Pro", dict [Constants.PLAT_DEVICE_MODEL]);
			Assert.AreEqual (4, dict [Constants.DESKTOP_PROC_COUNT]);

			Assert.AreEqual ("iglu:com.snowplowanalytics.snowplow/desktop_context/jsonschema/1-0-0", context.GetSchema());
			Assert.AreEqual ("{\"data\":{\"osVersion\":\"10.10.5\", \"osServicePack\":\"Yosemite\", \"deviceManufacturer\":\"Apple\", \"deviceProcessorCount\":4, \"osIs64Bit\":true, \"deviceModel\":\"Macbook Pro\", \"osType\":\"OS-X\"}, \"schema\":\"iglu:com.snowplowanalytics.snowplow/desktop_context/jsonschema/1-0-0\"}", context.GetJson().ToString());
		}

		[Test()]
		public void TestInitExceptions () {
			DesktopContext context = null;
			try {
				context = new DesktopContext().Build();
			} catch (Exception e) {
				Assert.AreEqual("Desktop Context requires 'osType'.", e.Message);
			}
			Assert.IsNull (context);

			try {
				context = new DesktopContext().SetOsType("OS-X").Build();
			} catch (Exception e) {
				Assert.AreEqual("Desktop Context requires 'osVersion'.", e.Message);
			}
			Assert.IsNull (context);
		}
	}
}
