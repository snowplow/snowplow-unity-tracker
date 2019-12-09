/*
 * Tracker.cs
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
 * Authors: Joshua Beemster, Paul Boocock
 * Copyright: Copyright (c) 2015-2019 Snowplow Analytics Ltd
 * License: Apache License Version 2.0
 */

using System;
using System.Threading;
using System.Collections.Generic;
using SnowplowTracker.Emitters;
using SnowplowTracker.Payloads;
using SnowplowTracker.Payloads.Contexts;
using SnowplowTracker.Events;
using SnowplowTracker.Enums;

namespace SnowplowTracker
{
    public class Tracker
    {

        private IEmitter emitter;
        private Subject subject;
        private Session session;
        private string trackerNamespace;
        private string appId;
        private DevicePlatforms platform;
        private bool base64Encoded;

        private bool dataCollection = false;
        private bool synchronous;

        /// <summary>
        /// Initializes a new instance of the <see cref="SnowplowTracker.Tracker"/> class.
        /// </summary>
        /// <param name="emitter">Emitter used for all sending and storing operations.</param>
        /// <param name="trackerNamespace">Tracker namespace.</param>
        /// <param name="appId">App identifier.</param>
        /// <param name="platform">The DevicePlatform the tracker is running on.</param>
        /// <param name="base64Encoded">If set to <c>true</c> all unstructured events and contextes will be base64 encoded.</param>
        public Tracker(IEmitter emitter, string trackerNamespace, string appId, Subject subject = null, Session session = null, DevicePlatforms platform = null,
                       bool base64Encoded = true)
        {
            // Preconditions
            Utils.CheckArgument(emitter != null, "Emitter cannot be null.");

            // Tracker Setup
            this.emitter = emitter;
            this.trackerNamespace = trackerNamespace;
            this.appId = appId;
            this.platform = (platform != null) ? platform : DevicePlatforms.Mobile;
            this.base64Encoded = base64Encoded;
            this.subject = subject;
            this.session = session;

            // Set Synchronous or Asynchronous operation
            if (typeof(AsyncEmitter) == emitter.GetType())
            {
                synchronous = false;
            }
            else
            {
                synchronous = true;
            }
        }

        // --- Tracker Control Methods

        /// <summary>
        /// Starts all background services for processing events:
        /// - Turns on dataCollection which allows events to be tracked
        /// - Turns on the emitter loop which will send all events
        /// - Turns on the session checker
        /// </summary>
        public void StartEventTracking()
        {
            dataCollection = true;
            emitter.Start();
            if (session != null)
            {
                session.StartChecker();
            }
        }

        /// <summary>
        /// Stops all event tracking and background services:
        /// - Turns of dataCollection
        /// - Stops the emitter loop thread
        /// - Stops the event consumer thread
        /// - Stops the session checking timer
        /// </summary>
        public void StopEventTracking()
        {
            dataCollection = false;
            emitter.Stop();
            if (session != null)
            {
                session.StopChecker();
            }
        }

        // --- Event Tracking

