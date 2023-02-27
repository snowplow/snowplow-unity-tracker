/*
 * DevicePlatforms.cs
 * SnowplowTracker.Enums
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

namespace SnowplowTracker.Enums
{
    public class DevicePlatforms {

		public string Value { get; set; }

		private DevicePlatforms (string value) { 
			Value = value; 
		}

		public static DevicePlatforms Web              { get { return new DevicePlatforms("web"); }}
		public static DevicePlatforms Mobile           { get { return new DevicePlatforms("mob"); }}
		public static DevicePlatforms Desktop          { get { return new DevicePlatforms("pc"); }}
		public static DevicePlatforms ServerSideApp    { get { return new DevicePlatforms("srv"); }}
		public static DevicePlatforms General          { get { return new DevicePlatforms("app"); }}
		public static DevicePlatforms ConnectedTV      { get { return new DevicePlatforms("tv"); }}
		public static DevicePlatforms GameConsole      { get { return new DevicePlatforms("cnsl"); }}
		public static DevicePlatforms InternetOfThings { get { return new DevicePlatforms("iot"); }}
	}
}
