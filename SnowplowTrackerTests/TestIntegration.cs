/*
 * TestIntegration.cs
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
using System.Threading;
using System.Collections.Generic;
using NUnit.Framework;
using SnowplowTracker;
using SnowplowTracker.Storage;
using SnowplowTracker.Events;
using SnowplowTracker.Payloads;
using SnowplowTracker.Payloads.Contexts;
using SnowplowTracker.Enums;
using SnowplowTracker.Emitters;

namespace SnowplowTrackerTests {
	
	[TestFixture()]
	public class TestIntegration {

		private string TEST_SERVER = "localhost:4545"; // Mountebank
		
		[Test()]
		public void TestGetRequests () {
			IEmitter emitter = new SyncEmitter (TEST_SERVER, HttpProtocol.HTTP, HttpMethod.GET);
			emitter.SetSendLimit (1);
			Tracker t1 = new Tracker (emitter, "TestNamespace", "TestAppId", GetSubject(), GetSession(), null, false);
			DoTest (t1);
		}

		[Test()]
		public void TestPostRequests () {
			IEmitter emitter = new SyncEmitter (TEST_SERVER, HttpProtocol.HTTP, HttpMethod.POST);
			emitter.SetSendLimit (1);
			Tracker t1 = new Tracker (emitter, "TestNamespace", "TestAppId", GetSubject(), GetSession(), null, false);
			DoTest (t1);
		}

		// --- Helpers

		private void DoTest(Tracker t1) {
			t1.StartEventTracking ();
			t1.Track (new PageView ().SetPageUrl("url").SetPageTitle("title").SetReferrer("refr").SetCustomContext(GetContextList()).Build());
			t1.Track (new ScreenView ().SetName ("name").SetId ("id").SetCustomContext (GetContextList ()).Build ());
			t1.Track (new Structured ().SetCategory ("category").SetAction ("action").SetLabel("label").SetProperty("property").SetValue(5.2).SetCustomContext (GetContextList ()).Build ());
			t1.Track (new Timing ().SetCategory ("category").SetVariable ("variable").SetTiming (5).SetLabel ("label").SetCustomContext (GetContextList ()).Build ());
			t1.Track (new EcommerceTransaction ().SetOrderId("orderId").SetTotalValue(10.22).SetAffiliation("affiliation").SetTaxValue(2.5).SetShipping(6.3).SetCity("London").SetState("Shoreditch").SetCountry("United Kingdom").SetCurrency("GBP").SetItems(GetItems()).SetCustomContext (GetContextList ()).Build ());
			t1.StopEventTracking ();
		}

		private SnowplowTracker.Subject GetSubject() {
			Subject s1 = new Subject ();
			s1.SetUserId("aUserId");
			s1.SetScreenResolution (1920, 1080);
			s1.SetViewPort (1080, 1920);
			s1.SetColorDepth (5);
			s1.SetTimezone("UTC");
			s1.SetLanguage("en-GB");
			s1.SetIpAddress("127.0.0.1");
			s1.SetUseragent("aUserAgent");
			s1.SetDomainUserId("domain-uid");
			s1.SetNetworkUserId("network-uid");
			return s1;
		}

		private SnowplowTracker.Session GetSession() {
			return new SnowplowTracker.Session(null);
		}

		private List<IContext> GetContextList() {
			List<IContext> contexts = new List<IContext> ();
			contexts.Add (new DesktopContext ().SetOsType ("OS-X").SetOsVersion ("10.10.5").SetOsServicePack ("Yosemite") .SetOsIs64Bit (true).SetDeviceManufacturer ("Apple").SetDeviceModel ("Macbook Pro").SetDeviceProcessorCount (4).Build ());
			contexts.Add (new MobileContext ().SetOsType ("iOS").SetOsVersion ("9.0").SetDeviceManufacturer ("Apple").SetDeviceModel ("iPhone 6S+").SetCarrier ("FREE").SetNetworkType (NetworkType.Mobile).SetNetworkTechnology ("LTE").SetOpenIdfa ("9c057ece-f4d1-4d26-8767-37768cd6736b").SetAppleIdfa ("9c057ece-f4d1-4d26-8767-37768cd6736b").SetAppleIdfv ("9c057ece-f4d1-4d26-8767-37768cd6736b").SetAndroidIdfa ("9c057ece-f4d1-4d26-8767-37768cd6736b").Build ());
			contexts.Add (new GeoLocationContext ().SetLatitude(123.564).SetLongitude(-12.6).SetLatitudeLongitudeAccuracy(5.6).SetAltitude(5.5).SetAltitudeAccuracy(2.1).SetBearing(3.2).SetSpeed(100.2).SetTimestamp(1234567890000).Build ());
			return contexts;
		}

		private List<EcommerceTransactionItem> GetItems() {
			return new List<EcommerceTransactionItem>{
				new EcommerceTransactionItem ()
					.SetSku ("sku")
					.SetPrice (10.2)
					.SetQuantity (1)
					.SetName ("name")
					.SetCategory ("category")
					.Build ()
			};
		}
	}
}
