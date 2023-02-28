/*
 * TestSessionContext.cs
 * SnowplowTrackerTests.Payloads.Contexts
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
using SnowplowTracker.Payloads.Contexts;
using SnowplowTracker.Enums;
using Newtonsoft.Json;

namespace SnowplowTrackerTests.Payloads.Contexts
{

    [TestFixture()]
    public class TestSessionContext
    {

        [Test()]
        public void TestInitMinimal()
        {
            SessionContext context = new SessionContext().SetUserId("userid").SetSessionId("sessionid").SetSessionIndex(30).SetPreviousSessionId(null).SetStorageMechanism(StorageMechanism.Litedb).Build();
            Assert.NotNull(context);

            Dictionary<string, object> dict = context.GetData();
            Assert.AreEqual(5, dict.Count);
            Assert.AreEqual("userid", dict[Constants.SESSION_USER_ID]);
            Assert.AreEqual("sessionid", dict[Constants.SESSION_ID]);
            Assert.AreEqual(30, dict[Constants.SESSION_INDEX]);
            Assert.AreEqual(null, dict[Constants.SESSION_PREVIOUS_ID]);
            Assert.AreEqual("LITEDB", dict[Constants.SESSION_STORAGE]);

            Assert.AreEqual("iglu:com.snowplowanalytics.snowplow/client_session/jsonschema/1-0-1", context.GetSchema());
            CollectionAssert.AreEquivalent(JsonConvert.DeserializeObject<Dictionary<string, object>>("{\"schema\":\"iglu:com.snowplowanalytics.snowplow/client_session/jsonschema/1-0-1\", \"data\":{\"userId\": \"userid\", \"sessionId\": \"sessionid\", \"sessionIndex\": 30, \"previousSessionId\": null, \"storageMechanism\": \"LITEDB\"}}"), JsonConvert.DeserializeObject<Dictionary<string, object>>(context.GetJson().ToString()));
        }

        [Test()]
        public void TestInitFull()
        {
            SessionContext context = new SessionContext().SetUserId("userid").SetSessionId("sessionid").SetSessionIndex(30).SetPreviousSessionId(null).SetStorageMechanism(StorageMechanism.Litedb).SetFirstEventId("firstid").Build();
            Assert.NotNull(context);

            Dictionary<string, object> dict = context.GetData();
            Assert.AreEqual(6, dict.Count);
            Assert.AreEqual("userid", dict[Constants.SESSION_USER_ID]);
            Assert.AreEqual("sessionid", dict[Constants.SESSION_ID]);
            Assert.AreEqual(30, dict[Constants.SESSION_INDEX]);
            Assert.AreEqual(null, dict[Constants.SESSION_PREVIOUS_ID]);
            Assert.AreEqual("LITEDB", dict[Constants.SESSION_STORAGE]);
            Assert.AreEqual("firstid", dict[Constants.SESSION_FIRST_ID]);

            Assert.AreEqual("iglu:com.snowplowanalytics.snowplow/client_session/jsonschema/1-0-1", context.GetSchema());
            CollectionAssert.AreEquivalent(JsonConvert.DeserializeObject<Dictionary<string, object>>("{\"schema\":\"iglu:com.snowplowanalytics.snowplow/client_session/jsonschema/1-0-1\",\"data\": {\"userId\":\"userid\",\"sessionId\":\"sessionid\",\"sessionIndex\": 30,\"previousSessionId\":null,\"storageMechanism\":\"LITEDB\",\"firstEventId\":\"firstid\"}}"), JsonConvert.DeserializeObject<Dictionary<string, object>>(context.GetJson().ToString()));
        }

        [Test()]
        public void TestInitExceptions()
        {
            SessionContext context = null;
            try
            {
                context = new SessionContext().Build();
            }
            catch (Exception e)
            {
                Assert.AreEqual("Session Context requires 'userId'.", e.Message);
            }
            Assert.IsNull(context);

            try
            {
                context = new SessionContext().SetUserId("userid").Build();
            }
            catch (Exception e)
            {
                Assert.AreEqual("Session Context requires 'sessionId'.", e.Message);
            }
            Assert.IsNull(context);

            try
            {
                context = new SessionContext().SetUserId("userid").SetSessionId("sessionid").Build();
            }
            catch (Exception e)
            {
                Assert.AreEqual("Session Context requires 'sessionIndex'.", e.Message);
            }
            Assert.IsNull(context);

            try
            {
                context = new SessionContext().SetUserId("userid").SetSessionId("sessionid").SetSessionIndex(30).Build();
            }
            catch (Exception e)
            {
                Assert.AreEqual("Session Context requires 'previousSessionId'.", e.Message);
            }
            Assert.IsNull(context);

            try
            {
                context = new SessionContext().SetUserId("userid").SetSessionId("sessionid").SetSessionIndex(30).SetPreviousSessionId(null).Build();
            }
            catch (Exception e)
            {
                Assert.AreEqual("Session Context requires 'storageMechanism'.", e.Message);
            }
            Assert.IsNull(context);
        }
    }
}
