/*
 * EventStore.cs
 * SnowplowTracker.Storage
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
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using SnowplowTracker.Payloads;
using LiteDB;

namespace SnowplowTracker.Storage
{
    public class EventStore : IStore
    {
        public class Event
        {
            public Guid Id { get; set; }
            public DateTime CreatedAt { get; set; }
            public string Payload { get; set; }
        }

        private const string COLLECTION_NAME = "events";

        private readonly LiteDatabase _db;
        private readonly ReaderWriterLockSlim _dbLock = new ReaderWriterLockSlim();

        public EventStore(string filename = "snowplow_events_lite.db")
        {
            //Exclusive mode required for iOS
            _db = new LiteDatabase(
                    new ConnectionString($"{Application.persistentDataPath }/{filename}")
                    {
                        Mode = FileMode.Exclusive
                    });

            var col = _db.GetCollection<Event>(COLLECTION_NAME);
            col.EnsureIndex("Id");
            col.EnsureIndex("CreatedAt");
        }

        // --- Database Functions

        /// <summary>
        /// Adds an event payload to the database.
        /// </summary>
        /// <returns><c>true</c>, if event was added, <c>false</c> otherwise.</returns>
        /// <param name="payload">An event payload</param>
        public bool AddEvent(TrackerPayload payload)
        {
            try
            {
                _dbLock.EnterWriteLock();
                // Get event collection
                var col = _db.GetCollection<Event>(COLLECTION_NAME);

                col.Insert(new Event { Payload = payload.ToString(), Id = Guid.NewGuid(), CreatedAt = DateTime.UtcNow });

                Log.Verbose("EventStore: Event added");
                return true;
            }
            catch (Exception e)
            {
                Log.Error("EventStore: Event add failed");
                Log.Error(e.ToString());
                return false;
            }
            finally
            {
                _dbLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Deletes all events from an array of row ids.
        /// </summary>
        /// <returns><c>true</c>, if events were deleted, <c>false</c> otherwise.</returns>
        /// <param name="rowId">Row id number</param>
        public bool DeleteEvents(List<Guid> rowIds)
        {
            try
            {
                _dbLock.EnterWriteLock();

                Log.Verbose($"Attempting to delete: {string.Join(",",rowIds)}");

                // Get event collection
                var events = _db.GetCollection<Event>(COLLECTION_NAME);

                var deleteCount = 0;
                foreach(var id in rowIds)
                {
                    deleteCount += events.Delete(new BsonValue(id)) ? 1 : 0;
                }

                Log.Verbose($"EventStore: Events deleted - Attempted: {rowIds.Count} / Deleted: {deleteCount}");
                return deleteCount == rowIds.Count;
            }
            catch (Exception e)
            {
                Log.Error($"EventStore: Events delete failed");
                Log.Error(e.ToString());
                return false;
            }
            finally
            {
                _dbLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Gets the count of events in the database.
        /// </summary>
        /// <returns>The event count.</returns>
        public long GetEventCount()
        {
            try
            {
                _dbLock.EnterReadLock();
                return _db.GetCollection<Event>(COLLECTION_NAME).LongCount();
            }
            catch (Exception e)
            {
                Log.Error($"EventStore: Get Event Count failed");
                Log.Error(e.ToString());
                return 0L;
            }
            finally
            {
                _dbLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Gets a descending range of events from the database.
        /// </summary>
        /// <returns>The list of rows within the range</returns>
        /// <param name="range">The amount of rows we want</param>
        public List<EventRow> GetEvents(int range)
        {
            try
            {
                _dbLock.EnterReadLock();
                // Get event collection
                var events = _db.GetCollection<Event>(COLLECTION_NAME);

                var result = events.FindAll()
                        .OrderBy(x => x.CreatedAt)
                        .Take(range)
                        .Select(x => new EventRow(x.Id, TrackerPayload.From(x.Payload)))
                        .ToList();

                Log.Verbose($"Got events: {string.Join(",", result.Select(x => x.GetRowId()))}");

                return result;
            }
            catch (Exception e)
            {
                Log.Error($"EventStore: Get Events failed");
                Log.Error(e.ToString());
                return new List<EventRow>();
            }
            finally
            {
                _dbLock.ExitReadLock();
            }
        }
    }
}
