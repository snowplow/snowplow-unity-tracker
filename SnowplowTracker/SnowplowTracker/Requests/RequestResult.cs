/*
 * RequestResult.cs
 * SnowplowTracker.Requests
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
using System.Collections.Generic;

namespace SnowplowTracker.Requests
{
    public class RequestResult
    {
        public bool success;
        public List<Guid> rowIds;

        /// <summary>
        /// Initializes a new instance of the <see cref="SnowplowTracker.Requests.RequestResult"/> class.
        /// </summary>
        /// <param name="success">If set to <c>true</c> success.</param>
        /// <param name="rowIds">Row identifiers.</param>
        public RequestResult(bool success, List<Guid> rowIds)
        {
            this.success = success;
            this.rowIds = rowIds;
        }
    }
}
