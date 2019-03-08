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
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Mono.Data.SqliteClient;
using SnowplowTracker.Payloads;
using UnityEngine;

namespace SnowplowTracker.Storage
{
    public class EventStore
    {

        // Database Details
        private static string DATABASE_NAME;
        private static readonly string TABLE_NAME = "events";
        private static readonly string COLUMN_ID = "id";
        private static readonly string COLUMN_EVENT_DATA = "eventData";

        // SQL Statements
        private static readonly string CREATE_TABLE = "CREATE TABLE IF NOT EXISTS '" + TABLE_NAME + "' (" + COLUMN_ID + " INTEGER PRIMARY KEY, " + COLUMN_EVENT_DATA + " BLOB)";
        private static readonly string INSERT_EVENT = "INSERT INTO " + TABLE_NAME + " (" + COLUMN_EVENT_DATA + ") " + "VALUES ";
        private static readonly string COUNT = "SELECT COUNT(*) as events FROM " + TABLE_NAME;
        private static readonly string SELECT_ALL = "SELECT * FROM " + TABLE_NAME;
        private static readonly string SELECT_RANGE = "SELECT * FROM " + TABLE_NAME + " ORDER BY " + COLUMN_ID + " DESC LIMIT ";
        private static readonly string SELECT_ROW = "SELECT * FROM " + TABLE_NAME + " WHERE " + COLUMN_ID + "=";
        private static readonly string DELETE_ALL = "DELETE FROM " + TABLE_NAME;
        private static readonly string DELETE_ROW = "DELETE FROM " + TABLE_NAME + " WHERE " + COLUMN_ID + "=";
        private static readonly string DELETE_ROWS = "DELETE FROM " + TABLE_NAME + " WHERE " + COLUMN_ID + " in ";

        // Connection
        private SqliteConnection dbConnection;

        /// <summary>
        /// Initializes a new instance of the <see cref="SnowplowTracker.Storage.EventStore"/> class.
        /// </summary>
        public EventStore()
        {
            if (DATABASE_NAME == null)
            {
                DATABASE_NAME = "URI=file:"+Application.persistentDataPath+"/snowplow_events.db";
            }
            Open();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SnowplowTracker.Storage.EventStore"/> class.
        /// With the database location.
        /// </summary>
        public EventStore(string DatabaseName)
        {
            if (!string.IsNullOrEmpty(DatabaseName))
            {
                DATABASE_NAME = DatabaseName;
            }
            Log.Debug("EventStore: creating new eventstore");
            Open();
        }

        /// <summary>
        /// Opens a connection and create's the events tables if required.
        /// </summary>
        public void Open()
        {
            if (!IsDatabaseOpen())
            {
                dbConnection = new SqliteConnection(DATABASE_NAME);
                dbConnection.Open();
                using (var someCommand = dbConnection.CreateCommand())
                {
                    someCommand.CommandType = System.Data.CommandType.Text;
                    // Set database mode to Write Ahead Logging
                    someCommand.CommandText = "PRAGMA JOURNAL_MODE = WAL;";
                    someCommand.ExecuteNonQuery();

                    // Set database mode to Serialized for safe multi-threading
                    someCommand.CommandText = "PRAGMA SQLITE_THREADSAFE = 0;";
                    someCommand.ExecuteNonQuery();

                    // Create the Table if it does not exist
                    someCommand.CommandText = CREATE_TABLE;
                    someCommand.ExecuteNonQuery();

                    Log.Verbose(" + Database open status: " + IsDatabaseOpen());

                }
            }
        }

        /// <summary>
        /// Closes the database connection if is currently active.
        /// </summary>
        public void Close()
        {
            if (IsDatabaseOpen())
            {
                dbConnection.Close();
                dbConnection.Dispose();
                dbConnection = null;
            }
        }

        // --- Database Functions

        /// <summary>
        /// Adds an event payload to the database.
        /// </summary>
        /// <returns><c>true</c>, if event was added, <c>false</c> otherwise.</returns>
        /// <param name="payload">An event payload</param>
        public bool AddEvent(TrackerPayload payload)
        {
            bool result = false;
            if (IsDatabaseOpen())
            {
                byte[] bytes = Utils.SerializeDictionary(payload.GetDictionary());
                if (bytes != null)
                {
                    using (var someCommand = dbConnection.CreateCommand())
                    {
                        Log.Debug("Emitter: EventStore.AddEvent preparing SQL");
                        someCommand.CommandText = INSERT_EVENT + "(@payload)";
                        Log.Verbose("EventStore: Event add: Checking for null property");
                        SqliteParameter param = new SqliteParameter("@payload", System.Data.DbType.Binary);
                        param.Value = bytes;
                        someCommand.Parameters.Add(param);
                        // Check whether insert succeeded
                        result = someCommand.ExecuteNonQuery() > 0;
                        // Close and dispose of resources
                    }
                }
            }
            Log.Verbose("EventStore: Event add result: " + result);
            return result;
        }

        /// <summary>
        /// Deletes the event.
        /// </summary>
        /// <returns><c>true</c>, if event was deleted, <c>false</c> otherwise.</returns>
        /// <param name="rowId">Row id number</param>
        public bool DeleteEvent(int rowId)
        {
            bool result = false;
            if (IsDatabaseOpen())
            {
                using (var someCommand = dbConnection.CreateCommand())
                {
                    someCommand.CommandText = DELETE_ROW + rowId;
                    result = someCommand.ExecuteNonQuery() > 0;

                    Log.Verbose("EventStore: Event delete result for id " + rowId + ": " + result);
                    // Close and dispose of resources
                }
            }
            return result;
        }

        /// <summary>
        /// Deletes all events from an array of row ids.
        /// </summary>
        /// <returns><c>true</c>, if events were deleted, <c>false</c> otherwise.</returns>
        /// <param name="rowId">Row id number</param>
        public bool DeleteEvents(List<int> rowIds)
        {
            bool result = false;
            if (IsDatabaseOpen())
            {
                using (var someCommand = dbConnection.CreateCommand())
                {
                    string ids = String.Join(",", rowIds.ConvertAll(x => x.ToString()).ToArray());
                    Log.Debug("Emitter: EventStore.DeleteEvents: ids: " + ids);
                    string cmdText = DELETE_ROWS + "(" + ids + ")";
                    Log.Debug("Emitter: EventStore.DeleteEvents: prepared st: " + cmdText);

                    someCommand.CommandText = cmdText;
                    result = someCommand.ExecuteNonQuery() > 0;

                    Log.Verbose("EventStore: Deleted " + rowIds.Count + " events.");
                    Log.Verbose(" + Row IDs: " + ids);
                    Log.Verbose(" + Result: " + result);
                    // Close and dispose of resources
                }
            }
            return result;
        }

        /// <summary>
        /// Deletes all events.
        /// </summary>
        /// <returns><c>true</c>, if all events was deleted, <c>false</c> otherwise.</returns>
        public bool DeleteAllEvents()
        {
            bool result = false;
            if (IsDatabaseOpen())
            {
                using (var someCommand = dbConnection.CreateCommand())
                {
                    someCommand.CommandText = DELETE_ALL;
                    result = someCommand.ExecuteNonQuery() > 0;

                    Log.Verbose("EventStore: Delete all events result: " + result);
                    // Close and dispose of resources
                }
            }
            return result;
        }

        /// <summary>
        /// Gets the count of events in the database.
        /// </summary>
        /// <returns>The event count.</returns>
        public long GetEventCount()
        {
            long count = 0;
            if (IsDatabaseOpen())
            {
                using (var someCommand = dbConnection.CreateCommand())
                {
                    someCommand.CommandText = COUNT;
                    using (var reader = someCommand.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                int colIndex = reader.GetOrdinal("events");
                                if (!reader.IsDBNull(colIndex))
                                {
                                    count = (long)reader.GetInt64(colIndex);
                                    Log.Verbose("EventStore: Get Event Count: " + count);
                                }
                                else
                                {
                                    count = 0;
                                }
                            }
                        }
                    }
                }
            }
            return count;
        }

