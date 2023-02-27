/*
 * TestSelfDescribingJson.cs
 * SnowplowTrackerTests.Payloads
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

using System.Collections.Generic;
using Newtonsoft.Json;
using NUnit.Framework;
using SnowplowTracker.Payloads;

namespace SnowplowTrackerTests.Payloads
{

    [TestFixture()]
    public class TestSelfDescribingJson
    {

        [Test()]
        public void TestInitWithObject()
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict.Add("demo", 5);
            SelfDescribingJson sdj = new SelfDescribingJson("iglu:acme.com/demo_app/jsonschema/1-0-0", dict);

            Assert.NotNull(sdj);
            Assert.AreEqual(70, sdj.GetByteSize());
            CollectionAssert.AreEquivalent(JsonConvert.DeserializeObject<Dictionary<string, object>>("{\"schema\":\"iglu:acme.com/demo_app/jsonschema/1-0-0\", \"data\":{\"demo\":5}}"), JsonConvert.DeserializeObject<Dictionary<string, object>>(sdj.ToString()));

            sdj.SetSchema("iglu:acme.com/demo_app/jsonschema/1-0-1");

            CollectionAssert.AreEquivalent(JsonConvert.DeserializeObject<Dictionary<string, object>>("{\"schema\":\"iglu:acme.com/demo_app/jsonschema/1-0-1\", \"data\":{\"demo\":5}}"), JsonConvert.DeserializeObject<Dictionary<string, object>>(sdj.ToString()));

            dict.Add("app", "hello");
            sdj.SetData(dict);

            CollectionAssert.AreEquivalent(JsonConvert.DeserializeObject<Dictionary<string, object>>("{\"schema\":\"iglu:acme.com/demo_app/jsonschema/1-0-1\", \"data\":{\"demo\":5, \"app\":\"hello\"}}"), JsonConvert.DeserializeObject<Dictionary<string, object>>(sdj.ToString()));
        }

        [Test()]
        public void TestInitWithSdj()
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict.Add("demo", 5);
            SelfDescribingJson data = new SelfDescribingJson("iglu:acme.com/demo_app/jsonschema/1-0-0", dict);
            SelfDescribingJson sdj = new SelfDescribingJson("iglu:acme.com/demo/jsonschema/1-0-0", data);

            Assert.NotNull(sdj);
            Assert.AreEqual(126, sdj.GetByteSize());
            CollectionAssert.AreEquivalent(JsonConvert.DeserializeObject<Dictionary<string, object>>("{\"schema\":\"iglu:acme.com/demo/jsonschema/1-0-0\", \"data\":{\"schema\":\"iglu:acme.com/demo_app/jsonschema/1-0-0\", \"data\":{\"demo\":5}}}"), JsonConvert.DeserializeObject<Dictionary<string, object>>(sdj.ToString()));

            dict.Add("app", "hello");
            data.SetData(dict);
            sdj.SetData(data);

            CollectionAssert.AreEquivalent(JsonConvert.DeserializeObject<Dictionary<string, object>>("{\"schema\":\"iglu:acme.com/demo/jsonschema/1-0-0\", \"data\":{\"schema\":\"iglu:acme.com/demo_app/jsonschema/1-0-0\", \"data\":{\"demo\":5, \"app\":\"hello\"}}}"), JsonConvert.DeserializeObject<Dictionary<string, object>>(sdj.ToString()));
        }
    }
}
