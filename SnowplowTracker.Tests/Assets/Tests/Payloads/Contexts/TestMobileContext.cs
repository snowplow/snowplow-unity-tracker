/*
 * TestMobileContext.cs
 * SnowplowTrackerTests.Payloads.Contexts
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
using SnowplowTracker.Payloads.Contexts;
using SnowplowTracker.Enums;
using Newtonsoft.Json;

namespace SnowplowTrackerTests.Payloads.Contexts
{

    [TestFixture()]
    public class TestMobileContext
    {

        [Test()]
        public void TestInitMinimal()
        {
            MobileContext context = new MobileContext().SetOsType("iOS").SetOsVersion("9.0").SetDeviceManufacturer("Apple").SetDeviceModel("iPhone 6S+").Build();
            Assert.NotNull(context);

            Dictionary<string, object> dict = context.GetData();
            Assert.AreEqual(4, dict.Count);
            Assert.AreEqual("iOS", dict[Constants.PLAT_OS_TYPE]);
            Assert.AreEqual("9.0", dict[Constants.PLAT_OS_VERSION]);
            Assert.AreEqual("Apple", dict[Constants.PLAT_DEVICE_MANU]);
            Assert.AreEqual("iPhone 6S+", dict[Constants.PLAT_DEVICE_MODEL]);

            Assert.AreEqual("iglu:com.snowplowanalytics.snowplow/mobile_context/jsonschema/1-0-1", context.GetSchema());
            CollectionAssert.AreEquivalent(JsonConvert.DeserializeObject<Dictionary<string, object>>("{\"schema\":\"iglu:com.snowplowanalytics.snowplow/mobile_context/jsonschema/1-0-1\", \"data\":{\"osType\":\"iOS\",\"osVersion\":\"9.0\",\"deviceManufacturer\":\"Apple\",\"deviceModel\":\"iPhone 6S+\"}}"), JsonConvert.DeserializeObject<Dictionary<string, object>>(context.GetJson().ToString()));
        }

        [Test()]
        public void TestInitFull()
        {
            MobileContext context = new MobileContext()
                .SetOsType("iOS")
                .SetOsVersion("9.0")
                .SetDeviceManufacturer("Apple")
                .SetDeviceModel("iPhone 6S+")
                .SetCarrier("FREE")
                .SetNetworkType(NetworkType.Mobile)
                .SetNetworkTechnology("LTE")
                .SetOpenIdfa("openidfa")
                .SetAppleIdfa("appleidfa")
                .SetAppleIdfv("appleidfv")
                .SetAndroidIdfa("androididfa")
                .Build();
            Assert.NotNull(context);

            Dictionary<string, object> dict = context.GetData();
            Assert.AreEqual(11, dict.Count);
            Assert.AreEqual("iOS", dict[Constants.PLAT_OS_TYPE]);
            Assert.AreEqual("9.0", dict[Constants.PLAT_OS_VERSION]);
            Assert.AreEqual("Apple", dict[Constants.PLAT_DEVICE_MANU]);
            Assert.AreEqual("iPhone 6S+", dict[Constants.PLAT_DEVICE_MODEL]);
            Assert.AreEqual("FREE", dict[Constants.MOBILE_CARRIER]);
            Assert.AreEqual("mobile", dict[Constants.MOBILE_NET_TYPE]);
            Assert.AreEqual("LTE", dict[Constants.MOBILE_NET_TECH]);
            Assert.AreEqual("openidfa", dict[Constants.MOBILE_OPEN_IDFA]);
            Assert.AreEqual("appleidfa", dict[Constants.MOBILE_APPLE_IDFA]);
            Assert.AreEqual("appleidfv", dict[Constants.MOBILE_APPLE_IDFV]);
            Assert.AreEqual("androididfa", dict[Constants.MOBILE_ANDROID_IDFA]);

            Assert.AreEqual("iglu:com.snowplowanalytics.snowplow/mobile_context/jsonschema/1-0-1", context.GetSchema());
            CollectionAssert.AreEquivalent(JsonConvert.DeserializeObject<Dictionary<string, object>>("{\"schema\":\"iglu:com.snowplowanalytics.snowplow/mobile_context/jsonschema/1-0-1\", \"data\":{\"osType\":\"iOS\",\"osVersion\":\"9.0\",\"deviceManufacturer\":\"Apple\",\"deviceModel\":\"iPhone 6S+\",\"carrier\":\"FREE\",\"networkType\":\"mobile\",\"networkTechnology\":\"LTE\",\"openIdfa\":\"openidfa\",\"appleIdfa\":\"appleidfa\",\"appleIdfv\":\"appleidfv\",\"androidIdfa\":\"androididfa\"}}"), JsonConvert.DeserializeObject<Dictionary<string, object>>(context.GetJson().ToString()));
        }

        [Test()]
        public void TestInitExceptions()
        {
            MobileContext context = null;
            try
            {
                context = new MobileContext().Build();
            }
            catch (Exception e)
            {
                Assert.AreEqual("MobileContext requires 'osType'.", e.Message);
            }
            Assert.IsNull(context);

            try
            {
                context = new MobileContext().SetOsType("iOS").Build();
            }
            catch (Exception e)
            {
                Assert.AreEqual("MobileContext requires 'osVersion'.", e.Message);
            }
            Assert.IsNull(context);

            try
            {
                context = new MobileContext().SetOsType("iOS").SetOsVersion("9.0").Build();
            }
            catch (Exception e)
            {
                Assert.AreEqual("MobileContext requires 'deviceManufacturer'.", e.Message);
            }
            Assert.IsNull(context);

            try
            {
                context = new MobileContext().SetOsType("iOS").SetOsVersion("9.0").SetDeviceManufacturer("Apple").Build();
            }
            catch (Exception e)
            {
                Assert.AreEqual("MobileContext requires 'deviceModel'.", e.Message);
            }
            Assert.IsNull(context);
        }
    }
}
