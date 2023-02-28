/*
 * TestSubject.cs
 * SnowplowTrackerTests
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
using SnowplowTracker.Payloads;

namespace SnowplowTrackerTests
{

    [TestFixture()]
    public class TestSubject
    {

        [Test()]
        public void TestSubjectInit()
        {
            Subject s1 = new Subject();

            Assert.NotNull(s1);
            Assert.NotNull(s1.GetPayload());
        }

        [Test()]
        public void TestSubjectSetFunctions()
        {
            Subject s1 = new Subject();

            s1.SetUserId("aUserId");
            s1.SetScreenResolution(1920, 1080);
            s1.SetViewPort(1080, 1920);
            s1.SetColorDepth(5);
            s1.SetTimezone("UTC");
            s1.SetLanguage("en-GB");
            s1.SetIpAddress("127.0.0.1");
            s1.SetUseragent("aUserAgent");
            s1.SetDomainUserId("domain-uid");
            s1.SetNetworkUserId("network-uid");

            Dictionary<string, object> dict = s1.GetPayload().GetDictionary();
            Assert.AreEqual(10, dict.Count);

            Assert.AreEqual("aUserId", dict[Constants.UID]);
            Assert.AreEqual("1920x1080", dict[Constants.RESOLUTION]);
            Assert.AreEqual("1080x1920", dict[Constants.VIEWPORT]);
            Assert.AreEqual("5", dict[Constants.COLOR_DEPTH]);
            Assert.AreEqual("UTC", dict[Constants.TIMEZONE]);
            Assert.AreEqual("en-GB", dict[Constants.LANGUAGE]);
            Assert.AreEqual("127.0.0.1", dict[Constants.IP_ADDRESS]);
            Assert.AreEqual("aUserAgent", dict[Constants.USERAGENT]);
            Assert.AreEqual("domain-uid", dict[Constants.DOMAIN_UID]);
            Assert.AreEqual("network-uid", dict[Constants.NETWORK_UID]);
        }
    }
}
