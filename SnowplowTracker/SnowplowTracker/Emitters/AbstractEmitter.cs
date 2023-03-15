/*
 * AbstractEmitter.cs
 * SnowplowTracker.Emitters
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
using System.Collections.Generic;
using SnowplowTracker.Payloads;
using SnowplowTracker.Enums;
using SnowplowTracker.Storage;
using SnowplowTracker.Collections;
using SnowplowTracker.Requests;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SnowplowTracker.Emitters
{
    public abstract class AbstractEmitter : IEmitter
    {
        protected int POST_WRAPPER_BYTES = 88; // "schema":"iglu:com.snowplowanalytics.snowplow/payload_data/jsonschema/1-0-3","data":[]
        protected int POST_STM_BYTES = 22;     // "stm":"1443452851000",
        protected int FAIL_INTERVAL = 10000;  // If all events failed to send

        protected string endpoint;
        protected Uri collectorUri;
        protected HttpProtocol httpProtocol;
        protected Enums.HttpMethod httpMethod;
        protected int sendLimit;
        protected long byteLimitGet;
        protected long byteLimitPost;
        protected IStore eventStore;

        /// <summary>
        /// Adds an event payload to the database.
        /// </summary>
        /// <param name="payload">Payload.</param>
        public abstract void Add(TrackerPayload payload);

        /// <summary>
        /// Starts the Emitter and the Event Consumer.
        /// - EmitLoop will send all events in the database and then
        ///   wait for the event consumer to signal that there is work.
        /// - The event consumer waits for its queue to be signalled and
        ///   will then add an event to the database; after which it 
        ///   signals the emitloop of work to be done.
        /// </summary>
        public abstract void Start();

        /// <summary>
        /// Stops the Emitter and the Event Consumer.
        /// </summary>
        public abstract void Stop();

        /// <summary>
        /// Gets whether the emitter is currently sending.
        /// </summary>
        /// <returns><c>true</c>, if emitter is sending, <c>false</c> otherwise.</returns>
        public abstract bool IsSending();

        // --- Event Senders

        /// <summary>
        /// From a range of event rows will construct a list of RequestResult objects
        /// </summary>
        /// <returns>The results of sending all requests</returns>
        /// <param name="eventRows">Event rows from the database</param>
        protected List<RequestResult> SendRequests(List<EventRow> eventRows)
        {
            ConcurrentQueue<RequestResult> resultQueue = new ConcurrentQueue<RequestResult>();
            int count;

            if (httpMethod == Enums.HttpMethod.GET)
            {
                count = HttpGet(eventRows, resultQueue);
            }
            else
            {
                count = HttpPost(eventRows, resultQueue);
            }

            // Wait for the results of each request
            List<RequestResult> results = new List<RequestResult>();
            while (count != 0)
            {
                results.Add(resultQueue.Dequeue());
                count--;
            }

            return results;
        }

        protected async Task<List<RequestResult>> SendRequestsAsync(List<EventRow> eventRows)
        {
            ConcurrentQueue<RequestResult> resultQueue = new ConcurrentQueue<RequestResult>();
            int count = 0;
            if (httpMethod == Enums.HttpMethod.GET)
            {
                count = await HttpGetAsync(eventRows, resultQueue);
            }
            else
            {
                count = await HttpPostAsync(eventRows, resultQueue);
            }

            List<RequestResult> results = new List<RequestResult>();
            // Since the queue is awaited, we can directly dequeue it here
            for (int i = 0; i < count; i++)
            {
                results.Add(resultQueue.Dequeue());
            }

            return results;
        }

        /// <summary>
        /// Sends all events as GET requests on background threads
        /// </summary>
        /// <returns>The get.</returns>
        /// <param name="eventRows">Event rows.</param>
        protected int HttpGet(List<EventRow> eventRows, ConcurrentQueue<RequestResult> resultQueue)
        {
            int count = eventRows.Count;
            try
            {
                // Send each row as an individual GET Request
                foreach (EventRow eRow in eventRows)
                {
                    TrackerPayload payload = eRow.GetPayload();
                    long byteSize = payload.GetByteSize() + POST_STM_BYTES;
                    bool oversize = byteSize > byteLimitGet;
                    Log.Debug("Emitter: Sending GET with byte-size: " + byteSize);
                    new ReadyRequest(
                        new HttpRequest(Enums.HttpMethod.GET, GetGETRequest(payload.GetDictionary())),
                        new List<Guid> { eRow.GetRowId() },
                        oversize,
                        resultQueue
                    ).Send();
                }
            }
            catch (Exception e)
            {
                Log.Debug("Emitter: caught exception in HTTPGet request: " + e.Message);
                Log.Debug("Emitter: HTTPGet exception trace: " + e.StackTrace);
            }
            return count;
        }
        
        /// <summary>
        /// Sends all events as GET requests asynchronously
        /// </summary>
        /// <returns>The get.</returns>
        /// <param name="eventRows">Event rows.</param>
        protected async Task<int> HttpGetAsync(List<EventRow> eventRows, ConcurrentQueue<RequestResult> resultQueue)
        {
            int count = eventRows.Count;
            try
            {
                // Send each row as an individual GET Request
                foreach (EventRow eRow in eventRows)
                {
                    TrackerPayload payload = eRow.GetPayload();
                    long byteSize = payload.GetByteSize() + POST_STM_BYTES;
                    bool oversize = byteSize > byteLimitGet;
                    Log.Debug("Emitter: Sending GET with byte-size: " + byteSize);
                    await new ReadyRequest(
                        new HttpRequest(Enums.HttpMethod.GET, GetGETRequest(payload.GetDictionary())),
                        new List<Guid> { eRow.GetRowId() },
                        oversize,
                        resultQueue
                    ).SendAsync();
                }
            }
            catch (Exception e)
            {
                Log.Debug("Emitter: caught exception in HTTPGet request: " + e.Message);
                Log.Debug("Emitter: HTTPGet exception trace: " + e.StackTrace);
            }
            return count;
        }

        /// <summary>
        /// Send all event rows as POST requests on background threads
        /// </summary>
        /// <returns>The results of all the requests</returns>
        /// <param name="eventRows">Event rows.</param>
        protected int HttpPost(List<EventRow> eventRows, ConcurrentQueue<RequestResult> resultQueue)
        {
            int count = 0;

            List<Guid> rowIds = new List<Guid>();
            List<Dictionary<string, object>> payloadDicts = new List<Dictionary<string, object>>();
            long totalByteSize = 0;
            try
            {

                for (int i = 0; i < eventRows.Count; i++)
                {
                    TrackerPayload payload = eventRows[i].GetPayload();
                    long payloadByteSize = payload.GetByteSize() + POST_STM_BYTES;

                    if ((payloadByteSize + POST_WRAPPER_BYTES) > byteLimitPost)
                    {
                        // A single Payload has exceeded the Byte Limit
                        Log.Debug("Emitter: Single event exceeds byte limit: " + (payloadByteSize + POST_WRAPPER_BYTES) + " is > " + byteLimitPost);
                        Log.Debug("Sending POST with byte-size: " + (payloadByteSize + POST_WRAPPER_BYTES));
                        List<Dictionary<string, object>> singlePayloadPost = new List<Dictionary<string, object>> { payload.GetDictionary() };
                        List<Guid> singlePayloadId = new List<Guid> { eventRows[i].GetRowId() };
                        new ReadyRequest(new HttpRequest(Enums.HttpMethod.POST, collectorUri, GetPOSTRequest(singlePayloadPost)), singlePayloadId, true, resultQueue).Send();
                        count++;
                    }
                    else if ((totalByteSize + payloadByteSize + POST_WRAPPER_BYTES + (payloadDicts.Count - 1)) > byteLimitPost)
                    {
                        Log.Debug("Emitter: Byte limit reached: " + (totalByteSize + payloadByteSize + POST_WRAPPER_BYTES + (payloadDicts.Count - 1)) +
                                   " is > " + byteLimitPost);
                        Log.Debug("Emitter: Sending POST with byte-size: " + (totalByteSize + POST_WRAPPER_BYTES + (payloadDicts.Count - 1)));
                        new ReadyRequest(new HttpRequest(Enums.HttpMethod.POST, collectorUri, GetPOSTRequest(payloadDicts)), rowIds, false, resultQueue).Send();
                        count++;

                        // Reset collections
                        payloadDicts = new List<Dictionary<string, object>> { payload.GetDictionary() };
                        rowIds = new List<Guid> { eventRows[i].GetRowId() };
                        totalByteSize = payloadByteSize;
                    }
                    else
                    {
                        payloadDicts.Add(payload.GetDictionary());
                        rowIds.Add(eventRows[i].GetRowId());
                        totalByteSize += payloadByteSize;
                    }
                }

                if (payloadDicts.Count > 0)
                {
                    Log.Debug("Emitter: Sending POST with byte-size: " + (totalByteSize + POST_WRAPPER_BYTES + (payloadDicts.Count - 1)));
                    new ReadyRequest(new HttpRequest(Enums.HttpMethod.POST, collectorUri, GetPOSTRequest(payloadDicts)), rowIds, false, resultQueue).Send();
                    count++;
                }
            }
            catch (Exception e)
            {
                Log.Debug("Emitter: caught exception in HTTPPost request: " + e.Message);
                Log.Debug("Emitter: HTTPPost exception trace: " + e.StackTrace);
            }
            return count;
        }
        
        /// <summary>
        /// Send all event rows as POST requests asynchronously
        /// </summary>
        /// <returns>The results of all the requests</returns>
        /// <param name="eventRows">Event rows.</param>
        protected async Task<int> HttpPostAsync(List<EventRow> eventRows, ConcurrentQueue<RequestResult> resultQueue)
        {
            int count = 0;

            List<Guid> rowIds = new List<Guid>();
            List<Dictionary<string, object>> payloadDicts = new List<Dictionary<string, object>>();
            long totalByteSize = 0;
            try
            {

                for (int i = 0; i < eventRows.Count; i++)
                {
                    TrackerPayload payload = eventRows[i].GetPayload();
                    long payloadByteSize = payload.GetByteSize() + POST_STM_BYTES;

                    if ((payloadByteSize + POST_WRAPPER_BYTES) > byteLimitPost)
                    {
                        // A single Payload has exceeded the Byte Limit
                        Log.Debug("Emitter: Single event exceeds byte limit: " + (payloadByteSize + POST_WRAPPER_BYTES) + " is > " + byteLimitPost);
                        Log.Debug("Sending POST with byte-size: " + (payloadByteSize + POST_WRAPPER_BYTES));
                        List<Dictionary<string, object>> singlePayloadPost = new List<Dictionary<string, object>> { payload.GetDictionary() };
                        List<Guid> singlePayloadId = new List<Guid> { eventRows[i].GetRowId() };
                        await new ReadyRequest(new HttpRequest(Enums.HttpMethod.POST, collectorUri, GetPOSTRequest(singlePayloadPost)), singlePayloadId, true, resultQueue).SendAsync();
                        count++;
                    }
                    else if ((totalByteSize + payloadByteSize + POST_WRAPPER_BYTES + (payloadDicts.Count - 1)) > byteLimitPost)
                    {
                        Log.Debug("Emitter: Byte limit reached: " + (totalByteSize + payloadByteSize + POST_WRAPPER_BYTES + (payloadDicts.Count - 1)) +
                                   " is > " + byteLimitPost);
                        Log.Debug("Emitter: Sending POST with byte-size: " + (totalByteSize + POST_WRAPPER_BYTES + (payloadDicts.Count - 1)));
                        await new ReadyRequest(new HttpRequest(Enums.HttpMethod.POST, collectorUri, GetPOSTRequest(payloadDicts)), rowIds, false, resultQueue).SendAsync();
                        count++;

                        // Reset collections
                        payloadDicts = new List<Dictionary<string, object>> { payload.GetDictionary() };
                        rowIds = new List<Guid> { eventRows[i].GetRowId() };
                        totalByteSize = payloadByteSize;
                    }
                    else
                    {
                        payloadDicts.Add(payload.GetDictionary());
                        rowIds.Add(eventRows[i].GetRowId());
                        totalByteSize += payloadByteSize;
                    }
                }

                if (payloadDicts.Count > 0)
                {
                    Log.Debug("Emitter: Sending POST with byte-size: " + (totalByteSize + POST_WRAPPER_BYTES + (payloadDicts.Count - 1)));
                    await new ReadyRequest(new HttpRequest(Enums.HttpMethod.POST, collectorUri, GetPOSTRequest(payloadDicts)), rowIds, false, resultQueue).SendAsync();
                    count++;
                }
            }
            catch (Exception e)
            {
                Log.Debug("Emitter: caught exception in HTTPPost request: " + e.Message);
                Log.Debug("Emitter: HTTPPost exception trace: " + e.StackTrace);
            }
            return count;
        }

        // --- Helpers

        /// <summary>
        /// Gets a ready request containing a POST
        /// </summary>
        /// <returns>The POST request that is already being sent</returns>
        /// <param name="events">Events to send in the post</param>
        protected HttpContent GetPOSTRequest(List<Dictionary<string, object>> events)
        {
            // Add STM to event
            AddSentTimeToEvents(events);

            // Build the event
            SelfDescribingJson sdj = new SelfDescribingJson(Constants.SCHEMA_PAYLOAD_DATA, events);

			// Build the HTTP Content Body
            HttpContent httpContent = new StringContent(sdj.ToString(), Encoding.UTF8, Constants.POST_CONTENT_TYPE);
            return httpContent;
        }

        /// <summary>
        /// Gets a ready request containing a GET
        /// </summary>
        /// <returns>The GET request that is already being sent</returns>
        /// <param name="eventDict">The event to be converted into a URI</param>
        protected Uri GetGETRequest(Dictionary<string, object> eventDict)
        {
            // Add STM to event
            eventDict.Add(Constants.SENT_TIMESTAMP, Utils.GetTimestamp().ToString());

            // Build the event
            return new Uri(collectorUri + Utils.ToQueryString(eventDict));
        }

        /// <summary>
        /// Gets the collector URI.
        /// </summary>
        /// <returns>The collector URI.</returns>
        /// <param name="endpoint">Endpoint.</param>
        /// <param name="protocol">Protocol.</param>
        /// <param name="method">Method.</param>
        protected Uri MakeCollectorUri(string endpoint, HttpProtocol protocol, Enums.HttpMethod method)
        {
            string path = (method == Enums.HttpMethod.GET) ? Constants.GET_URI_SUFFIX : Constants.POST_URI_SUFFIX;
            string requestProtocol = (protocol == HttpProtocol.HTTP) ? "http" : "https";
            return new Uri($"{requestProtocol}://{endpoint}{path}");
        }

        /// <summary>
        /// Adds the sent time to a list of event payloads
        /// </summary>
        /// <param name="events">The event list to add the stm to</param>
        protected void AddSentTimeToEvents(List<Dictionary<string, object>> events)
        {
            string stm = Utils.GetTimestamp().ToString();
            foreach (Dictionary<string, object> eventDict in events)
            {
                eventDict.Add(Constants.SENT_TIMESTAMP, stm);
            }
        }

        // --- Setters

        /// <summary>
        /// Sets the collector URI.
        /// </summary>
        /// <param name="endpoint">Endpoint.</param>
        public void SetCollectorUri(string endpoint)
        {
            this.endpoint = endpoint;
            collectorUri = MakeCollectorUri(this.endpoint, this.httpProtocol, this.httpMethod);
        }

        /// <summary>
        /// Sets the http protocol.
        /// </summary>
        /// <param name="httpProtocol">Http protocol.</param>
        public void SetHttpProtocol(HttpProtocol httpProtocol)
        {
            this.httpProtocol = httpProtocol;
            collectorUri = MakeCollectorUri(this.endpoint, this.httpProtocol, this.httpMethod);
        }

        /// <summary>
        /// Sets the http method.
        /// </summary>
        /// <param name="httpMethod">Http method.</param>
        public void SetHttpMethod(Enums.HttpMethod httpMethod)
        {
            this.httpMethod = httpMethod;
            collectorUri = MakeCollectorUri(this.endpoint, this.httpProtocol, this.httpMethod);
        }

        /// <summary>
        /// Sets the send limit; this controls how many events are grabbed out of the database at anytime.
        /// </summary>
        /// <param name="sendLimit">Send limit.</param>
        public void SetSendLimit(int sendLimit)
        {
            this.sendLimit = sendLimit;
        }

        /// <summary>
        /// Sets the byte limit for get requests.
        /// </summary>
        /// <param name="byteLimitGet">Byte limit get.</param>
        public void SetByteLimitGet(long byteLimitGet)
        {
            this.byteLimitGet = byteLimitGet;
        }

        /// <summary>
        /// Sets the byte limit for post requests.
        /// </summary>
        /// <param name="byteLimitPost">Byte limit post.</param>
        public void SetByteLimitPost(long byteLimitPost)
        {
            this.byteLimitPost = byteLimitPost;
        }

        // --- Getters

        /// <summary>
        /// Gets the collector URI.
        /// </summary>
        /// <returns>The collector URI.</returns>
        public Uri GetCollectorUri()
        {
            return collectorUri;
        }

        /// <summary>
        /// Gets the http protocol.
        /// </summary>
        /// <returns>The http protocol.</returns>
        public HttpProtocol GetHttpProtocol()
        {
            return httpProtocol;
        }

        /// <summary>
        /// Gets the http method.
        /// </summary>
        /// <returns>The http method.</returns>
        public Enums.HttpMethod GetHttpMethod()
        {
            return httpMethod;
        }

        /// <summary>
        /// Gets the send limit.
        /// </summary>
        /// <returns>The send limit.</returns>
        public int GetSendLimit()
        {
            return sendLimit;
        }

        /// <summary>
        /// Gets the byte limit for get requests.
        /// </summary>
        /// <returns>The byte limit get.</returns>
        public long GetByteLimitGet()
        {
            return byteLimitGet;
        }

        /// <summary>
        /// Gets the byte limit for post requests.
        /// </summary>
        /// <returns>The byte limit post.</returns>
        public long GetByteLimitPost()
        {
            return byteLimitPost;
        }

        /// <summary>
        /// Gets the event store.
        /// </summary>
        /// <returns>The event store.</returns>
        public IStore GetEventStore()
        {
            return eventStore;
        }
    }
}

