/*
 * TestTracker.cs
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
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SnowplowTrackerTests.TestHelpers;
using SnowplowTracker;
using SnowplowTracker.Emitters;
using SnowplowTracker.Enums;
using SnowplowTracker.Events;
using SnowplowTracker.Payloads;
using SnowplowTracker.Payloads.Contexts;
using SnowplowTracker.Requests;
using SnowplowTracker.Collections;
using UnityEngine.Networking;

namespace SnowplowTrackerTests
{

    [TestFixture()]
    public class TestTracker
    {

        [Test()]
        public void TestTrackerInitMinimal()
        {
            Tracker t = new Tracker(new AsyncEmitter("acme.com", HttpProtocol.HTTP, HttpMethod.POST, 500, 52000L, 52000L), "aNamespace", "aAppId");

            Assert.NotNull(t);
            Assert.NotNull(t.GetEmitter());
            Assert.Null(t.GetSubject());
            Assert.Null(t.GetSession());
            Assert.AreEqual("aNamespace", t.GetTrackerNamespace());
            Assert.AreEqual("aAppId", t.GetAppId());
            Assert.AreEqual(true, t.GetBase64Encoded());
            Assert.AreEqual(DevicePlatforms.Mobile.Value, t.GetPlatform().Value);
        }

        [Test()]
        public void TestTrackerInitException()
        {
            Tracker t = null;
            try
            {
                t = new Tracker(null, "aNamespace", "aAppId");
            }
            catch (Exception e)
            {
                Assert.AreEqual("Emitter cannot be null.", e.Message);
            }
            Assert.Null(t);
        }

        [Test()]
        public void TestTrackerSetterFunctions()
        {
            Subject s1 = new Subject();
            Session sess1 = new Session(null);
            IEmitter e1 = new AsyncEmitter("acme.com", HttpProtocol.HTTP, HttpMethod.POST, 500, 52000L, 52000L);
            Tracker t = new Tracker(e1, "aNamespace", "aAppId", s1, sess1);

            Assert.NotNull(t.GetEmitter());
            Assert.AreEqual("http://acme.com/com.snowplowanalytics.snowplow/tp2", t.GetEmitter().GetCollectorUri().ToString());
            Assert.NotNull(t.GetSubject());
            Assert.NotNull(t.GetSession());
            Assert.AreEqual("aNamespace", t.GetTrackerNamespace());
            Assert.AreEqual("aAppId", t.GetAppId());
            Assert.AreEqual(true, t.GetBase64Encoded());
            Assert.AreEqual(DevicePlatforms.Mobile.Value, t.GetPlatform().Value);

            IEmitter e2 = new AsyncEmitter("acme.com.au", HttpProtocol.HTTP, HttpMethod.POST, 500, 52000L, 52000L);

            t.SetEmitter(e2);
            Assert.AreEqual("http://acme.com.au/com.snowplowanalytics.snowplow/tp2", t.GetEmitter().GetCollectorUri().ToString());
            t.SetSession(null);
            Assert.Null(t.GetSession());
            t.SetSubject(null);
            Assert.Null(t.GetSubject());
            t.SetTrackerNamespace("newNamespace");
            Assert.AreEqual("newNamespace", t.GetTrackerNamespace());
            t.SetAppId("newAppId");
            Assert.AreEqual("newAppId", t.GetAppId());
            t.SetBase64Encoded(false);
            Assert.AreEqual(false, t.GetBase64Encoded());
            t.SetPlatform(DevicePlatforms.Desktop);
            Assert.AreEqual(DevicePlatforms.Desktop.Value, t.GetPlatform().Value);
        }

        [Test()]
        public void TestTrackerSendEvent()
        {
            IEmitter e1 = new BaseEmitter();
            Tracker t = new Tracker(e1, "aNamespace", "aAppId");
            t.StartEventTracking();

            t.Track(new PageView().SetPageTitle("title").SetPageUrl("url").SetReferrer("ref").SetTimestamp(1234567890).SetEventId("event-id-custom").Build());
            t.Track(new PageView().SetPageTitle("title").SetPageUrl("url").SetReferrer("ref").SetTimestamp(1234567890).SetEventId("event-id-custom").Build());

            BaseEmitter te1 = (BaseEmitter)t.GetEmitter();
            Assert.AreEqual(2, te1.payloads.Count);

            foreach (TrackerPayload payload in te1.payloads)
            {
                Dictionary<string, object> dict = payload.GetDictionary();
                Assert.AreEqual(SnowplowTracker.Version.VERSION, dict[Constants.TRACKER_VERSION]);
                Assert.AreEqual("1234567890", dict[Constants.TIMESTAMP]);
                Assert.AreEqual("event-id-custom", dict[Constants.EID]);
                Assert.AreEqual("aNamespace", dict[Constants.NAMESPACE]);
                Assert.AreEqual("aAppId", dict[Constants.APP_ID]);
                Assert.AreEqual("mob", dict[Constants.PLATFORM]);
                Assert.AreEqual(Constants.EVENT_PAGE_VIEW, dict[Constants.EVENT]);
                Assert.AreEqual("title", dict[Constants.PAGE_TITLE]);
                Assert.AreEqual("url", dict[Constants.PAGE_URL]);
                Assert.AreEqual("ref", dict[Constants.PAGE_REFR]);
            }
        }

        [Test()]
        public void TestTrackingEventsDoesntChangeContextsArray()
        {
            IEmitter e1 = new BaseEmitter();
            Session session = new Session(null);
            Tracker t = new Tracker(e1, "aNamespace", "aAppId", null, session);
            t.StartEventTracking();

            List<IContext> contexts = new List<IContext>();
            contexts.Add(new DesktopContext().SetOsType("OS-X").SetOsVersion("10.10.5").SetOsServicePack("Yosemite").SetOsIs64Bit(true).SetDeviceManufacturer("Apple").SetDeviceModel("Macbook Pro").SetDeviceProcessorCount(4).Build());

            t.Track(new PageView().SetPageTitle("title").SetPageUrl("url").SetReferrer("ref").SetTimestamp(1234567890).SetEventId("event-id-custom").SetCustomContext(contexts).Build());

            Assert.AreEqual(1, contexts.Count);
        }

        [Test()]
        public void TestUserAnonymisationSuppressesPiiKeys()
        {
            var subject = new Subject();
            subject.SetUserId("user123");
            subject.SetDomainUserId("duid123");
            subject.SetNetworkUserId("nuid123");
            subject.SetIpAddress("1.2.3.4");
            subject.SetTimezone("UTC");

            IEmitter e1 = new BaseEmitter();
            Tracker t = new Tracker(e1, "ns", "app", subject, userAnonymisation: true);
            t.StartEventTracking();

            t.Track(new PageView().SetPageUrl("url").Build());

            BaseEmitter be = (BaseEmitter)e1;
            Assert.AreEqual(1, be.payloads.Count);
            Dictionary<string, object> dict = be.payloads[0].GetDictionary();
            Assert.IsFalse(dict.ContainsKey(Constants.UID));
            Assert.IsFalse(dict.ContainsKey(Constants.DOMAIN_UID));
            Assert.IsFalse(dict.ContainsKey(Constants.NETWORK_UID));
            Assert.IsFalse(dict.ContainsKey(Constants.IP_ADDRESS));
            Assert.IsTrue(dict.ContainsKey(Constants.TIMEZONE));
        }

        [Test()]
        public void TestUserAnonymisationSessionContext()
        {
            Session session = new Session(null);
            SessionContext ctx = session.GetSessionContext("event-1", userAnonymisation: true);
            Dictionary<string, object> data = ctx.GetData();
            Assert.AreEqual(Constants.SESSION_ANONYMOUS_USER_ID, data[Constants.SESSION_USER_ID]);
            Assert.IsNull(data[Constants.SESSION_PREVIOUS_ID]);
        }

        [Test()]
        public void TestSetUserAnonymisationRotatesSession()
        {
            IEmitter e1 = new BaseEmitter();
            Session session = new Session(null);
            Tracker t = new Tracker(e1, "ns", "app", null, session);
            t.StartEventTracking();

            SessionContext ctx1 = session.GetSessionContext("e1");
            string firstSessionId = (string)ctx1.GetData()[Constants.SESSION_ID];

            t.SetUserAnonymisation(true);

            SessionContext ctx2 = session.GetSessionContext("e2");
            string newSessionId = (string)ctx2.GetData()[Constants.SESSION_ID];

            Assert.AreNotEqual(firstSessionId, newSessionId);
        }

        [Test()]
        public void TestServerAnonymisationHeader()
        {
            var request = new ReadyRequest(
                new HttpRequest(HttpMethod.POST, new Uri("https://example.com/tp2")),
                new System.Collections.Generic.List<Guid>(),
                false,
                new SnowplowTracker.Collections.ConcurrentQueue<RequestResult>(),
                serverAnonymisation: true
            );
            UnityWebRequest webRequest = request.GetUnityWebRequest("POST", new Uri("https://example.com/tp2"), "{}");
            Assert.AreEqual("*", webRequest.GetRequestHeader("SP-Anonymous"));
            webRequest.Dispose();
        }

        [Test()]
        public void TestUserAnonymisationToggleRestoresFull()
        {
            var subject = new Subject();
            subject.SetUserId("user123");

            IEmitter e1 = new BaseEmitter();
            Tracker t = new Tracker(e1, "ns", "app", subject);
            t.StartEventTracking();

            t.Track(new PageView().SetPageUrl("url").Build());
            Assert.IsTrue(((BaseEmitter)e1).payloads[0].GetDictionary().ContainsKey(Constants.UID));

            t.SetUserAnonymisation(true);
            t.Track(new PageView().SetPageUrl("url").Build());
            Assert.IsFalse(((BaseEmitter)e1).payloads[1].GetDictionary().ContainsKey(Constants.UID));

            t.SetUserAnonymisation(false);
            t.Track(new PageView().SetPageUrl("url").Build());
            Assert.IsTrue(((BaseEmitter)e1).payloads[2].GetDictionary().ContainsKey(Constants.UID));
        }

        [Test()]
        public void TestUserAnonymisationAppliedAtTrackTime()
        {
            var subject = new Subject();
            subject.SetUserId("user123");

            IEmitter e1 = new BaseEmitter();
            Tracker t = new Tracker(e1, "ns", "app", subject, userAnonymisation: true);
            t.StartEventTracking();

            t.Track(new PageView().SetPageUrl("url").Build());
            Assert.IsFalse(((BaseEmitter)e1).payloads[0].GetDictionary().ContainsKey(Constants.UID));

            t.SetUserAnonymisation(false);

            // Already-stored event retains masked payload
            Assert.IsFalse(((BaseEmitter)e1).payloads[0].GetDictionary().ContainsKey(Constants.UID));
        }

        [Test()]
        public void TestServerAnonymisationToggleAffectsHeader()
        {
            var requestWithAnon = new ReadyRequest(
                new HttpRequest(HttpMethod.POST, new Uri("https://example.com/tp2")),
                new System.Collections.Generic.List<Guid>(),
                false,
                new SnowplowTracker.Collections.ConcurrentQueue<RequestResult>(),
                serverAnonymisation: true
            );
            UnityWebRequest webRequestWithAnon = requestWithAnon.GetUnityWebRequest("POST", new Uri("https://example.com/tp2"), "{}");
            Assert.AreEqual("*", webRequestWithAnon.GetRequestHeader("SP-Anonymous"));
            webRequestWithAnon.Dispose();

            var requestWithoutAnon = new ReadyRequest(
                new HttpRequest(HttpMethod.POST, new Uri("https://example.com/tp2")),
                new System.Collections.Generic.List<Guid>(),
                false,
                new SnowplowTracker.Collections.ConcurrentQueue<RequestResult>(),
                serverAnonymisation: false
            );
            UnityWebRequest webRequestWithoutAnon = requestWithoutAnon.GetUnityWebRequest("POST", new Uri("https://example.com/tp2"), "{}");
            Assert.IsNull(webRequestWithoutAnon.GetRequestHeader("SP-Anonymous"));
            webRequestWithoutAnon.Dispose();
        }

        [Test()]
        public void TestSyncPostWithServerAnonymisationAddsHeader()
        {
            var handler = new MockHttpMessageHandler();
            var httpClient = new System.Net.Http.HttpClient(handler);
            var request = new ReadyRequest(
                new HttpRequest(SnowplowTracker.Enums.HttpMethod.POST, new Uri("https://example.com/tp2"), new System.Net.Http.StringContent("{}")),
                new List<Guid>(),
                false,
                new SnowplowTracker.Collections.ConcurrentQueue<RequestResult>(),
                serverAnonymisation: true,
                httpClient: httpClient
            );
            request.Send();
            Assert.IsNotNull(handler.LastRequest);
            Assert.AreEqual("*", handler.LastRequest.Headers.GetValues("SP-Anonymous").First());
        }

        [Test()]
        public void TestSyncPostWithoutServerAnonymisationOmitsHeader()
        {
            var handler = new MockHttpMessageHandler();
            var httpClient = new System.Net.Http.HttpClient(handler);
            var request = new ReadyRequest(
                new HttpRequest(SnowplowTracker.Enums.HttpMethod.POST, new Uri("https://example.com/tp2"), new System.Net.Http.StringContent("{}")),
                new List<Guid>(),
                false,
                new SnowplowTracker.Collections.ConcurrentQueue<RequestResult>(),
                serverAnonymisation: false,
                httpClient: httpClient
            );
            request.Send();
            Assert.IsNotNull(handler.LastRequest);
            Assert.IsFalse(handler.LastRequest.Headers.Contains("SP-Anonymous"));
        }

        [Test()]
        public void TestSyncGetWithServerAnonymisationAddsHeader()
        {
            var handler = new MockHttpMessageHandler();
            var httpClient = new System.Net.Http.HttpClient(handler);
            var request = new ReadyRequest(
                new HttpRequest(SnowplowTracker.Enums.HttpMethod.GET, new Uri("https://example.com/i?e=pv")),
                new List<Guid>(),
                false,
                new SnowplowTracker.Collections.ConcurrentQueue<RequestResult>(),
                serverAnonymisation: true,
                httpClient: httpClient
            );
            request.Send();
            Assert.IsNotNull(handler.LastRequest);
            Assert.AreEqual("*", handler.LastRequest.Headers.GetValues("SP-Anonymous").First());
        }
    }

    internal class MockHttpMessageHandler : System.Net.Http.HttpMessageHandler
    {
        public System.Net.Http.HttpRequestMessage LastRequest { get; private set; }

        protected override Task<System.Net.Http.HttpResponseMessage> SendAsync(System.Net.Http.HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(new System.Net.Http.HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}
