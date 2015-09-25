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

namespace SnowplowTracker.Storage {
	public class EventStore {

		// Database Details
		private static readonly string DATABASE_NAME     = "URI=file:snowplow_events.db";
		private static readonly string TABLE_NAME        = "events";
		private static readonly string COLUMN_ID         = "id";
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
		private IDbConnection dbConnection;

		/// <summary>
		/// Initializes a new instance of the <see cref="SnowplowTracker.Storage.EventStore"/> class.
		/// </summary>
		public EventStore() {
			Open ();
		}

		/// <summary>
		/// Opens a connection and create's the events tables if required.
		/// </summary>
		public void Open() {
			if (!IsDatabaseOpen ()) {
				Log.Verbose("EventStore: opening database: " + DATABASE_NAME);
				dbConnection = new SqliteConnection (DATABASE_NAME);
				dbConnection.Open();

				IDbCommand command = dbConnection.CreateCommand ();

				// Set database mode to Write Ahead Logging
				command.CommandText = "PRAGMA JOURNAL_MODE = WAL;";
				command.ExecuteNonQuery();

				// Set database mode to Serialized for safe multi-threading
				command.CommandText = "PRAGMA SQLITE_THREADSAFE = 1;";
				command.ExecuteNonQuery();

				// Create the Table if it does not exist
				command.CommandText = CREATE_TABLE;
				command.ExecuteNonQuery();

				Log.Verbose(" + Database open status: " + IsDatabaseOpen ());

				// Close command
				command.Dispose();
				command = null;
			}
		}

		/// <summary>
		/// Closes the database connection if is currently active.
		/// </summary>
		public void Close() {
			if (IsDatabaseOpen ()) {
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
		public bool AddEvent(TrackerPayload payload) {
			bool result = false;
			if (IsDatabaseOpen ()) {
				byte[] bytes = Utils.SerializeDictionary (payload.GetDictionary());
				if (bytes != null) {
					// Build Insert Command
					IDbCommand command = dbConnection.CreateCommand ();
					command.CommandText = INSERT_EVENT + "(@payload)";
					SqliteParameter param = new SqliteParameter ("@payload", DbType.Binary);
					param.Value = bytes;
					command.Parameters.Add (param);
					
					// Check whether insert succeeded
					result = command.ExecuteNonQuery () > 0;
					
					// Close and dispose of resources
					command.Dispose();
					command = null;
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
		public bool DeleteEvent(int rowId) {
			bool result = false;
			if (IsDatabaseOpen ()) {
				IDbCommand command = dbConnection.CreateCommand ();
				command.CommandText = DELETE_ROW + rowId;
				result = command.ExecuteNonQuery () > 0;

				Log.Verbose("EventStore: Event delete result for id " + rowId + ": " + result);

				// Close and dispose of resources
				command.Dispose();
				command = null;
			}
			return result;
		}

		/// <summary>
		/// Deletes all events from an array of row ids.
		/// </summary>
		/// <returns><c>true</c>, if events were deleted, <c>false</c> otherwise.</returns>
		/// <param name="rowId">Row id number</param>
		public bool DeleteEvents(List<int> rowIds) {
			bool result = false;
			if (IsDatabaseOpen ()) {
				IDbCommand command = dbConnection.CreateCommand ();
				string ids = String.Join(",", rowIds.ConvertAll( x => x.ToString() ).ToArray());
				command.CommandText = DELETE_ROWS + "(" + ids+ ")";
				result = command.ExecuteNonQuery () > 0;

				Log.Verbose("EventStore: Deleted " + rowIds.Count + " events.");
				Log.Verbose(" + Row IDs: " + ids);
				Log.Verbose(" + Result: " + result);
				
				// Close and dispose of resources
				command.Dispose();
				command = null;
			}
			return result;
		}

		/// <summary>
		/// Deletes all events.
		/// </summary>
		/// <returns><c>true</c>, if all events was deleted, <c>false</c> otherwise.</returns>
		public bool DeleteAllEvents() {
			bool result = false;
			if (IsDatabaseOpen ()) {
				IDbCommand command = dbConnection.CreateCommand ();
				command.CommandText = DELETE_ALL;
				result = command.ExecuteNonQuery () > 0;

				Log.Verbose("EventStore: Delete all events result: " + result);
				
				// Close and dispose of resources
				command.Dispose();
				command = null;
			}
			return result;
		}

		/// <summary>
		/// Gets the count of events in the database.
		/// </summary>
		/// <returns>The event count.</returns>
		public long GetEventCount() {
			long count = 0;
			if (IsDatabaseOpen ()) {
				IDbCommand command = dbConnection.CreateCommand ();
				command.CommandText = COUNT;
				IDataReader reader = command.ExecuteReader ();
				while (reader.Read()) {
					count = (long)reader["events"];
				}
				
				// Close and dispose of resources
				reader.Close ();
				reader = null;
				command.Dispose();
				command = null;
			}
			return count;
		}

		/// <summary>
		/// Gets an event at a specific row id
		/// </summary>
		/// <returns>The event row</returns>
		/// <param name="rowId">Row id number</param>
		public EventRow GetEvent(int rowId) {
			List<EventRow> rows = QueryDatabase (SELECT_ROW + rowId);
			if (rows.Count == 1) {
				return rows[0];
			} else {
				return null;
			}
		}

		/// <summary>
		/// Gets all events in the database
		/// </summary>
		/// <returns>The list of rows</returns>
		public List<EventRow> GetAllEvents() {
			return QueryDatabase (SELECT_ALL);
		}

		/// <summary>
		/// Gets a descending range of events from the database.
		/// </summary>
		/// <returns>The list of rows within the range</returns>
		/// <param name="range">The amount of rows we want</param>
		public List<EventRow> GetDescEventRange(int range) {
			return QueryDatabase (SELECT_RANGE + range);
		}

		/// <summary>
		/// Queries the database.
		/// </summary>
		/// <returns>A list of rows returned from the database</returns>
		/// <param name="query">The SQL Query to execute</param>
		public List<EventRow> QueryDatabase(string query) {
			List<EventRow> rows = new List<EventRow>();
			if (IsDatabaseOpen ()) {
				try {
					IDbCommand command = dbConnection.CreateCommand ();
					command.CommandText = query;
					IDataReader reader = command.ExecuteReader ();

					// Add the row id and deserialized row data to the list
					while (reader.Read()) {
						int rowId = (int) reader[COLUMN_ID];
						Dictionary<string, object> payloadDict = Utils.DeserializeDictionary((byte[])reader[COLUMN_EVENT_DATA]);
						TrackerPayload payload = new TrackerPayload ();
						payload.AddDict (payloadDict);
						rows.Add (new EventRow(rowId, payload));
					}

					// Close and dispose of resources
					reader.Close ();
					reader = null;
					command.Dispose ();
					command = null;
				} catch (Exception e) {
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
		public bool IsDatabaseOpen() {
			return dbConnection != null && dbConnection.State == ConnectionState.Open;
		}
	}
}
