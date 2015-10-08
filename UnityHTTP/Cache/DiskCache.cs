/*
 * DiskCache.cs
 * UnityHTTP.Cache
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 * 
 * Authors: Andy Burke
 * Copyright: Copyright (c) 2015 andyburke
 * License: GPL
 */

using UnityEngine;
using System.Collections;
using System.IO;
using System;

namespace UnityHTTP.Cache {
	#if UNITY_WEBPLAYER
	public class DiskCache : MonoBehaviour {
		static DiskCache _instance = null;
		public static DiskCache Instance {
			get {
				if (_instance == null) {
					var g = new GameObject ("DiskCache", typeof(DiskCache));
					g.hideFlags = HideFlags.HideAndDontSave;
					_instance = g.GetComponent<DiskCache> ();
				}
				return _instance;
			}
		}
		
		public DiskCacheOperation Fetch (Request request) {
			var handle = new DiskCacheOperation ();
			handle.request = request;
			StartCoroutine (Download (request, handle));
			return handle;
		}
		
		IEnumerator Download(Request request, DiskCacheOperation handle) {
			request.Send ();
			while (!request.isDone)
				yield return new WaitForEndOfFrame ();
			handle.isDone = true;
		}
	}
	#else
	public class DiskCache : MonoBehaviour {
		string cachePath = null;
		
		static DiskCache _instance = null;
		public static DiskCache Instance {
			get {
				if (_instance == null) {
					var g = new GameObject ("DiskCache", typeof(DiskCache));
					g.hideFlags = HideFlags.HideAndDontSave;
					_instance = g.GetComponent<DiskCache> ();
				}
				return _instance;
			}
		}
		
		void Awake () {
			cachePath = System.IO.Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.LocalApplicationData), "uwcache");
			if (!Directory.Exists (cachePath))
				Directory.CreateDirectory (cachePath);
		}
		
		public DiskCacheOperation Fetch (Request request) {
			var guid = string.Empty;
			// MD5 is disposable
			// http://msdn.microsoft.com/en-us/library/system.security.cryptography.md5.aspx#3
			using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create ()) {
				foreach (var b in md5.ComputeHash (System.Text.ASCIIEncoding.ASCII.GetBytes (request.uri.ToString ()))) {
					guid = guid + b.ToString ("X2");
				}
			}
			
			var filename = System.IO.Path.Combine (cachePath, guid);
			if (File.Exists (filename) && File.Exists (filename + ".etag"))
				request.SetHeader ("If-None-Match", File.ReadAllText (filename + ".etag"));
			var handle = new DiskCacheOperation ();
			handle.request = request;
			StartCoroutine (DownloadAndSave (request, filename, handle));
			return handle;
		}
		
		IEnumerator DownloadAndSave (Request request, string filename, DiskCacheOperation handle) {
			var useCachedVersion = File.Exists(filename);
			Action< UnityHTTP.Request > callback = request.completedCallback;
			request.Send(); // will clear the completedCallback
			while (!request.isDone)
				yield return new WaitForEndOfFrame ();
			if (request.exception == null && request.response != null) {
				if (request.response.status == 200) {
					var etag = request.response.GetHeader ("etag");
					if (etag != string.Empty) {
						File.WriteAllBytes (filename, request.response.bytes);
						File.WriteAllText (filename + ".etag", etag);
					}
					useCachedVersion = false;
				}
			}
			
			if (useCachedVersion) {
				if(request.exception != null) {
					Debug.LogWarning("Using cached version due to exception:" + request.exception);
					request.exception = null;
				}
				request.response.status = 304;
				request.response.bytes = File.ReadAllBytes (filename);
				request.isDone = true;
			}
			handle.isDone = true;
			
			if ( callback != null ) {
				callback( request );
			}
		}
	}
	#endif
}
