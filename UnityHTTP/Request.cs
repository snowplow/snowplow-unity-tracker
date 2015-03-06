/*
 * Request.cs
 * UnityHTTP
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
using UnityHTTP.Enums;
using UnityHTTP.Exceptions;
using UnityHTTP.Cache;
using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Globalization;
using System.Threading;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace UnityHTTP {
	public class Request {
		
		public static bool LogAllRequests = false;
		public static bool VerboseLogging = false;
		
		public RequestType method;
		public string protocol = "HTTP/1.1";
		public Uri uri;
		public Stream byteStream;
		public static byte[] EOL = { (byte)'\r', (byte)'\n' };
		public Response response = null;
		public bool isDone = false;
		public int maximumRetryCount = 8;
		public bool acceptGzip = true;
		public bool useCache = false;
		public Exception exception = null;
		public RequestState state = RequestState.Waiting;
		public long responseTime = 0;
		public bool synchronous = false;
		public int bufferSize = 4 * 1024;
		
		public Action< UnityHTTP.Request > completedCallback = null;
		
		Dictionary<string, List<string>> headers = new Dictionary<string, List<string>> ();
		static Dictionary<string, string> etags = new Dictionary<string, string> ();

		/// <summary>
		/// Initializes a new instance of the <see cref="UnityHTTP.Request"/> class.
		/// </summary>
		/// <param name="method">Either GET or POST.</param>
		/// <param name="uri">The URI to send to.</param>
		public Request (RequestType method, string uri) {
			this.method = method;
			this.uri = new Uri (uri);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="UnityHTTP.Request"/> class.
		/// Assumed to be a POST due to the byte[] being included.
		/// </summary>
		/// <param name="method">Method.</param>
		/// <param name="uri">URI.</param>
		/// <param name="bytes">Bytes.</param>
		public Request (RequestType method, string uri, byte[] bytes) {
			this.method = method;
			this.uri = new Uri (uri);
			this.byteStream = new MemoryStream(bytes);
		}

		/// <summary>
		/// Adds a header to the HTTP Request.
		/// </summary>
		/// <param name="name">Name of the header.</param>
		/// <param name="value">The header value.</param>
		public void AddHeader (string name, string value) {
			name = name.ToLower ().Trim ();
			value = value.Trim ();
			if (!headers.ContainsKey (name))
				headers[name] = new List<string> ();
			headers[name].Add (value);
		}

		/// <summary>
		/// Gets a particular headers value.
		/// </summary>
		/// <returns>The header to try and get.</returns>
		/// <param name="name">Name.</param>
		public string GetHeader (string name) {
			name = name.ToLower ().Trim ();
			if (!headers.ContainsKey (name))
				return "";
			return headers[name][0];
		}

		/// <summary>
		/// Gets the headers and their values.
		/// </summary>
		/// <returns>The headers.</returns>
		public List< string > GetHeaders() {
			List< string > result = new List< string >();
			foreach (string name in headers.Keys) {
				foreach (string value in headers[name]) {
					result.Add( name + ": " + value );
				}
			}
			return result;
		}

		/// <summary>
		/// Gets the headers for a particular header name.
		/// </summary>
		/// <returns>The headers.</returns>
		/// <param name="name">Name.</param>
		public List<string> GetHeaders (string name) {
			name = name.ToLower ().Trim ();
			if (!headers.ContainsKey (name))
				headers[name] = new List<string> ();
			return headers[name];
		}

		/// <summary>
		/// Sets a new header into the header list.
		/// </summary>
		/// <param name="name">Name.</param>
		/// <param name="value">Value.</param>
		public void SetHeader (string name, string value) {
			name = name.ToLower ().Trim ();
			value = value.Trim ();
			if (!headers.ContainsKey (name))
				headers[name] = new List<string> ();
			headers[name].Clear ();
			headers[name].TrimExcess();
			headers[name].Add (value);
		}

		/// <summary>
		/// Responsible for sending the request and processing the response.
		/// </summary>
		private void GetResponse() {
			System.Diagnostics.Stopwatch curcall = new System.Diagnostics.Stopwatch();
			curcall.Start();

			try {
				var retry = 0;
				while (++retry < maximumRetryCount) {
					if (useCache) {
						string etag = "";
						if (etags.TryGetValue (uri.AbsoluteUri, out etag)) {
							SetHeader ("If-None-Match", etag);
						}
					}
					
					SetHeader ("Host", uri.Host);
					
					var client = new TcpClient ();
					client.Connect (uri.Host, uri.Port);
					using (var stream = client.GetStream ()) {
						var ostream = stream as Stream;
						if (uri.Scheme.ToLower() == "https") {
							ostream = new SslStream (stream, false, new RemoteCertificateValidationCallback (ValidateServerCertificate));
							try {
								var ssl = ostream as SslStream;
								ssl.AuthenticateAsClient (uri.Host);
							} catch (Exception e) {
								Debug.LogError ("Exception: " + e.Message);
								return;
							}
						}
						WriteToStream( ostream );
						response = new Response ();
						response.request = this;
						state = RequestState.Reading;
						response.ReadFromStream( ostream );
					}
					client.Close ();
					
					switch (response.status) {
						case 307:
						case 302:
						case 301:
							uri = new Uri (response.GetHeader ("Location"));
							continue;
						default:
							retry = maximumRetryCount;
							break;
					}
				}
				if (useCache) {
					string etag = response.GetHeader ("etag");
					if (etag.Length > 0) {
						etags[uri.AbsoluteUri] = etag;
					}
				}
			} catch (Exception e) {
#if !UNITY_EDITOR
				Console.WriteLine ("Unhandled Exception, aborting request.");
				Console.WriteLine (e);
#else
				Debug.LogError("Unhandled Exception, aborting request.");
				Debug.LogException(e);
#endif
				exception = e;
				response = null;
			}
			
			state = RequestState.Done;
			isDone = true;
			responseTime = curcall.ElapsedMilliseconds;
			
			if ( byteStream != null ) {
				byteStream.Close();
			}
			
			if ( completedCallback != null ) {
				completedCallback(this);
			}
			
			if ( LogAllRequests ) {
#if !UNITY_EDITOR
				System.Console.WriteLine("NET: " + InfoString( VerboseLogging ));
#else
				if ( response != null && response.status >= 200 && response.status < 300 ) {
					Debug.Log( InfoString( VerboseLogging ) );
				} else if ( response != null && response.status >= 400 ) {
					Debug.LogError( InfoString( VerboseLogging ) );
				} else {
					Debug.LogWarning( InfoString( VerboseLogging ) );
				}
#endif
			}
		}

		/// <summary>
		/// Send the HTTP Request with the options set.
		/// </summary>
		/// <param name="callback">Callback.</param>
		public virtual void Send( Action< UnityHTTP.Request > callback = null) {
			completedCallback = callback;
			
			isDone = false;
			state = RequestState.Waiting;
			if ( acceptGzip ) {
				SetHeader ("Accept-Encoding", "gzip");
			}
			
			if ( byteStream != null && byteStream.Length > 0 && GetHeader ("Content-Length") == "" ) {
				SetHeader( "Content-Length", byteStream.Length.ToString() );
			}
			
			if ( GetHeader( "User-Agent" ) == "" ) {
				SetHeader( "User-Agent", "UnityWeb/1.0" );
			}
			
			if ( GetHeader( "Connection" ) == "" ) {
				SetHeader( "Connection", "close" );
			}
			
			// Basic Authorization
			if (!String.IsNullOrEmpty(uri.UserInfo)) {	
				SetHeader("Authorization", "Basic " + System.Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(uri.UserInfo)));
			}
			
			if (synchronous) {
				GetResponse();
			} else {
				ThreadPool.QueueUserWorkItem (new WaitCallback ( delegate(object t) {
					GetResponse();
				})); 
			}
		}
		
		public string Text {
			set { byteStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes (value)); }
		}

		/// <summary>
		/// Validates the server certificate.
		/// </summary>
		/// <returns><c>true</c>, if server certificate was validated, <c>false</c> otherwise.</returns>
		/// <param name="sender">Sender.</param>
		/// <param name="certificate">Certificate.</param>
		/// <param name="chain">Chain.</param>
		/// <param name="sslPolicyErrors">Ssl policy errors.</param>
		public static bool ValidateServerCertificate (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) {
#if !UNITY_EDITOR
			System.Console.WriteLine( "NET: SSL Cert: " + sslPolicyErrors.ToString() );
#else
			Debug.LogWarning("SSL Cert Error: " + sslPolicyErrors.ToString ());
#endif
			return true;
		}

		/// <summary>
		/// Writes the HTTP Request to the output stream.
		/// </summary>
		/// <param name="outputStream">Output stream.</param>
		void WriteToStream( Stream outputStream ) {
			var stream = new BinaryWriter( outputStream );
			stream.Write( ASCIIEncoding.ASCII.GetBytes( method.Value + " " + uri.PathAndQuery + " " + protocol ) );
			stream.Write( EOL );
			
			foreach (string name in headers.Keys) {
				foreach (string value in headers[name]) {
					stream.Write( ASCIIEncoding.ASCII.GetBytes( name ) );
					stream.Write( ':');
					stream.Write( ASCIIEncoding.ASCII.GetBytes( value ) );
					stream.Write( EOL );
				}
			}
			
			stream.Write( EOL );
			
			if (byteStream == null) {
				return;
			}
			
			long numBytesToRead = byteStream.Length;
			byte[] buffer = new byte[bufferSize];
			while (numBytesToRead > 0) {
				int readed = byteStream.Read(buffer, 0, bufferSize);
				stream.Write(buffer, 0, readed);
				numBytesToRead -= readed;
			}
		}

		/// <summary>
		/// Gets information about the request and returns an Information String.
		/// </summary>
		/// <returns>The reponse information.</returns>
		/// <param name="verbose">If set to <c>true</c> verbose.</param>
		private static string[] sizes = { "B", "KB", "MB", "GB" };
		public string InfoString( bool verbose ) {
			string status = isDone && response != null ? response.status.ToString() : "---";
			string message = isDone && response != null ? response.message : "Unknown";
			double size = isDone && response != null && response.bytes != null ? response.bytes.Length : 0.0f;
			
			int order = 0;
			while ( size >= 1024.0f && order + 1 < sizes.Length ) {
				++order;
				size /= 1024.0f;
			}
			
			string sizeString = String.Format( "{0:0.##}{1}", size, sizes[ order ] );
			
			string result = uri.ToString() + " [ " + method.Value + " ] [ " + status + " " + message + " ] [ " + sizeString + " ] [ " + responseTime + "ms ]";
			
			if ( verbose && response != null ) {
				result += "\n\nRequest Headers:\n\n" + String.Join( "\n", GetHeaders().ToArray() );
				result += "\n\nResponse Headers:\n\n" + String.Join( "\n", response.GetHeaders().ToArray() );
				
				if ( response.Text != null ) {
					result += "\n\nResponse Body:\n" + response.Text;
				}
			}
			return result;
		}
	}
}
