/*
 * Log.cs
 * SnowplowTracker
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
using UnityEngine;

namespace SnowplowTracker {
	public class Log : MonoBehaviour {

		private static int level = 2;
		private static bool logging = true;

		/// <summary>
		/// Writes a line either to the console or to the Unity debugger.
		/// </summary>
		/// <param name="message">Message to be logged.</param>
		public static void Error(string message) {
			if (logging && level >= 1) {
#if UNITY_EDITOR
				UnityEngine.Debug.LogError(message);
#else
				Console.Error.WriteLine(message);
#endif
			}
		}

		/// <summary>
		/// Writes a line either to the console or to the Unity debugger.
		/// </summary>
		/// <param name="message">Message to be logged.</param>
		public static void Debug(string message) {
			if (logging && level >= 2) {
#if UNITY_EDITOR
				UnityEngine.Debug.Log(message);
#else
				Console.WriteLine(message);
#endif
			}
		}

		/// <summary>
		/// Writes a line either to the console or to the Unity debugger.
		/// </summary>
		/// <param name="message">Message to be logged.</param>
		public static void Verbose(string message) {
			if (logging && level >= 3) {
#if UNITY_EDITOR
				UnityEngine.Debug.Log(message);
#else
				Console.WriteLine(message);
#endif
			}
		}

		/// <summary>
		/// Turns logging on.
		/// </summary>
		public static void On() {
			logging = true;
		}

		/// <summary>
		/// Turns logging off.
		/// </summary>
		public static void Off() {
			logging = false;
		}

		/// <summary>
		/// Sets the log level.
		/// </summary>
		/// <param name="newLevel">New level.</param>
		public static void SetLogLevel(int newLevel) {
			level = newLevel;
		}

        internal static void Debug(object p)
        {
            throw new NotImplementedException();
        }
    }
}
