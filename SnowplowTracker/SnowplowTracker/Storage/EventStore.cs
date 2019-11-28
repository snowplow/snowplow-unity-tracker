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
using System.Collections.Generic;
using System.IO;
using SnowplowTracker.Payloads;

namespace SnowplowTracker.Storage
{
    public class EventStore
    {
        // Connection
        //private SqliteConnection dbConnection;

        /// <summary>
        /// Initializes a new instance of the <see cref="SnowplowTracker.Storage.EventStore"/> class.
        /// </summary>
        public EventStore()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SnowplowTracker.Storage.EventStore"/> class.
        /// With the database location.
        /// </summary>
        public EventStore(string DatabaseName)
        {
        }

        /// <summary>
        /// Opens a connection and create's the events tables if required.
        /// </summary>
        public void Open()
        {
        }

        /// <summary>
        /// Closes the database connection if is currently active.
        /// </summary>
        public void Close()
        {
        }

        // --- Database Functions

        /// <summary>
        /// Adds an event payload to the database.
        /// </summary>
        /// <returns><c>true</c>, if event was added, <c>false</c> otherwise.</returns>
        /// <param name="payload">An event payload</param>
        public bool AddEvent(TrackerPayload payload)
        {
            return false;
        }

        /// <summary>
        /// Deletes the event.
        /// </summary>
        /// <returns><c>true</c>, if event was deleted, <c>false</c> otherwise.</returns>
        /// <param name="rowId">Row id number</param>
        public bool DeleteEvent(int rowId)
        {
            return false;
        }

        /// <summary>
        /// Deletes all events from an array of row ids.
        /// </summary>
        /// <returns><c>true</c>, if events were deleted, <c>false</c> otherwise.</returns>
        /// <param name="rowId">Row id number</param>
        public bool DeleteEvents(List<int> rowIds)
        {
            return false;
        }

        /// <summary>
        /// Deletes all events.
        /// </summary>
        /// <returns><c>true</c>, if all events was deleted, <c>false</c> otherwise.</returns>
        public bool DeleteAllEvents()
        {
            return false;
        }

        /// <summary>
        /// Gets the count of events in the database.
        /// </summary>
        /// <returns>The event count.</returns>
        public long GetEventCount()
        {
            return 0L;
        }

        /// <summary>
        /// Gets an event at a specific row id
        /// </summary>
        /// <returns>The event row</returns>
        /// <param name="rowId">Row id number</param>
        public EventRow GetEvent(int rowId)
        {
                return null;
        }

        /// <summary>
        /// Gets all events in the database
        /// </summary>
        /// <returns>The list of rows</returns>
        public List<EventRow> GetAllEvents()
        {
            return new List<EventRow>();
        }

        /// <summary>
        /// Gets a descending range of events from the database.
        /// </summary>
        /// <returns>The list of rows within the range</returns>
        /// <param name="range">The amount of rows we want</param>
        public List<EventRow> GetDescEventRange(int range)
        {
            return new List<EventRow>();
        }

        /// <summary>
        /// Queries the database.
        /// </summary>
        /// <returns>A list of rows returned from the database</returns>
        /// <param name="query">The SQL Query to execute</param>
        public List<EventRow> QueryDatabase(string query)
        {
            return new List<EventRow>();
        }

        // --- Helpers

        /// <summary>
        /// Determines whether this database connection is open.
        /// </summary>
        /// <returns><c>true</c> if this the database is open; otherwise, <c>false</c>.</returns>
        public bool IsDatabaseOpen()
        {
            return false;
        }
    }
}
