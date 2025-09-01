# Snowplow Unity Tracker - Claude Code Documentation

## Project Overview
The Snowplow Unity Tracker is a C# library for Unity that enables event tracking and analytics in Unity games and applications. It sends events to Snowplow collectors for data analysis, supporting multiple platforms including iOS, Android, Windows, Mac, Linux, and WebGL.

## Development Commands
```bash
# Build the tracker library
cd SnowplowTracker && dotnet build

# Run tests (requires Unity 2021.3.12f1 LTS)
# Open SnowplowTracker.Tests in Unity Editor
# Window -> General -> Test Runner -> EditMode -> Run All

# Start test collector (requires Vagrant)
vagrant up && vagrant ssh
cd /vagrant && mb &
curl -X POST -d @/vagrant/Resources/imposter.json http://localhost:2525/imposters

# Build demo game
# Open SnowplowTracker.Demo in Unity Editor and build
```

## Architecture
The tracker follows a layered architecture with clear separation of concerns:

### Core Components
- **Tracker**: Main entry point for event tracking operations
- **Emitter**: Handles event transmission (AsyncEmitter, SyncEmitter, WebGlEmitter)
- **Storage**: Event persistence using LiteDB or in-memory storage
- **Events**: Strongly-typed event classes with builder pattern
- **Payloads**: Event data structures and JSON serialization
- **Subject**: User/device information management
- **Session**: Session state and management

### Data Flow
1. Application creates events using builder pattern
2. Tracker validates and enriches events with context
3. Events are queued in storage (LiteDB or memory)
4. Emitter batches and sends events to collector
5. Failed events are retried with backoff

## Core Architectural Principles

### 1. Builder Pattern for Events
All events use fluent builder pattern for construction:
```csharp
// ✅ Correct: Use builder pattern
var event = new PageView()
    .SetPageUrl("https://example.com")
    .SetCustomContext(contexts)
    .Build();
```

### 2. Async-First Design
Primary emitter is asynchronous with thread-safe operations:
```csharp
// ✅ Correct: Use AsyncEmitter for production
IEmitter emitter = new AsyncEmitter(collectorUrl, 
    HttpProtocol.HTTPS, HttpMethod.POST);
```

### 3. Platform-Specific Adaptations
Different implementations for different Unity platforms:
```csharp
// ✅ Correct: Platform-specific emitter selection
IEmitter emitter = Application.platform == RuntimePlatform.WebGLPlayer 
    ? new WebGlEmitter(url) 
    : new AsyncEmitter(url);
```

### 4. Immutable Event Data
Events are immutable once built to ensure thread safety:
```csharp
// ❌ Wrong: Modifying built event
var event = new Structured().Build();
event.category = "new"; // Compilation error

// ✅ Correct: Configure before building
var event = new Structured()
    .SetCategory("category")
    .Build();
```

## Layer Organization & Responsibilities

