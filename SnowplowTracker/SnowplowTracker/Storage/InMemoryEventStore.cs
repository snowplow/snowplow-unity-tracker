/*
 * InMemoryEventStore.cs
 * SnowplowTracker.Storage
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
 */

using System;
using System.Collections;
using System.Collections.Generic;
using SnowplowTracker.Payloads;

namespace SnowplowTracker.Storage
{
    /// <summary>
    ///  Buffers events (as TrackerPayloads) in memory before they are sent by the emitter.
    /// </summary>
	public class InMemoryEventStore : IStore
	{
        private Dictionary<Guid, TrackerPayload> eventQueue = new Dictionary<Guid, TrackerPayload>();
        private int bufferCapacity;

        /// <summary>
        /// Initializes a new instance of the event store.
        /// </summary>
        /// <param name="bufferCapacity">
        /// Maximum number of unsent events to hold in memory.
        /// When crossed, new events won't be added to the store and will be discarded.
        /// </param>
		public InMemoryEventStore(int bufferCapacity = 10000)
		{
            this.bufferCapacity = bufferCapacity;
		}

        /// <summary>
        /// Adds an event payload to the store.
        /// </summary>
        /// <returns><c>true</c>, if event was added, <c>false</c> otherwise.</returns>
        /// <param name="payload">An event payload</param>
        public bool AddEvent(TrackerPayload payload)
        {
            bool success = false;
            lock (eventQueue)
            {
                if (eventQueue.Count < bufferCapacity)
                {
                    eventQueue.Add(Guid.NewGuid(), payload);
                    success = true;
                }
            }
            if (success)
            {
                Log.Verbose("EventStore: Event added");
                return true;
            }
            else
            {
                Log.Error("EventStore: Reached buffer capacity, event won't be tracked");
                return false;
            }
        }

        /// <summary>
        /// Deletes all events from an array of row ids.
        /// </summary>
        /// <returns><c>true</c>, if events were deleted, <c>false</c> otherwise.</returns>
        /// <param name="rowIds">Row id guids</param>
        public bool DeleteEvents(List<Guid> rowIds)
        {
            lock (eventQueue)
            {
                foreach (var rowId in rowIds)
                {
                    eventQueue.Remove(rowId);
                }
            }
            Log.Verbose($"EventStore: {rowIds.Count} events deleted");
            return true;
        }

        /// <summary>
        /// Gets the count of events in the database.
        /// </summary>
        /// <returns>The event count.</returns>
        public long GetEventCount()
        {
            lock (eventQueue)
            {
                return eventQueue.Count;
            }
        }

        /// <summary>
        /// Gets a descending range of events from the database.
        /// </summary>
        /// <returns>The list of rows within the range</returns>
        /// <param name="range">The amount of rows we want</param>
        public List<EventRow> GetEvents(int range)
        {
            List<EventRow> events = new List<EventRow>();
            lock (eventQueue)
            {
                foreach (var rowId in eventQueue.Keys)
                {
                    if (events.Count >= range) { break; }

                    // make a shallow copy of the payload
                    // to avoid adding the stm property twice on retries
                    TrackerPayload payload = new TrackerPayload();
                    payload.AddDict(eventQueue[rowId].GetDictionary());
                    events.Add(new EventRow(rowId, payload));
                }
            }
            Log.Verbose($"EventStore: {events.Count} events retrieved");
            return events;
        }
    }
}

