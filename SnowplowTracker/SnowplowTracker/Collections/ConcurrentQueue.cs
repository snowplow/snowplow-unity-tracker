/*
 * ConcurrentQueue.cs
 * SnowplowTracker.Collections
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
using System.Threading;
using System.Runtime.CompilerServices;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace SnowplowTracker.Collections {
	public class ConcurrentQueue<T> {

		private readonly Queue<T> q = new Queue<T>();

		/// <summary>
		/// Enqueue the specified item.
		/// </summary>
		/// <param name="item">Item.</param>
		public void Enqueue(T item) {
			lock (q) {
				q.Enqueue(item);
				Monitor.Pulse(q);
			}
		}

		/// <summary>
		/// Dequeue an item; will wait until an item is added before returning.
		/// </summary>
		public T Dequeue() {
			lock (q) {
				while (q.Count == 0) {
					Monitor.Wait(q);
				}
				return q.Dequeue();
			}
		}
	}
}
