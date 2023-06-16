/*
 * MobileScreenView.cs
 * SnowplowTracker.Events
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
using SnowplowTracker.Payloads;

namespace SnowplowTracker.Events
{
    /**
     * Screen view event (newer version of the `ScreenView` event class).
     */
    public class MobileScreenView : AbstractEvent<MobileScreenView> {
		
		private string name;
		private string id;
        private string type;
        private string previousName;
        private string previousId;
        private string previousType;
        private string transitionType;

        /// <summary>
        /// Create a screen view event with a generated a screen view ID.
        /// </summary>
        /// <param name="name">Name of the screen viewed.</param>
        public MobileScreenView(string name)
		{
            SetId(Utils.GetGUID());
            SetName(name);
		}

        /// <summary>
        /// Create a screen view event
        /// </summary>
        /// <param name="id">An ID from the associated screenview event.</param>
        /// <param name="name">Name of the screen viewed.</param>
        public MobileScreenView(string id, string name)
		{
            SetId(id);
            SetName(name);
		}

        /// <summary>
        /// Sets the name of the screen viewed.
        /// </summary>
        /// <returns>The event.</returns>
        /// <param name="name">Name of the screen viewed.</param>
        public MobileScreenView SetName(string name) {
            this.name = name;
			return this;
		}

        /// <summary>
        /// Gets the ID of the screen view event.
        /// </summary>
        /// <returns>The ID of the screen view.</returns>
        public string GetId()
        {
            return id;
        }

        /// <summary>
        /// Sets the ID from the associated screenview event.
        /// </summary>
        /// <returns>The event.</returns>
        /// <param name="id">An ID from the associated screenview event.</param>
        public MobileScreenView SetId(string id) {
			this.id = id;
			return this;
		}

        /// <summary>
        /// Sets the type of screen that was viewed e.g feed / carousel.
        /// </summary>
        /// <returns>The event.</returns>
        /// <param name="type">The type of screen that was viewed e.g feed / carousel.</param>
        public MobileScreenView SetType(string type)
        {
            this.type = type;
            return this;
        }

        /// <summary>
        /// Sets the name of the previous screen.
        /// </summary>
        /// <returns>The event.</returns>
        /// <param name="previousName">The name of the previous screen.</param>
        public MobileScreenView SetPreviousName(string previousName)
        {
            this.previousName = previousName;
            return this;
        }

        /// <summary>
        /// Sets the screenview ID of the previous screenview.
        /// </summary>
        /// <returns>The event.</returns>
        /// <param name="previousId">The screenview ID of the previous screenview.</param>
        public MobileScreenView SetPreviousId(string previousId)
        {
            this.previousId = previousId;
            return this;
        }

        /// <summary>
        /// Sets the screen type of the previous screenview.
        /// </summary>
        /// <returns>The event.</returns>
        /// <param name="previousType">The screen type of the previous screenview.</param>
        public MobileScreenView SetPreviousType(string previousType)
        {
            this.previousType = previousType;
            return this;
        }

        /// <summary>
        /// Sets the type of transition that led to the screen being viewed.
        /// </summary>
        /// <returns>The event.</returns>
        /// <param name="transitionType">The type of transition that led to the screen being viewed.</param>
        public MobileScreenView SetTransitionType(string transitionType)
        {
            this.transitionType = transitionType;
            return this;
        }

        public override MobileScreenView Self() {
			return this;
		}
		
		public override MobileScreenView Build()
        {
            Utils.CheckArgument (!String.IsNullOrEmpty(name), "Name cannot be null or empty.");
            Utils.CheckArgument (!String.IsNullOrEmpty(id), "Id cannot be null or empty.");
			return this;
		}
		
		// --- Interface Methods

		/// <summary>
		/// Gets the event payload.
		/// </summary>
		/// <returns>The event payload</returns>
		public override IPayload GetPayload() {
			TrackerPayload payload = new TrackerPayload();
			payload.Add (Constants.SV_NAME, name);
			payload.Add (Constants.SV_ID, id);
            if (!String.IsNullOrEmpty(type)) { payload.Add(Constants.SV_TYPE, type); }
            if (!String.IsNullOrEmpty(previousName)) { payload.Add(Constants.SV_PREVIOUS_NAME, previousName); }
            if (!String.IsNullOrEmpty(previousId)) { payload.Add(Constants.SV_PREVIOUS_ID, previousId); }
            if (!String.IsNullOrEmpty(previousType)) { payload.Add(Constants.SV_PREVIOUS_TYPE, previousType); }
            if (!String.IsNullOrEmpty(transitionType)) { payload.Add(Constants.SV_TRANSITION_TYPE, transitionType); }
            return new SelfDescribingJson (Constants.SCHEMA_MOBILE_SCREEN_VIEW, payload.GetDictionary());
		}
	}
}
