# SnowplowTracker Core Library - Claude Code Documentation

## Module Overview
This is the core C# library implementing the Snowplow tracker for Unity. It provides event tracking, storage, and transmission capabilities with platform-specific optimizations for Unity applications.

## Core Design Patterns

### Builder Pattern Implementation
All events inherit from `AbstractEvent<T>` with fluent builder:
```csharp
public abstract class AbstractEvent<T> : IEvent {
    public abstract T Self();
    public abstract T Build();
}
```

### Thread-Safe Event Queue
Emitters use concurrent collections for thread safety:
```csharp
private ConcurrentQueue<TrackerPayload> payloadQueue = 
    new ConcurrentQueue<TrackerPayload>();
```

### Platform Detection Pattern
Runtime platform checks for Unity-specific behavior:
```csharp
ConnectionType type = Application.platform == 
    RuntimePlatform.IPhonePlayer ? 
    ConnectionType.Direct : ConnectionType.Shared;
```

## Critical Class Responsibilities

### Tracker.cs
- Main API entry point
- Event enrichment with metadata
- Subject and session management
- Synchronous/asynchronous operation control

### Emitters/AbstractEmitter.cs
- Base class for all emitter implementations
- HTTP request construction
- Batch size management (GET: 1 event, POST: multiple)
- Retry logic with exponential backoff

### Storage/EventStore.cs
- LiteDB persistence for events
- Platform-specific connection types
- Thread-safe database operations with ReaderWriterLockSlim
- Automatic index creation on Id and CreatedAt

### Session.cs
- Session state management
- Background timeout checking
- Session context generation
- Storage in PlayerPrefs (Unity-specific)

## Event Type Patterns

### Standard Events
```csharp
// PageView: Web page tracking
new PageView().SetPageUrl(url).Build();

// Structured: Categorized custom events  
new Structured().SetCategory(cat).SetAction(act).Build();

// Unstructured: Schema-based events
new Unstructured().SetEventData(json).Build();
```

### Mobile-Specific Events
```csharp
// MobileScreenView: Native screen tracking
new MobileScreenView().SetName(name)
    .SetType(type).SetId(id).Build();
```

## Payload Construction

### TrackerPayload Structure
```csharp
// Key-value pairs with null filtering
payload.Add(Constants.EID, eventId);
payload.Add(Constants.TIMESTAMP, timestamp);
```

### SelfDescribingJson Pattern
```csharp
var json = new SelfDescribingJson(schema, data);
// Automatically wraps in schema envelope
```

## Context Implementation

### Context Builder Pattern
```csharp
public T SetOsType(string osType) {
    this.osType = osType;
    return (T)this;
}
```

### Context Types Priority
1. **SessionContext**: Auto-added if session exists
2. **MobileContext**: Device and network information
3. **DesktopContext**: Desktop-specific metadata
4. **GeoLocationContext**: Location data
5. **GenericContext**: Custom schema-based contexts

## Storage Strategy

### Platform-Specific Storage
```csharp
// tvOS: No file system access
if (Application.platform == RuntimePlatform.tvOS)
    return new InMemoryEventStore();
// Others: LiteDB file storage
return new EventStore("snowplow_events_lite.db");
```

### Database Schema
```sql
-- Events collection
Id: GUID (indexed)
CreatedAt: DateTime (indexed)
Payload: JSON string
```

## HTTP Request Handling

### GET Request Format
```
/i?e=pv&url=http://example.com&aid=app&...
```

### POST Request Format
```json
{
  "schema": "iglu:com.snowplowanalytics.snowplow/payload_data/jsonschema/1-0-3",
  "data": [...]
}
```

### Request Result Processing
```csharp
if (result.IsSuccessful()) {
    eventStore.DeleteEvents(rowIds);
} else {
    // Retry with backoff
}
```

## Unity-Specific Adaptations

### UnityWebRequest Integration
```csharp
// WebGL compatibility
UnityWebRequest request = new UnityWebRequest(uri);
request.uploadHandler = new UploadHandlerRaw(data);
```

### Coroutine Support
```csharp
// Async operations in Unity
yield return request.SendWebRequest();
```

### Application Lifecycle
```csharp
void OnApplicationPause(bool pauseStatus) {
    if (pauseStatus) session?.StopChecker();
    else session?.StartChecker();
}
```

## Error Handling Patterns

### Validation Guards
```csharp
Utils.CheckArgument(emitter != null, 
    "Emitter cannot be null.");
```

### Logging Levels
```csharp
Log.Debug("Event Store: Created");
Log.Error("Event Store: Failed to create");
Log.Verbose("Sending GET request...");
```

### Exception Resilience
```csharp
try {
    _dbLock.EnterWriteLock();
    // Database operations
} finally {
    _dbLock.ExitWriteLock();
}
```

## Performance Optimizations

### Batch Limits
- GET requests: 52KB default limit
- POST requests: 52KB default limit
- Send limit: 500 events per batch

### String Caching
```csharp
// Constants for repeated strings
public readonly static string EVENT_PAGE_VIEW = "pv";
```

### Lazy Initialization
```csharp
private static readonly Lazy<Tracker> _tracker = 
    new Lazy<Tracker>(() => new Tracker(...));
```

## Testing Patterns

### Mock Emitter
```csharp
public class BaseEmitter : IEmitter {
    // Captures events for testing
    public TrackerPayload GetLastEvent() { }
}
```

### Test Database Isolation
```csharp
var store = new EventStore(
    "test_" + Guid.NewGuid() + ".db");
```

## Common Implementation Mistakes

### Incorrect Platform Detection
```csharp
// ❌ Wrong: String comparison
if (Application.platform.ToString() == "iPhone")

// ✅ Correct: Enum comparison
if (Application.platform == RuntimePlatform.IPhonePlayer)
```

### Threading Issues
```csharp
// ❌ Wrong: Direct list access
emitter.eventList.Add(event);

// ✅ Correct: Thread-safe queue
emitter.payloadQueue.Enqueue(event);
```

### Resource Cleanup
```csharp
// ❌ Wrong: Not disposing database
var db = new LiteDatabase(path);

// ✅ Correct: Proper disposal
using (var db = new LiteDatabase(path)) { }
```

## Quick Implementation Reference

### Minimal Tracker Setup
```csharp
var emitter = new AsyncEmitter("collector.com");
var tracker = new Tracker(emitter, "ns", "app");
tracker.StartEventTracking();
```

### Full Featured Setup
```csharp
var emitter = new AsyncEmitter(url, HttpProtocol.HTTPS);
var subject = new Subject().SetUserId(id);
var session = new Session(null);
var tracker = new Tracker(emitter, "ns", "app", 
    subject, session);
```

### Event with Full Context
```csharp
tracker.Track(new Structured()
    .SetCategory("game")
    .SetAction("start")
    .SetCustomContext(contexts)
    .SetTimestamp(Utils.GetTimestamp())
    .Build());
```