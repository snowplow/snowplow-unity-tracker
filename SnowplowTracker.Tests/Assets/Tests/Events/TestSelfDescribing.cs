/*
 * TestSelfDescribing.cs
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
using Newtonsoft.Json;
using NUnit.Framework;
using SnowplowTracker;
using SnowplowTracker.Events;
using SnowplowTracker.Payloads;

namespace SnowplowTrackerTests.Events
{

    [TestFixture]
    public class TestSelfDescribing
    {

        [Test]
        public void TestInitMinimal()
        {
            SelfDescribing se = new SelfDescribing("iglu:acme.com/demo/jsonschema/1-0-0", new Dictionary<string, object> { { "demo", "app" } }).Build();
            Assert.NotNull(se);
            Dictionary<string, object> payload = se.GetPayload().GetDictionary();
            Assert.AreEqual(4, payload.Count);
            Assert.AreEqual("ue", payload[Constants.EVENT]);
            CollectionAssert.AreEquivalent(JsonConvert.DeserializeObject<Dictionary<string, object>>("{\"schema\":\"iglu:com.snowplowanalytics.snowplow/unstruct_event/jsonschema/1-0-0\", \"data\":{\"schema\":\"iglu:acme.com/demo/jsonschema/1-0-0\", \"data\":{\"demo\":\"app\"}}}"), JsonConvert.DeserializeObject<Dictionary<string, object>>(payload[Constants.UNSTRUCTURED].ToString()));
        }

        [Test]
        public void TestInitException()
        {
            SelfDescribing se = null;
            try
            {
                se = new SelfDescribing(null).Build();
            }
            catch (Exception e)
            {
                Assert.AreEqual("EventData cannot be null.", e.Message);
            }
            Assert.Null(se);
        }
    }
}
