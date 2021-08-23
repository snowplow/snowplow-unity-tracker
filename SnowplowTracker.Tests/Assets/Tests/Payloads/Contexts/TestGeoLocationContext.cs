/*
 * TestGeoLocationContext.cs
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
using Newtonsoft.Json;
using NUnit.Framework;
using SnowplowTracker;
using SnowplowTracker.Payloads.Contexts;

namespace SnowplowTrackerTests.Payloads.Contexts
{

    [TestFixture()]
    public class TestGeoLocationContext
    {

        [Test()]
        public void TestInitMinimal()
        {
            GeoLocationContext context = new GeoLocationContext().SetLatitude(123.564).SetLongitude(-12.6).Build();
            Assert.NotNull(context);

            Dictionary<string, object> dict = context.GetData();
            Assert.AreEqual(2, dict.Count);
            Assert.AreEqual(123.564, dict[Constants.GEO_LAT]);
            Assert.AreEqual(-12.6, dict[Constants.GEO_LONG]);

            Assert.AreEqual("iglu:com.snowplowanalytics.snowplow/geolocation_context/jsonschema/1-1-0", context.GetSchema());
            CollectionAssert.AreEquivalent(JsonConvert.DeserializeObject<Dictionary<string, object>>("{\"data\":{\"latitude\":123.564, \"longitude\":-12.6}, \"schema\":\"iglu:com.snowplowanalytics.snowplow/geolocation_context/jsonschema/1-1-0\"}"), JsonConvert.DeserializeObject<Dictionary<string, object>>(context.GetJson().ToString()));
        }

        [Test()]
        public void TestInitFull()
        {
            GeoLocationContext context = new GeoLocationContext()
                .SetLatitude(123.564)
                .SetLongitude(-12.6)
                .SetLatitudeLongitudeAccuracy(5.6)
                .SetAltitude(5.5)
                .SetAltitudeAccuracy(2.1)
                .SetBearing(3.2)
                .SetSpeed(100.2)
                .SetTimestamp(1234567890000)
                .Build();
            Assert.NotNull(context);

            Dictionary<string, object> dict = context.GetData();
            Assert.AreEqual(8, dict.Count);
            Assert.AreEqual(123.564, dict[Constants.GEO_LAT]);
            Assert.AreEqual(-12.6, dict[Constants.GEO_LONG]);
            Assert.AreEqual(5.6, dict[Constants.GEO_LAT_LONG_ACC]);
            Assert.AreEqual(5.5, dict[Constants.GEO_ALT]);
            Assert.AreEqual(2.1, dict[Constants.GEO_ALT_ACC]);
            Assert.AreEqual(3.2, dict[Constants.GEO_BEARING]);
            Assert.AreEqual(100.2, dict[Constants.GEO_SPEED]);
            Assert.AreEqual(1234567890000, dict[Constants.GEO_TIMESTAMP]);

            Assert.AreEqual("iglu:com.snowplowanalytics.snowplow/geolocation_context/jsonschema/1-1-0", context.GetSchema());
            CollectionAssert.AreEquivalent(JsonConvert.DeserializeObject<Dictionary<string, object>>("{\"schema\":\"iglu:com.snowplowanalytics.snowplow/geolocation_context/jsonschema/1-1-0\", \"data\":{\"latitude\":123.564,\"longitude\":-12.6,\"latitudeLongitudeAccuracy\":5.6,\"altitude\":5.5,\"altitudeAccuracy\":2.1,\"bearing\":3.2,\"speed\":100.2,\"timestamp\":1234567890000}}"), JsonConvert.DeserializeObject<Dictionary<string, object>>(context.GetJson().ToString()));
        }

        [Test()]
        public void TestInitExceptions()
        {
            GeoLocationContext context = null;
            try
            {
                context = new GeoLocationContext().Build();
            }
            catch (Exception e)
            {
                Assert.AreEqual("GeoLocation Context requires 'latitude'.", e.Message);
            }
            Assert.IsNull(context);

            try
            {
                context = new GeoLocationContext().SetLatitude(123.564).Build();
            }
            catch (Exception e)
            {
                Assert.AreEqual("GeoLocation Context requires 'longitude'.", e.Message);
            }
            Assert.IsNull(context);
        }
    }
}
