# SnowplowTracker Tests - Claude Code Documentation

## Test Module Overview
NUnit-based test suite for the Snowplow Unity Tracker, providing comprehensive unit and integration testing. Tests run in Unity Editor's EditMode for immediate feedback without scene loading.

## Test Framework Setup

### NUnit Configuration
```csharp
using NUnit.Framework;
[TestFixture()]
public class TestClassName { }
```

### Unity Test Runner
- Location: Window → General → Test Runner → EditMode
- Run Mode: EditMode (no scene required)
- Framework: NUnit 3.x with Unity Test Framework

## Test Organization Pattern

### Test Class Structure
```csharp
[TestFixture()]
public class TestTracker {
    [Test()]
    public void TestTrackerInitMinimal() { }
}
```

### Test Naming Conventions
- Test classes: `Test{ComponentName}`
- Test methods: `Test{Feature}{Scenario}`
- Example: `TestTrackerInitException`

## Mock Implementation Patterns

### BaseEmitter Mock
```csharp
public class BaseEmitter : IEmitter {
    private List<TrackerPayload> events = new();
    public TrackerPayload GetLastEvent() => 
        events.LastOrDefault();
}
```

### Test Collector Setup
```csharp
// Mountebank mock collector
const string TEST_COLLECTOR = "http://localhost:4545";
```

## Common Test Patterns

### Initialization Testing
```csharp
[Test()]
public void TestTrackerInitMinimal() {
    var emitter = new AsyncEmitter("acme.com");
    var tracker = new Tracker(emitter, "ns", "app");
    Assert.NotNull(tracker);
}
```

### Exception Testing
```csharp
[Test()]
public void TestTrackerInitException() {
    Tracker t = null;
    try {
        t = new Tracker(null, "ns", "app");
    } catch (Exception e) {
        Assert.AreEqual("Emitter cannot be null.", e.Message);
    }
    Assert.Null(t);
}
```

### Event Payload Verification
```csharp
[Test()]
public void TestEventPayload() {
    var mockEmitter = new BaseEmitter();
    tracker.Track(event);
    var payload = mockEmitter.GetLastEvent();
    Assert.AreEqual("pv", payload.GetDictionary()["e"]);
}
```

## Test Data Builders

### Context Test Data
```csharp
var mobileContext = new MobileContext()
    .SetOsType("iOS")
    .SetOsVersion("14.0")
    .Build();
```

### Event Test Data
```csharp
var pageView = new PageView()
    .SetPageUrl("http://test.com")
    .SetPageTitle("Test")
    .Build();
```

## Storage Testing

### Temporary Test Database
```csharp
var testDb = "test_" + Guid.NewGuid() + ".db";
var store = new EventStore(testDb, false);
// Test operations
File.Delete(testDb); // Cleanup
```

### In-Memory Testing
```csharp
var store = new InMemoryEventStore();
// No cleanup required
```

## Integration Test Patterns

### End-to-End Tracking
```csharp
[Test()]
public void TestIntegration() {
    var emitter = new AsyncEmitter(TEST_COLLECTOR);
    var tracker = new Tracker(emitter, "test", "app");
    tracker.StartEventTracking();
    tracker.Track(event);
    Thread.Sleep(1000); // Wait for async
    // Verify at collector
}
```

### Session Testing
```csharp
[Test()]
public void TestSession() {
    var session = new Session(testPath);
    session.StartChecker();
    Thread.Sleep(500);
    Assert.True(session.IsBackground());
}
```

## Assertion Patterns

### Value Assertions
```csharp
Assert.AreEqual(expected, actual);
Assert.NotNull(object);
Assert.True(condition);
Assert.False(condition);
```

### Collection Assertions
```csharp
Assert.AreEqual(3, list.Count);
Assert.Contains(item, collection);
```

### Exception Assertions
```csharp
Assert.Throws<ArgumentException>(() => {
    new Tracker(null, "ns", "app");
});
```

## Test Categories

### Unit Tests
- **Payloads**: TrackerPayload, SelfDescribingJson
- **Contexts**: Mobile, Desktop, GeoLocation, Session
- **Events**: All event type builders
- **Storage**: EventStore, InMemoryEventStore
- **Utils**: Utility function testing

### Integration Tests
- **TestIntegration**: Full tracking pipeline
- **TestEmitter**: Network communication
- **TestSession**: Background timers

## Performance Testing

### Concurrent Operations
```csharp
[Test()]
public void TestConcurrency() {
    var tasks = new List<Task>();
    for (int i = 0; i < 100; i++) {
        tasks.Add(Task.Run(() => tracker.Track(event)));
    }
    Task.WaitAll(tasks.ToArray());
}
```

### Memory Testing
```csharp
[Test()]
public void TestMemoryStore() {
    var store = new InMemoryEventStore();
    for (int i = 0; i < 1000; i++) {
        store.AddEvent(payload);
    }
    Assert.LessOrEqual(store.GetEventCount(), 1000);
}
```

## Test Utilities

### Test Constants
```csharp
const string TEST_URL = "http://test.com";
const string TEST_NAMESPACE = "test-namespace";
const string TEST_APP_ID = "test-app";
```

### Helper Methods
```csharp
private TrackerPayload CreateTestPayload() {
    var payload = new TrackerPayload();
    payload.Add("test", "value");
    return payload;
}
```

## Common Test Issues

### Async Test Timing
```csharp
// ❌ Wrong: No wait for async
tracker.Track(event);
Assert.AreEqual(1, GetEventCount());

// ✅ Correct: Wait for async completion
tracker.Track(event);
Thread.Sleep(100);
Assert.AreEqual(1, GetEventCount());
```

### Database Cleanup
```csharp
// ❌ Wrong: Leaving test databases
var store = new EventStore("test.db");

// ✅ Correct: Unique names and cleanup
var db = "test_" + Guid.NewGuid() + ".db";
try { /* test */ }
finally { File.Delete(db); }
```

### Mock Isolation
```csharp
// ❌ Wrong: Shared mock state
static BaseEmitter mockEmitter = new();

// ✅ Correct: Fresh mock per test
var mockEmitter = new BaseEmitter();
```

## Test Execution Guidelines

### Running Tests
1. Build SnowplowTracker solution first
2. Open SnowplowTracker.Tests in Unity
3. Start Mountebank if testing integration
4. Run via Test Runner window

### CI/CD Integration
- GitHub Actions workflow included
- Unity Test Runner action configured
- Mountebank setup automated
- Test artifacts uploaded

## Quick Test Reference

### Test Structure Template
```csharp
[TestFixture()]
public class TestComponent {
    private BaseEmitter emitter;
    private Tracker tracker;
    
    [SetUp]
    public void Setup() {
        emitter = new BaseEmitter();
        tracker = new Tracker(emitter, "ns", "app");
    }
    
    [Test()]
    public void TestFeature() {
        // Arrange
        var event = new PageView().Build();
        // Act
        tracker.Track(event);
        // Assert
        Assert.NotNull(emitter.GetLastEvent());
    }
}
```