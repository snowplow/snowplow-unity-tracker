# SnowplowTracker Emitters - Claude Code Documentation

## Emitter Module Overview
Emitters handle event transmission to Snowplow collectors with platform-specific optimizations. They manage batching, retries, threading, and HTTP communication strategies.

## Emitter Hierarchy

### Interface & Base Class
```csharp
IEmitter (interface)
└── AbstractEmitter (base implementation)
    ├── AsyncEmitter (production)
    ├── SyncEmitter (testing)
    └── WebGlEmitter (browser)
```

## Core Emitter Patterns

### Thread-Safe Event Queue
```csharp
// AsyncEmitter uses concurrent queue
private ConcurrentQueue<TrackerPayload> payloadQueue = 
    new ConcurrentQueue<TrackerPayload>();
```

### Dual-Thread Architecture
```csharp
// Event consumer thread
Thread payloadConsumer = new Thread(EventConsumer);
// Emitter send thread  
Thread emitThread = new Thread(EmitLoop);
```

## AsyncEmitter Implementation

### Initialization
```csharp
var emitter = new AsyncEmitter(
    endpoint: "collector.snowplow.io",
    protocol: HttpProtocol.HTTPS,
    method: HttpMethod.POST,
    sendLimit: 500,
    byteLimitGet: 52000,
    byteLimitPost: 52000,
    eventStore: null // Auto-selects based on platform
);
```

### Event Processing Loop
```csharp
void EmitLoop() {
    while (sending) {
        lock (emitLock) {
            if (eventStore.GetEventCount() == 0) {
                Monitor.Wait(emitLock);
            }
        }
        SendRequests(eventStore.GetEvents(sendLimit));
    }
}
```

### Event Consumer Pattern
```csharp
void EventConsumer() {
    while (consuming) {
        if (payloadQueue.TryDequeue(out payload)) {
            eventStore.AddEvent(payload);
            lock (emitLock) {
                Monitor.Pulse(emitLock);
            }
        }
    }
}
```

## SyncEmitter Implementation

### Synchronous Processing
```csharp
public override void Add(TrackerPayload payload) {
    eventStore.AddEvent(payload);
    SendRequests(eventStore.GetAllEvents());
}
```

### Use Cases
- Unit testing
- Debugging
- Guaranteed immediate send

## WebGlEmitter Implementation

### Browser Compatibility
```csharp
// Uses UnityWebRequest for WebGL
IEnumerator SendCoroutine(string uri, byte[] data) {
    var request = UnityWebRequest.Post(uri, "POST");
    yield return request.SendWebRequest();
}
```

### Coroutine Management
```csharp
public override void Start() {
    GameObject obj = new GameObject("WebGlEmitter");
    MonoBehaviour runner = obj.AddComponent<MonoBehaviour>();
    runner.StartCoroutine(EmitLoop());
}
```

## HTTP Request Strategies

### GET Request Construction
```csharp
string queryString = Utils.ToQueryString(payload);
string uri = collectorUri + "?" + queryString;
// Single event per GET request
```

### POST Request Construction
```csharp
var wrapper = new Dictionary<string, object> {
    ["schema"] = Constants.SCHEMA_PAYLOAD_DATA,
    ["data"] = eventPayloads
};
string json = JsonConvert.SerializeObject(wrapper);
```

### Batch Size Management
```csharp
if (httpMethod == HttpMethod.GET) {
    // One event per request
    foreach (var row in eventRows) {
        results.Add(SendGetRequest(row));
    }
} else {
    // Multiple events per POST
    results.AddRange(SendPostRequests(eventRows));
}
```

## Retry Logic

### Exponential Backoff
```csharp
int attempt = 0;
while (!success && attempt < MAX_ATTEMPTS) {
    success = SendRequest();
    if (!success) {
        Thread.Sleep(FAIL_INTERVAL * Math.Pow(2, attempt));
        attempt++;
    }
}
```

