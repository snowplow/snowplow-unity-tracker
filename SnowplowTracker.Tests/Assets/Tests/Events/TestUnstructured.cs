/*
 * TestUnstructured.cs
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
using UnityJSON;

namespace SnowplowTrackerTests.Events
{

    [TestFixture]
    public class TestUnstructured
    {

        [Test]
        public void TestInitMinimal()
        {
            Unstructured ue = new Unstructured().SetEventData(new SelfDescribingJson("iglu:acme.com/demo/jsonschema/1-0-0", new Dictionary<string, object> { { "demo", "app" } })).Build();
            Assert.NotNull(ue);
            Dictionary<string, object> payload = ue.GetPayload().GetDictionary();
            Assert.AreEqual(4, payload.Count);
            Assert.AreEqual("ue", payload[Constants.EVENT]);
            CollectionAssert.AreEquivalent(JSON.Deserialize<Dictionary<string, object>>("{\"data\":{\"data\":{\"demo\":\"app\"}, \"schema\":\"iglu:acme.com/demo/jsonschema/1-0-0\"}, \"schema\":\"iglu:com.snowplowanalytics.snowplow/unstruct_event/jsonschema/1-0-0\"}"), JSON.Deserialize<Dictionary<string, object>>(payload[Constants.UNSTRUCTURED].ToString()));
        }

        [Test]
        public void TestInitException()
        {
            Unstructured ue = null;
            try
            {
                ue = new Unstructured().Build();
            }
            catch (Exception e)
            {
                Assert.AreEqual("EventData cannot be null.", e.Message);
            }
            Assert.Null(ue);
        }
    }
}
