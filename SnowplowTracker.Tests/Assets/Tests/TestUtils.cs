/*
 * TestUtils.cs
 * SnowplowTrackerTests
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

namespace SnowplowTrackerTests
{

    [TestFixture()]
    public class TestUtils
    {

        [Test()]
        public void TestGetTimestamp()
        {
            long tstamp = Utils.GetTimestamp();
            Assert.AreEqual(13, tstamp.ToString().Length);
        }

        [Test()]
        public void TestGetGuid()
        {
            string guid = Utils.GetGUID();
            Assert.True(System.Text.RegularExpressions.Regex.IsMatch(guid, "^[a-fA-F0-9]{8}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{12}$", System.Text.RegularExpressions.RegexOptions.IgnoreCase));
        }

        [Test()]
        public void TestDictToJSON()
        {
            // Produces valid JSON
            Dictionary<string, object> test = new Dictionary<string, object>();
            test.Add("hello", "world");
            string jsonTest = Utils.DictToJSONString(test);

            Assert.AreEqual("{\"hello\":\"world\"}", jsonTest);

            // Will fail to produce JSON
            Dictionary<object, object> test2 = new Dictionary<object, object>();
            test2.Add("hello", "world");
            jsonTest = Utils.DictToJSONString(test2);

            Assert.AreEqual("{\"hello\":\"world\"}", jsonTest);
        }

        [Test()]
        public void TestGetUtf8length()
        {
            string test = "UTF8 String length is quite important! Even with special characters: ç";
            long length = Utils.GetUTF8Length(test);

            Assert.AreEqual(71, length);
        }

        [Test()]
        public void TestBase64EncodeString()
        {
            string test = "UTF8 String length is quite important! Even with special characters: ç";
            string base64 = Utils.Base64EncodeString(test);

            Assert.AreEqual("VVRGOCBTdHJpbmcgbGVuZ3RoIGlzIHF1aXRlIGltcG9ydGFudCEgRXZlbiB3aXRoIHNwZWNpYWwgY2hhcmFjdGVyczogw6c=", base64);
        }

        [Test()]
        public void TestDictionaryToQueryString()
        {
            Dictionary<string, object> test = new Dictionary<string, object>();
            test.Add("hello", "world");
            string query = Utils.ToQueryString(test);

            Assert.AreEqual("?hello=world", query);

            test.Add("foo", "bar");
            query = Utils.ToQueryString(test);

            Assert.AreEqual("?hello=world&foo=bar", query);
        }

        [Test()]
        public void TestCheckArgument()
        {
            try
            {
                Utils.CheckArgument(false, "This will throw.");
            }
            catch (Exception e)
            {
                Assert.AreEqual("This will throw.", e.Message);
            }
        }

        [Test()]
        public void TestSaveAndReadDictionaryToFile()
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            data.Add("hello", "world");
            data.Add("number", 2);
            string path = "data.dict";

            Utils.WriteDictionaryToFile(path, data);
            Dictionary<string, object> retrieved = Utils.ReadDictionaryFromFile(path);

            Assert.AreEqual(data, retrieved);
        }

        [Test()]
        public void TestIsTimeInRange()
        {
            long startTime = 1443513030000;
            long checkTime = 1443513530000;
            long range = 600000; // 10 minutes

            Assert.True(Utils.IsTimeInRange(startTime, checkTime, range));
        }
    }
}
