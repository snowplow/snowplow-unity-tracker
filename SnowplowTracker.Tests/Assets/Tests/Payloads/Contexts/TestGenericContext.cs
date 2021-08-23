/*
 * TestGenericContext.cs
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
using SnowplowTracker.Payloads.Contexts;
using Newtonsoft.Json;

namespace SnowplowTrackerTests.Payloads.Contexts
{

    [TestFixture()]
    public class TestGenericContext
    {

        [Test()]
        public void TestInitMinimal()
        {
            GenericContext context = new GenericContext().SetSchema("iglu:com.acme/custom_events/jsonchema/1-0-0").Build();
            Assert.NotNull(context);

            Assert.AreEqual(0, context.GetData().Count);
            Assert.AreEqual("iglu:com.acme/custom_events/jsonchema/1-0-0", context.GetSchema());
            CollectionAssert.AreEquivalent(JsonConvert.DeserializeObject<Dictionary<string, object>>("{\"data\":{}, \"schema\":\"iglu:com.acme/custom_events/jsonchema/1-0-0\"}"), JsonConvert.DeserializeObject<Dictionary<string, object>>(context.GetJson().ToString()));
        }

        [Test()]
        public void TestInitFull()
        {
            GenericContext context = new GenericContext()
                .SetSchema("iglu:com.acme/custom_events/jsonchema/1-0-0")
                .Add("demo", "context")
                .Build();
            Assert.NotNull(context);

            Assert.AreEqual(1, context.GetData().Count);
            Assert.AreEqual("iglu:com.acme/custom_events/jsonchema/1-0-0", context.GetSchema());
            CollectionAssert.AreEquivalent(JsonConvert.DeserializeObject<Dictionary<string, object>>("{\"data\":{\"demo\":\"context\"}, \"schema\":\"iglu:com.acme/custom_events/jsonchema/1-0-0\"}"), JsonConvert.DeserializeObject<Dictionary<string, object>>(context.GetJson().ToString()));
        }

        [Test()]
        public void TestInitExceptions()
        {
            GenericContext context = null;
            try
            {
                context = new GenericContext().Build();
            }
            catch (Exception e)
            {
                Assert.AreEqual("Schema cannot be null or empty.", e.Message);
            }
            Assert.IsNull(context);
        }
    }
}