### SnowplowTracker/ (Core Library)
- **Root**: Core tracker, subject, session, utilities
- **Emitters/**: Event transmission strategies
- **Events/**: Event type definitions and builders
- **Payloads/**: Data structures and serialization
- **Storage/**: Event persistence implementations
- **Requests/**: HTTP request handling
- **Enums/**: Type-safe enumerations

### SnowplowTracker.Tests/ (Unit Tests)
- **Tests/**: NUnit test fixtures
- **TestHelpers/**: Mock implementations and utilities

### SnowplowTracker.Demo/ (Example Unity Game)
- **Scripts/**: Demo game implementation with tracker usage

## Critical Import Patterns

### Namespace Organization
```csharp
// ✅ Correct: Organized namespace imports
using SnowplowTracker;
using SnowplowTracker.Emitters;
using SnowplowTracker.Events;
using SnowplowTracker.Payloads.Contexts;
using SnowplowTracker.Enums;
```

### Unity-Specific Imports
```csharp
// ✅ Correct: Unity engine imports when needed
using UnityEngine;
using System.Collections;
```

## Essential Library Patterns

### Tracker Initialization
```csharp
// ✅ Correct: Full initialization with session
var emitter = new AsyncEmitter(collectorUrl);
var subject = new Subject().SetUserId(userId);
var session = new Session(null);
var tracker = new Tracker(emitter, "namespace", 
    "appId", subject, session);
tracker.StartEventTracking();
```

### Event Tracking
```csharp
// ✅ Correct: Track with contexts
tracker.Track(new Structured()
    .SetCategory("game")
    .SetAction("click")
    .SetCustomContext(contexts)
    .Build());
```

### Context Creation
```csharp
// ✅ Correct: Build contexts with fluent API
var context = new MobileContext()
    .SetOsType("iOS")
    .SetOsVersion("14.0")
    .SetDeviceModel("iPhone 12")
    .Build();
```

## Model Organization Pattern

### Event Hierarchy
- **IEvent**: Base interface for all events
- **AbstractEvent<T>**: Generic base with common fields
- **Concrete Events**: PageView, Structured, Unstructured, etc.

### Payload Structure
- **IPayload**: Base payload interface
- **TrackerPayload**: Key-value event data
- **SelfDescribingJson**: Schema-based JSON payloads

### Context Types
- **IContext**: Base context interface
- **AbstractContext**: Common context implementation
- **Specific Contexts**: Mobile, Desktop, GeoLocation, Session

## Common Pitfalls & Solutions

### Storage on Mobile Platforms
```csharp
// ❌ Wrong: Using file storage on tvOS
IStore store = new EventStore(); // Fails on tvOS

// ✅ Correct: Platform-appropriate storage
IStore store = Application.platform == RuntimePlatform.tvOS
    ? new InMemoryEventStore()
    : new EventStore();
```

### Thread Safety in Emitters
```csharp
// ❌ Wrong: Direct collection manipulation
emitter.eventQueue.Add(event); // Not thread-safe

// ✅ Correct: Use thread-safe Add method
emitter.Add(payload); // Thread-safe
```

### Base64 Encoding for Custom Events
```csharp
// ❌ Wrong: Forgetting base64 encoding
var tracker = new Tracker(emitter, ns, appId, 
    base64Encoded: false); // May cause issues

// ✅ Correct: Enable base64 for compatibility
var tracker = new Tracker(emitter, ns, appId, 
    base64Encoded: true); // Default and recommended
```

### Unity Lifecycle Management
```csharp
// ❌ Wrong: Not stopping tracker
void OnApplicationPause(bool pause) { }

// ✅ Correct: Manage tracker lifecycle
void OnApplicationPause(bool pause) {
    if (pause) tracker.StopEventTracking();
    else tracker.StartEventTracking();
}
```

## File Structure Template
```
snowplow-unity-tracker/
├── SnowplowTracker/           # Core library project
│   └── SnowplowTracker/
│       ├── Emitters/          # Event transmission
│       ├── Events/            # Event types
│       ├── Payloads/          # Data structures
│       │   └── Contexts/      # Context types
│       ├── Storage/           # Persistence layer
│       ├── Requests/          # HTTP handling
│       ├── Enums/             # Type enumerations
│       ├── Tracker.cs         # Main tracker class
│       ├── Subject.cs         # User/device info
│       └── Session.cs         # Session management
├── SnowplowTracker.Tests/     # Unit test project
│   └── Assets/Tests/          # NUnit test fixtures
└── SnowplowTracker.Demo/      # Demo Unity game
    └── Assets/Scripts/        # Example implementation
```

## Quick Reference

### Import Checklist
- [ ] `using SnowplowTracker;` - Core namespace
- [ ] `using SnowplowTracker.Emitters;` - For emitter types
- [ ] `using SnowplowTracker.Events;` - For event builders
- [ ] `using SnowplowTracker.Payloads.Contexts;` - For contexts
- [ ] `using SnowplowTracker.Enums;` - For enumerations

### Common Event Types
- **PageView**: Web page views in WebGL builds
- **ScreenView**: Mobile screen transitions
- **Structured**: Custom categorized events
- **Unstructured**: Schema-based custom events
- **Timing**: Performance measurements
- **EcommerceTransaction**: Purchase events

### Platform Considerations
- **iOS/Android**: Use AsyncEmitter with EventStore
- **WebGL**: Use WebGlEmitter for browser compatibility
- **tvOS**: Use InMemoryEventStore (no file system)
- **Desktop**: Full feature support with all emitters

### Testing Patterns
- Use `BaseEmitter` for mocking in tests
- Test with local Mountebank collector
- Verify event payloads with `GetLastEvent()`
- Check thread safety with concurrent operations

## Contributing to CLAUDE.md

When adding or updating content in this document, please follow these guidelines:

### File Size Limit
- **CLAUDE.md must not exceed 40KB** (currently ~19KB)
- Check file size after updates: `wc -c CLAUDE.md`
- Remove outdated content if approaching the limit

### Code Examples
- Keep all code examples **4 lines or fewer**
- Focus on the essential pattern, not complete implementations
- Use `// ❌` and `// ✅` to clearly show wrong vs right approaches

### Content Organization
- Add new patterns to existing sections when possible
- Create new sections sparingly to maintain structure
- Update the architectural principles section for major changes
- Ensure examples follow current codebase conventions

### Quality Standards
- Test any new patterns in actual code before documenting
- Verify imports and syntax are correct for the codebase
- Keep language concise and actionable
- Focus on "what" and "how", minimize "why" explanations

### Multiple CLAUDE.md Files
- **Directory-specific CLAUDE.md files** can be created for specialized modules
- Follow the same structure and guidelines as this root CLAUDE.md
- Keep them focused on directory-specific patterns and conventions
- Maximum 20KB per directory-specific CLAUDE.md file

### Instructions for LLMs
When editing files in this repository, **always check for CLAUDE.md guidance**:

1. **Look for CLAUDE.md in the same directory** as the file being edited
2. **If not found, check parent directories** recursively up to project root
3. **Follow the patterns and conventions** described in the applicable CLAUDE.md
4. **Prioritize directory-specific guidance** over root-level guidance when conflicts exist