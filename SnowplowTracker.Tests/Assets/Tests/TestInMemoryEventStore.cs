/*
 * TestInMemoryEventStore.cs
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
 */

using System;
using System.Collections.Generic;
using NUnit.Framework;
using SnowplowTracker;
using SnowplowTracker.Payloads;
using SnowplowTracker.Storage;

namespace SnowplowTrackerTests
{

    [TestFixture()]
    public class TestInMemoryEventStore
    {

        [Test()]
        public void TestAddsEventsToStorage()
        {
            InMemoryEventStore store = new InMemoryEventStore();
            store.AddEvent(new TrackerPayload());
            store.AddEvent(new TrackerPayload());

            Assert.AreEqual(2, store.GetEventCount());
        }

        [Test()]
        public void TestDoesntAddMoreEventsThanCapacity()
        {
            InMemoryEventStore store = new InMemoryEventStore(2);
            store.AddEvent(new TrackerPayload());
            store.AddEvent(new TrackerPayload());
            store.AddEvent(new TrackerPayload());

            Assert.AreEqual(2, store.GetEventCount());
        }

        [Test()]
        public void TestRetrievesEventFromStorage()
        {
            var store = new InMemoryEventStore();
            var payload = new TrackerPayload();
            payload.Add("test", "value");
            store.AddEvent(payload);
            store.AddEvent(new TrackerPayload());

            var events = store.GetEvents(1);

            Assert.AreEqual(1, events.Count);
            Assert.AreEqual("value", events[0].GetPayload().GetDictionary()["test"]);
        }

        [Test()]
        public void TestRemovesEventFromStorage()
        {
            var store = new InMemoryEventStore();
            store.AddEvent(new TrackerPayload());

            var events = store.GetEvents(1);
            var rowIds = new List<Guid>();
            rowIds.Add(events[0].GetRowId());
            store.DeleteEvents(rowIds);

            Assert.AreEqual(0, store.GetEventCount());
        }
    }
}
