/*
 * TestEcommerceTransactionItem.cs
 * SnowplowTrackerTests.Events
 * 
 * Copyright (c) 2015-2022 Snowplow Analytics Ltd. All rights reserved.
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
 * Copyright: Copyright (c) 2015-2022 Snowplow Analytics Ltd
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
	public class TestEcommerceTransactionItem {
		
		[Test()]
		public void TestInitMinimal () {
			EcommerceTransactionItem eti = new EcommerceTransactionItem ().SetSku ("sku").SetPrice (10.2).SetQuantity (1).Build ();
			Assert.NotNull (eti);
			
			Dictionary<string, object> payload = eti.GetPayload ().GetDictionary();
			Assert.AreEqual (6, payload.Count);
			Assert.AreEqual ("ti", payload [Constants.EVENT]);
			Assert.AreEqual ("sku", payload [Constants.TI_ITEM_SKU]);
			Assert.AreEqual ("10.20", payload [Constants.TI_ITEM_PRICE]);
			Assert.AreEqual ("1", payload [Constants.TI_ITEM_QUANTITY]);
		}
		
		[Test()]
		public void TestInitFull () {
			EcommerceTransactionItem eti = new EcommerceTransactionItem ()
				.SetSku ("sku")
				.SetPrice (10.2)
				.SetQuantity (1)
				.SetName ("name")
				.SetCategory ("category")
				.Build ();
			Assert.NotNull (eti);
			
			Dictionary<string, object> payload = eti.GetPayload ().GetDictionary();
			Assert.AreEqual (8, payload.Count);
			Assert.AreEqual ("ti", payload [Constants.EVENT]);
			Assert.AreEqual ("sku", payload [Constants.TI_ITEM_SKU]);
			Assert.AreEqual ("10.20", payload [Constants.TI_ITEM_PRICE]);
			Assert.AreEqual ("1", payload [Constants.TI_ITEM_QUANTITY]);
			Assert.AreEqual ("name", payload [Constants.TI_ITEM_NAME]);
			Assert.AreEqual ("category", payload [Constants.TI_ITEM_CATEGORY]);
		}
		
		[Test()]
		public void TestInitException () {
			EcommerceTransactionItem eti = null;
			try {
				eti = new EcommerceTransactionItem ().Build();
			} catch (Exception e) {
				Assert.AreEqual("Sku cannot be null or empty.", e.Message);
			}
			Assert.IsNull (eti);

			try {
				eti = new EcommerceTransactionItem ().SetSku("sku").Build();
			} catch (Exception e) {
				Assert.AreEqual("Price cannot be null.", e.Message);
			}
			Assert.IsNull (eti);

			try {
				eti = new EcommerceTransactionItem ().SetSku("sku").SetPrice(10.78).Build();
			} catch (Exception e) {
				Assert.AreEqual("Quantity cannot be null.", e.Message);
			}
			Assert.IsNull (eti);
		}
	}
}
