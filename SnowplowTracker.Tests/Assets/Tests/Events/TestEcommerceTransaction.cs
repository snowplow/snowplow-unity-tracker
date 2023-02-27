/*
 * TestEcommerceTransaction.cs
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
	public class TestEcommerceTransaction {
		
		[Test()]
		public void TestInitMinimal () {
			EcommerceTransaction et = new EcommerceTransaction ().SetOrderId("orderId").SetItems(GetItem()).SetTotalValue(10.20).Build ();
			Assert.NotNull (et);

			Dictionary<string, object> payload = et.GetPayload ().GetDictionary();
			Assert.AreEqual (5, payload.Count);
			Assert.AreEqual ("tr", payload [Constants.EVENT]);
			Assert.AreEqual ("orderId", payload [Constants.TR_ID]);
			Assert.AreEqual ("10.20", payload [Constants.TR_TOTAL]);
		}
		
		[Test()]
		public void TestInitFull () {
			EcommerceTransaction et = new EcommerceTransaction ()
				.SetOrderId("orderId")
				.SetTotalValue(10.22)
				.SetAffiliation("affiliation")
				.SetTaxValue(2.5)
				.SetShipping(6.3)
				.SetCity("London")
				.SetState("Shoreditch")
				.SetCountry("United Kingdom")
				.SetCurrency("GBP")
				.SetItems(GetItem())
				.Build ();
			Assert.NotNull (et);
			
			Dictionary<string, object> payload = et.GetPayload ().GetDictionary();
			Assert.AreEqual (12, payload.Count);
			Assert.AreEqual ("tr", payload [Constants.EVENT]);
			Assert.AreEqual ("orderId", payload [Constants.TR_ID]);
			Assert.AreEqual ("10.22", payload [Constants.TR_TOTAL]);
			Assert.AreEqual ("affiliation", payload [Constants.TR_AFFILIATION]);
			Assert.AreEqual ("2.50", payload [Constants.TR_TAX]);
			Assert.AreEqual ("6.30", payload [Constants.TR_SHIPPING]);
			Assert.AreEqual ("London", payload [Constants.TR_CITY]);
			Assert.AreEqual ("Shoreditch", payload [Constants.TR_STATE]);
			Assert.AreEqual ("United Kingdom", payload [Constants.TR_COUNTRY]);
			Assert.AreEqual ("GBP", payload [Constants.TR_CURRENCY]);
		}
		
		[Test()]
		public void TestInitException () {
			EcommerceTransaction et = null;
			try {
				et = new EcommerceTransaction ().Build ();
			} catch (Exception e) {
				Assert.AreEqual("OrderId cannot be null or empty.", e.Message);
			}
			Assert.IsNull (et);

			try {
				et = new EcommerceTransaction ().SetOrderId("orderId").Build ();
			} catch (Exception e) {
				Assert.AreEqual("Items cannot be null.", e.Message);
			}
			Assert.IsNull (et);

			try {
				et = new EcommerceTransaction ().SetOrderId("orderId").SetItems(GetItem()).Build ();
			} catch (Exception e) {
				Assert.AreEqual("TotalValue cannot be null.", e.Message);
			}
			Assert.IsNull (et);
		}

		private List<EcommerceTransactionItem> GetItem() {
			return new List<EcommerceTransactionItem>{
				new EcommerceTransactionItem()
					.SetSku("sku")
					.SetPrice(10.20)
					.SetQuantity(1)
					.Build()
			};
		}
	}
}
