/*
 * Response.cs
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Globalization;
using Ionic.Zlib;
using UnityJSON;
using UnityHTTP.Exceptions;

namespace UnityHTTP {
	public class Response {

		public Request request;
		public int status = 200;
		public string message = "OK";
		public byte[] bytes;

		Dictionary<string, List<string>> headers = new Dictionary<string, List<string>> ();

		/// <summary>
		/// Initializes a new instance of the <see cref="UnityHTTP.Response"/> class.
		/// </summary>
		public Response () {}

		// --- Response return types.

		/// <summary>
		/// Gets the bytes as a UTF8 encoded string.
		/// </summary>
		/// <value>The text.</value>
		public string Text {
			get {
				if (bytes == null) {
					return "";
				}
				return System.Text.UTF8Encoding.UTF8.GetString (bytes);
			}
		}

		/// <summary>
		/// Gets the bytes as a Hashtable.
		/// </summary>
		/// <value>The object.</value>
		public Hashtable Object {
			get {
				if (bytes == null) {
					return null;
				}
				
				bool result = false;
				Hashtable obj = (Hashtable)JSON.JsonDecode( this.Text, ref result );
				if ( !result ) {
					obj = null;
				}
				return obj;
			}
		}
		
		/// <summary>
		/// Gets the bytes as an ArrayList.
		/// </summary>
		/// <value>The array.</value>
		public ArrayList Array {
			get {
				if ( bytes == null ) {
					return null;
				}
				
				bool result = false;
				ArrayList array = (ArrayList)JSON.JsonDecode( this.Text, ref result );
				if ( !result ) {
					array = null;
				}
				return array;
			}
		}

		// --- Response processing
		
		/// <summary>
		/// Adds a header.
		/// </summary>
		/// <param name="name">Name.</param>
		/// <param name="value">Value.</param>
		public void AddHeader (string name, string value) {
			name = name.ToLower ().Trim ();
			value = value.Trim ();
			if (!headers.ContainsKey (name))
				headers[name] = new List<string> ();
			headers[name].Add (value);
		}

		/// <summary>
		/// Gets the headers.
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
				return new List<string> ();
			return headers[name];
		}

		/// <summary>
		/// Gets a single header for a particular name.
		/// </summary>
		/// <returns>The header.</returns>
		/// <param name="name">Name.</param>
		public string GetHeader (string name) {
			name = name.ToLower ().Trim ();
			if (!headers.ContainsKey (name))
				return string.Empty;
			return headers[name][headers[name].Count - 1];
		}

		/// <summary>
		/// Reads the stream.
		/// </summary>
		/// <returns>The line.</returns>
		/// <param name="stream">Stream.</param>
		public string ReadLine (Stream stream) {
			var line = new List<byte> ();
			while (true) {
				int c = stream.ReadByte ();
				if (c == -1) {
					throw new HTTPException("Unterminated Stream Encountered.");
				}
				if ((byte)c == Request.EOL[1]) {
					break;
				} 
				line.Add ((byte)c);
			}
			var s = ASCIIEncoding.ASCII.GetString (line.ToArray ()).Trim ();
			return s;
		}

		/// <summary>
		/// Reads the key value.
		/// </summary>
		/// <returns>The key value.</returns>
		/// <param name="stream">Stream.</param>
		public string[] ReadKeyValue (Stream stream) {
			string line = ReadLine (stream);
			if (line == "") {
				return null;
			} else {
				var split = line.IndexOf (':');
				if (split == -1) {
					return null;
				}
				var parts = new string[2];
				parts[0] = line.Substring (0, split).Trim ();
				parts[1] = line.Substring (split + 1).Trim ();
				return parts;
			}
		}

		/// <summary>
		/// Reads from the input stream and procceses the response data.
		/// </summary>
		/// <param name="inputStream">Input stream.</param>
		public void ReadFromStream( Stream inputStream ) {
			var top = ReadLine (inputStream).Split (new char[] { ' ' });

			if (!int.TryParse (top [1], out status)) {
				throw new HTTPException ("Bad Status Code");
			}

			// MemoryStream is a disposable
			// http://stackoverflow.com/questions/234059/is-a-memory-leak-created-if-a-memorystream-in-net-is-not-closed
			using (var output = new MemoryStream ()) {
				message = string.Join (" ", top, 2, top.Length - 2);
				headers.Clear ();

				while (true) {
					// Collect Headers
					string[] parts = ReadKeyValue( inputStream );
					if ( parts == null )
						break;
					AddHeader( parts[ 0 ], parts[ 1 ] );
				}

				if ( GetHeader( "transfer-encoding" ) == "chunked" ) {
					while (true) {
						// Collect Body
						int length = int.Parse( ReadLine( inputStream ), NumberStyles.AllowHexSpecifier );

						if (length == 0) {
							break;
						}
						
						for (int i = 0; i < length; i++) {
							output.WriteByte( (byte)inputStream.ReadByte() );
						}

						//forget the CRLF.
						inputStream.ReadByte();
						inputStream.ReadByte();
					}

					while (true) {
						//Collect Trailers
						string[] parts = ReadKeyValue( inputStream );
						if (parts == null) {
							break;
						}
						AddHeader( parts[0], parts[1] );
					}
					
				} else {
					// Read Body
					int contentLength = 0;

					try {
						contentLength = int.Parse( GetHeader("content-length") );
					} catch {
						contentLength = 0;
					}

					int _b;
					while ((contentLength == 0 || output.Length < contentLength) && (_b = inputStream.ReadByte()) != -1) {
						output.WriteByte((byte)_b);
					}

					if (contentLength > 0 && output.Length != contentLength) {
						throw new HTTPException ("Response length does not match content length");
					}
				}

				if (GetHeader("content-encoding").Contains( "gzip" )) {
					bytes = UnZip( output );
				}
				else {
					bytes = output.ToArray();
				}
			}
		}

		/// <summary>
		/// Uns the zip.
		/// </summary>
		/// <returns>The Unzipped Byte Array</returns>
		/// <param name="output">A memory stream of output</param>
		public byte[] UnZip(MemoryStream output) {
			var cms = new MemoryStream ();
			output.Seek(0, SeekOrigin.Begin);
			using (var gz = new GZipStream(output, CompressionMode.Decompress)) {
				var buf = new byte[1024];
				int byteCount = 0;
				while ((byteCount = gz.Read(buf, 0, buf.Length)) > 0) {
					cms.Write(buf, 0, byteCount);
				}
			}
			return cms.ToArray();
		}
	}
}