        /// <summary>
        /// Track the specified event.
        /// - Checks whether dataCollection is on
        /// - Checks whether to use synchronous sending or not
        /// </summary>
        /// <param name="newEvent">New event to track.</param>
        public void Track(IEvent newEvent)
        {
            if (!dataCollection)
            {
                return;
            }
            if (synchronous)
            {
                ProcessEvent(newEvent);
            }
            else
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(delegate (object t)
                {
                    ProcessEvent(newEvent);
                }));
            }
        }

        /// <summary>
        /// Processes the event.
        /// </summary>
        /// <param name="newEvent">New event.</param>
        private void ProcessEvent(IEvent newEvent)
        {
            List<IContext> contexts = newEvent.GetContexts();
            string eventId = newEvent.GetEventId();
            Type eType = newEvent.GetType();

            if (eType == typeof(PageView) || eType == typeof(Structured))
            {
                AddTrackerPayload((TrackerPayload)newEvent.GetPayload(), contexts, eventId);
            }
            else if (eType == typeof(EcommerceTransaction))
            {
                AddTrackerPayload((TrackerPayload)newEvent.GetPayload(), contexts, eventId);
                EcommerceTransaction ecommerceTransaction = (EcommerceTransaction)newEvent;
                foreach (EcommerceTransactionItem item in ecommerceTransaction.GetItems())
                {
                    item.SetItemId(ecommerceTransaction.GetOrderId());
                    item.SetCurrency(ecommerceTransaction.GetCurrency());
                    item.SetTimestamp(ecommerceTransaction.GetTimestamp());
                    AddTrackerPayload((TrackerPayload)item.GetPayload(), item.GetContexts(), item.GetEventId());
                }
            }
            else if (eType == typeof(Unstructured))
            {
                Unstructured unstruct = (Unstructured)newEvent;
                unstruct.SetBase64Encode(this.base64Encoded);
                AddTrackerPayload((TrackerPayload)unstruct.GetPayload(), contexts, eventId);
            }
            else if (eType == typeof(Timing) || eType == typeof(ScreenView))
            {
                this.ProcessEvent(new Unstructured()
                           .SetEventData((SelfDescribingJson)newEvent.GetPayload())
                           .SetCustomContext(newEvent.GetContexts())
                           .SetTimestamp(newEvent.GetTimestamp())
                           .SetEventId(newEvent.GetEventId())
                           .Build());
            }
        }

        /// <summary>
        /// Adds the tracker payload to the emitter.
        /// </summary>
        /// <param name="payload">The base event payload.</param>
        /// <param name="contexts">The list of contexts from the event.</param>
        private void AddTrackerPayload(TrackerPayload payload, List<IContext> contexts, string eventId)
        {

            // Add default parameters to the payload
            payload.Add(Constants.PLATFORM, this.platform.Value);
            payload.Add(Constants.APP_ID, this.appId);
            payload.Add(Constants.NAMESPACE, this.trackerNamespace);
            payload.Add(Constants.TRACKER_VERSION, Version.VERSION);

            // Add the subject data if available
            if (subject != null)
            {
                payload.AddDict(subject.GetPayload().GetDictionary());
            }

            // Add the session context if available
            if (session != null)
            {
                contexts.Add(session.GetSessionContext(eventId));
            }

            // Build the final context and add it to the payload
            if (contexts != null && contexts.Count > 0)
            {
                SelfDescribingJson envelope = GetFinalContext(contexts);
                payload.AddJson(envelope.GetDictionary(), this.base64Encoded, Constants.CONTEXT_ENCODED, Constants.CONTEXT);
            }

            Log.Verbose("Tracker: Sending event to the emitter.");
            Log.Verbose(" + Event: " + payload.ToString());

            // Add the event to the emitter.
            emitter.Add(payload);
        }

        /// <summary>
        /// Gets the final context.
        /// </summary>
        /// <returns>The final context.</returns>
        /// <param name="contexts">A list of contexts to nest into the final context</param>
        private SelfDescribingJson GetFinalContext(List<IContext> contexts)
        {
            List<Dictionary<string, object>> contextDicts = new List<Dictionary<string, object>>();
            foreach (IContext context in contexts)
            {
                contextDicts.Add(context.GetJson().GetDictionary());
            }
            return new SelfDescribingJson(Constants.SCHEMA_CONTEXTS, contextDicts);
        }

        // --- Setters

        /// <summary>
        /// Sets the emitter. NOTE this is a blocking function as the emitter will not be replaced until it is no longer
        /// sending events.
        /// </summary>
        /// <param name="emitter">Emitter.</param>
        public void SetEmitter(IEmitter emitter)
        {
            if (emitter != null)
            {
                // Prevents the emitter from looping
                this.emitter.Stop();

                // Wait for the current loop to finish
                while (emitter.IsSending())
                {
                    Thread.Sleep(5);
                }

                // Switch emitters over and start the new one
                this.emitter = emitter;
                this.emitter.Start();
            }
        }

        /// <summary>
        /// Sets the subject.
        /// </summary>
        /// <param name="subject">Subject.</param>
        public void SetSubject(Subject subject)
        {
            this.subject = subject;
        }

        /// <summary>
        /// Sets the session.
        /// </summary>
        /// <param name="session">Session.</param>
        public void SetSession(Session session)
        {
            if (this.session != null)
            {
                this.session.StopChecker();
            }
            this.session = session;
        }

        /// <summary>
        /// Sets the tracker namespace.
        /// </summary>
        /// <param name="trackerNamespace">Tracker namespace.</param>
        public void SetTrackerNamespace(string trackerNamespace)
        {
            this.trackerNamespace = trackerNamespace;
        }

        /// <summary>
        /// Sets the app identifier.
        /// </summary>
        /// <param name="appId">App identifier.</param>
        public void SetAppId(string appId)
        {
            this.appId = appId;
        }

        /// <summary>
        /// Sets the platform.
        /// </summary>
        /// <param name="platform">Platform.</param>
        public void SetPlatform(DevicePlatforms platform)
        {
            this.platform = platform;
        }

        /// <summary>
        /// Sets the base64 encoded truth.
        /// </summary>
        /// <param name="base64Encoded">If set to <c>true</c> base64 encoded.</param>
        public void SetBase64Encoded(bool base64Encoded)
        {
            this.base64Encoded = base64Encoded;
        }

        // --- Getters

        /// <summary>
        /// Gets the emitter.
        /// </summary>
        /// <returns>The emitter.</returns>
        public IEmitter GetEmitter()
        {
            return this.emitter;
        }

        /// <summary>
        /// Gets the subject object.
        /// </summary>
        /// <returns>The subject.</returns>
        public Subject GetSubject()
        {
            return this.subject;
        }

        /// <summary>
        /// Gets the session object.
        /// </summary>
        /// <returns>The session.</returns>
        public Session GetSession()
        {
            return this.session;
        }

        /// <summary>
        /// Gets the tracker namespace.
        /// </summary>
        /// <returns>The tracker namespace.</returns>
        public string GetTrackerNamespace()
        {
            return this.trackerNamespace;
        }

        /// <summary>
        /// Gets the app identifier.
        /// </summary>
        /// <returns>The app identifier.</returns>
        public string GetAppId()
        {
            return this.appId;
        }

        /// <summary>
        /// Gets the platform.
        /// </summary>
        /// <returns>The platform.</returns>
        public DevicePlatforms GetPlatform()
        {
            return this.platform;
        }

        /// <summary>
        /// Gets the base64 encoded value.
        /// </summary>
        /// <returns><c>true</c>, if base64 encoded is true <c>false</c> otherwise.</returns>
        public bool GetBase64Encoded()
        {
            return this.base64Encoded;
        }
    }
}