        /// <summary>
        /// Gets an event at a specific row id
        /// </summary>
        /// <returns>The event row</returns>
        /// <param name="rowId">Row id number</param>
        public EventRow GetEvent(int rowId)
        {
            List<EventRow> rows = QueryDatabase(SELECT_ROW + rowId);
            if (rows.Count == 1)
            {
                return rows[0];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets all events in the database
        /// </summary>
        /// <returns>The list of rows</returns>
        public List<EventRow> GetAllEvents()
        {
            return QueryDatabase(SELECT_ALL);
        }

        /// <summary>
        /// Gets a descending range of events from the database.
        /// </summary>
        /// <returns>The list of rows within the range</returns>
        /// <param name="range">The amount of rows we want</param>
        public List<EventRow> GetDescEventRange(int range)
        {
            return QueryDatabase(SELECT_RANGE + range);
        }

        /// <summary>
        /// Queries the database.
        /// </summary>
        /// <returns>A list of rows returned from the database</returns>
        /// <param name="query">The SQL Query to execute</param>
        public List<EventRow> QueryDatabase(string query)
        {
            List<EventRow> rows = new List<EventRow>();
            if (IsDatabaseOpen())
            {
                try
                {
                    using (var someCommand = dbConnection.CreateCommand())
                    {
                        someCommand.CommandText = query;

                        using (var reader = someCommand.ExecuteReader())
                        {
                            // Add the row id and deserialized row data to the list
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    if (!reader.IsDBNull(reader.GetOrdinal(COLUMN_ID)) && !reader.IsDBNull(reader.GetOrdinal(COLUMN_EVENT_DATA)))
                                    {
                                        int rowId = (int)reader[COLUMN_ID];
                                        Dictionary<string, object> payloadDict = Utils.DeserializeDictionary((byte[])reader[COLUMN_EVENT_DATA]);
                                        TrackerPayload payload = new TrackerPayload();
                                        payload.AddDict(payloadDict);
                                        rows.Add(new EventRow(rowId, payload));
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Error("EventStore: Error querying database: " + e.StackTrace);
                }
            }
            return rows;
        }

        // --- Helpers

        /// <summary>
        /// Determines whether this database connection is open.
        /// </summary>
        /// <returns><c>true</c> if this the database is open; otherwise, <c>false</c>.</returns>
        public bool IsDatabaseOpen()
        {
            if (dbConnection == null)
            {
                Log.Verbose("Emitter: Database is null");
                return false;
            }
            else if (dbConnection.State != System.Data.ConnectionState.Open)
            {
                Log.Verbose("Emitter: Database connection is " + dbConnection.State.ToString());
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
