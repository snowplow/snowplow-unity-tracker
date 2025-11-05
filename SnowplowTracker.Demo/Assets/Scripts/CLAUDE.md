# SnowplowTracker Demo - Claude Code Documentation

## Demo Module Overview
Unity game demonstrating Snowplow tracker integration. Shows best practices for tracker initialization, event tracking in gameplay, and platform-specific implementations.

## Tracker Integration Pattern

### Singleton Tracker with Lazy Initialization
```csharp
private static readonly Lazy<Tracker> _snowplowTracker = 
    new Lazy<Tracker>(() => {
        var emitter = CreateEmitter();
        return new Tracker(emitter, "ns", "appId");
    });
```

### Platform-Specific Emitter Selection
```csharp
IEmitter emitter = Application.platform == 
    RuntimePlatform.WebGLPlayer 
    ? new WebGlEmitter(url, HttpProtocol.HTTPS) 
    : new AsyncEmitter(url, HttpProtocol.HTTPS);
```

## Unity Component Patterns

### TrackerManager MonoBehaviour
```csharp
public class TrackerManager : MonoBehaviour {
    public static Tracker SnowplowTracker => 
        _snowplowTracker.Value;
}
```

### UI Integration
```csharp
public InputField CollectorInput;
void Start() {
    CollectorInput.onEndEdit.AddListener((x) => 
        _collectorUrl = x);
}
```

## Gameplay Event Tracking

### Game State Events
```csharp
// Game start
tracker.Track(new Structured()
    .SetCategory("game")
    .SetAction("start")
    .Build());
```

### Player Action Events
```csharp
// Snowball collision
tracker.Track(new Structured()
    .SetCategory("gameplay")
    .SetAction("collision")
    .SetProperty("snowball")
    .Build());
```

### Screen Transition Events
```csharp
// Menu navigation
tracker.Track(new ScreenView()
    .SetName("MainMenu")
    .SetId(Guid.NewGuid().ToString())
    .Build());
```

## Subject Management

### Persistent User ID
```csharp
if (!PlayerPrefs.HasKey("userId")) {
    PlayerPrefs.SetString("userId", 
        Guid.NewGuid().ToString());
    PlayerPrefs.Save();
}
subject.SetUserId(PlayerPrefs.GetString("userId"));
```

### Device Information
```csharp
subject.SetPlatform(DevicePlatforms.Mobile);
subject.SetScreenResolution(Screen.width, Screen.height);
```

## Context Examples

### Game-Specific Contexts
```csharp
var gameContext = new GenericContext()
    .SetSchema("iglu:com.game/context/1-0-0")
    .Add("level", currentLevel)
    .Add("score", playerScore)
    .Build();
```

### Platform Contexts
```csharp
var contexts = new List<IContext> {
    new DesktopContext()
        .SetOsType("OS-X")
        .SetDeviceModel("Macbook Pro")
        .Build(),
    new MobileContext()
        .SetOsType("iOS")
        .SetCarrier("Verizon")
        .Build()
};
```

## Unity Lifecycle Integration

### Application Focus
```csharp
void OnApplicationFocus(bool hasFocus) {
    if (hasFocus) {
        tracker.StartEventTracking();
    }
}
```

### Application Pause
```csharp
void OnApplicationPause(bool pauseStatus) {
    if (pauseStatus) {
        tracker.StopEventTracking();
    }
}
```

### Scene Management
```csharp
void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
    tracker.Track(new ScreenView()
        .SetName(scene.name)
        .Build());
}
```

## Demo Game Components

### GameplayManager
- Handles game logic and scoring
- Tracks gameplay events (collisions, scoring)
- Manages game state transitions

### UIManager
- Manages UI state and navigation
- Tracks UI interaction events
- Handles collector URL configuration

### Platform/Ice/Snowball
- Game objects with tracking triggers
- Collision detection with event firing
- Physics-based gameplay mechanics

## Event Tracking Best Practices

### Batching Related Events
```csharp
// ❌ Wrong: Multiple tracker calls
tracker.Track(event1);
tracker.Track(event2);
tracker.Track(event3);

// ✅ Correct: Use contexts for related data
tracker.Track(event.SetCustomContext(contexts));
```

### Timing Events
```csharp
float startTime = Time.time;
// Gameplay action
tracker.Track(new Timing()
    .SetCategory("performance")
    .SetVariable("level_load")
    .SetTiming((Time.time - startTime) * 1000)
    .Build());
```

### Error Tracking
```csharp
try {
    // Game operation
} catch (Exception e) {
    tracker.Track(new Unstructured()
        .SetEventData(new SelfDescribingJson(
            "iglu:com.game/error/1-0-0",
            new { error = e.Message }))
        .Build());
}
```

## Configuration Management

### Development vs Production
```csharp
#if UNITY_EDITOR
    string collectorUrl = "http://localhost:4545";
#else
    string collectorUrl = "https://collector.snowplow.io";
#endif
```

### Build-Specific Settings
```csharp
#if UNITY_IOS
    var emitter = new AsyncEmitter(url);
#elif UNITY_WEBGL
    var emitter = new WebGlEmitter(url);
#else
    var emitter = new AsyncEmitter(url);
#endif
```

## Performance Considerations

### Event Throttling
```csharp
private float lastEventTime;
void SendEvent() {
    if (Time.time - lastEventTime > 0.5f) {
        tracker.Track(event);
        lastEventTime = Time.time;
    }
}
```

### Memory Management
```csharp
void OnDestroy() {
    tracker?.StopEventTracking();
    contexts?.Clear();
}
```

## Testing in Demo

### Manual Testing
1. Enter collector URL in UI
2. Play game and trigger events
3. Check Mountebank for received events
4. Verify event payloads

### Debug Logging
```csharp
Log.Verbose = true; // Enable tracker logs
Debug.Log($"Tracking event: {eventType}");
```

## Common Demo Patterns

### Initialization Flow
```csharp
Start() → Configure URL → 
    Lazy<Tracker> accessed → 
    Emitter created → 
    Tracker started
```

### Event Flow
```csharp
Game Action → 
    Create Event → 
    Add Contexts → 
    Track Event → 
    Queue in Emitter
```

## Quick Reference

### Essential Demo Classes
- **TrackerManager**: Tracker singleton and configuration
- **GameplayManager**: Game logic and event tracking
- **UIManager**: UI state and interaction tracking

### Key Integration Points
- Collector URL configuration via UI
- Platform-specific emitter selection
- Persistent user ID management
- Context list generation