### Failed Event Handling
```csharp
if (result.IsSuccessful()) {
    eventStore.DeleteEvents(result.RowIds);
} else {
    // Events remain in store for retry
    Thread.Sleep(FAIL_INTERVAL);
}
```

## Storage Selection

### Platform-Based Storage
```csharp
this.eventStore = eventStore ?? (
    Application.platform == RuntimePlatform.tvOS ?
    new InMemoryEventStore() : // tvOS: memory only
    new EventStore()            // Others: LiteDB
);
```

### Storage Limits
```csharp
// InMemoryEventStore
const int MAX_MEMORY_EVENTS = 1000;
// EventStore (LiteDB)
// No hard limit, disk space dependent
```

## URI Construction

### Collector Endpoints
```csharp
// GET endpoint
string getUri = $"{protocol}://{host}/i";
// POST endpoint  
string postUri = $"{protocol}://{host}/com.snowplowanalytics.snowplow/tp2";
```

### Protocol Handling
```csharp
Uri MakeCollectorUri(string endpoint, HttpProtocol protocol, HttpMethod method) {
    string scheme = protocol == HttpProtocol.HTTPS ? "https" : "http";
    string path = method == HttpMethod.GET ? GET_URI_SUFFIX : POST_URI_SUFFIX;
    return new Uri($"{scheme}://{endpoint}{path}");
}
```

## Performance Optimizations

### Byte Limit Checking
```csharp
long totalBytes = POST_WRAPPER_BYTES;
foreach (var payload in payloads) {
    long payloadBytes = Utils.GetUTF8Length(payload);
    if (totalBytes + payloadBytes > byteLimitPost) {
        // Send current batch
        SendPostRequest(currentBatch);
        currentBatch.Clear();
    }
}
```

### Connection Pooling
```csharp
// HttpClient reuse for performance
private static readonly HttpClient httpClient = new HttpClient();
```

## Thread Synchronization

### Monitor Pattern
```csharp
lock (emitLock) {
    // Wait for events
    Monitor.Wait(emitLock);
}
// Signal work available
lock (emitLock) {
    Monitor.Pulse(emitLock);
}
```

### Thread Lifecycle
```csharp
public override void Stop() {
    sending = false;
    consuming = false;
    lock (emitLock) {
        Monitor.PulseAll(emitLock);
    }
    emitThread?.Join(2000);
    payloadConsumer?.Join(2000);
}
```

## Error Handling

### Network Failures
```csharp
try {
    response = await httpClient.PostAsync(uri, content);
} catch (HttpRequestException e) {
    Log.Error($"Network error: {e.Message}");
    return new RequestResult(false, rowIds);
}
```

### Timeout Handling
```csharp
httpClient.Timeout = TimeSpan.FromSeconds(30);
```

## Testing Support

### Mock Endpoints
```csharp
// Mountebank test collector
const string TEST_ENDPOINT = "localhost:4545";
```

### Event Capture
```csharp
public class TestEmitter : IEmitter {
    public List<TrackerPayload> CapturedEvents { get; }
}
```

## Platform-Specific Considerations

### iOS/Android
- Use AsyncEmitter
- File-based storage (EventStore)
- Background thread processing

### WebGL
- Use WebGlEmitter
- Coroutine-based sending
- Browser security restrictions

### tvOS
- Use AsyncEmitter
- Memory-only storage (InMemoryEventStore)
- No file system access

### Desktop (Windows/Mac/Linux)
- Use AsyncEmitter
- Full feature support
- Optimal thread pooling

## Quick Reference

### Emitter Selection Guide
```csharp
IEmitter SelectEmitter(string endpoint) {
    if (Application.platform == RuntimePlatform.WebGLPlayer)
        return new WebGlEmitter(endpoint);
    if (testMode)
        return new SyncEmitter(endpoint);
    return new AsyncEmitter(endpoint);
}
```

### Common Configuration
```csharp
// Production settings
var emitter = new AsyncEmitter(
    endpoint: productionUrl,
    protocol: HttpProtocol.HTTPS,
    method: HttpMethod.POST,
    sendLimit: 500
);
```