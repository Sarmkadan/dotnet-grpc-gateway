# StructuredLogger

Static helper class that provides structured logging for various stages of gRPC‑gateway request processing. It centralizes the emission of log events so that consumers can consistently record request lifecycle, service discovery, caching, routing, rate‑limiting, authentication, error conditions, and performance metrics without duplicating logging logic.

## API

### LogRequestStart
**Purpose:** Emits a log entry when a gRPC‑gateway request begins processing.  
**Parameters:** (as defined in the source)  
**Return value:** `void`  
**Throws:** May throw an exception if the underlying logging sink fails (e.g., `IOException` when writing to a file).

### LogRequestComplete
**Purpose:** Emits a log entry when a gRPC‑gateway request finishes, indicating success or failure.  
**Parameters:** (as defined in the source)  
**Return value:** `void`  
**Throws:** May throw an exception if logging to the configured sink fails.

### LogServiceDiscovery
**Purpose:** Logs the outcome of a service‑discovery lookup performed to resolve a target service for the request.  
**Parameters:** (as defined in the source)  
**Return value:** `void`  
**Throws:** May throw if the logging operation encounters an error.

### LogCacheOperation
**Purpose:** Records cache‑related actions such as hits, misses, inserts, or evictions during request handling.  
**Parameters:** (as defined in the source)  
**Return value:** `void`  
**Throws:** May throw when the logger cannot write the cache event.

### LogRouteResolution
**Purpose:** Logs the result of routing table lookup that determines which gRPC service method will handle the request.  
**Parameters:** (as defined in the source)  
**Return value:** `void`  
**Throws:** May throw if the logging backend experiences a failure.

### LogRateLimit
**Purpose:** Emits a log entry when a request is evaluated against rate‑limiting rules, including allowed or denied outcomes.  
**Parameters:** (as defined in the source)  
**Return value:** `void`  
**Throws:** May throw on logging failure.

### LogAuthentication
**Purpose:** Logs authentication attempts, results, and any relevant metadata (e.g., token validation success/failure).  
**Parameters:** (as defined in the source)  
**Return value:** `void`  
**Throws:** May throw if the logger cannot record the authentication event.

### LogCriticalError
**Purpose:** Records unexpected or fatal errors that occur during request processing, ensuring they are captured for diagnostics.  
**Parameters:** (as defined in the source)  
**Return value:** `void`  
**Throws:** May throw if logging the critical error itself fails (though implementations often guard against this).

### LogPerformanceMetrics
**Purpose:** Emits structured performance data such as latency, throughput, or resource usage measurements for a request.  
**Parameters:** (as defined in the source)  
**Return value:** `void`  
**Throws:** May throw when the performance metric cannot be written to the log sink.

## Usage

```csharp
using DotNetGrpcGateway.Logging;

// At the start of request handling
StructuredLogger.LogRequestStart(httpContext, requestId, methodName);

// After service discovery resolves the target endpoint
StructuredLogger.LogServiceDiscovery(serviceName, resolvedAddress, latencyMs);
```

```csharp
using DotNetGrpcGateway.Logging;

// Inside a middleware that enforces rate limits
if (!rateLimiter.TryConsume(key))
{
    StructuredLogger.LogRateLimit(key, allowed: false, retryAfterSeconds: 30);
    context.Response.StatusCode = 429;
    return;
}

// After successful authentication
StructuredLogger.LogAuthentication(userId, scheme: "Bearer", success: true);
```

## Notes

- All methods are `static`; they do not maintain instance state and are safe to call from any thread provided the underlying logging implementation is thread‑safe (most common logging frameworks such as Serilog, Microsoft.Extensions.Logging, or NLog are thread‑safe).
- If a logging sink throws an exception, the exception will propagate to the caller. Applications that require logging to never disrupt request processing should wrap calls in a try/catch block or configure the logger to swallow errors internally.
- The methods return `void`; they are intended solely for side‑effects (writing log entries). No return values are used to convey success or failure.
- Parameter types and exact semantics are defined in the source code of `StructuredLogger`; consult the implementation for precise details (e.g., whether `HttpContext`, timestamps, or correlation IDs are expected). 
- In high‑throughput scenarios, consider the performance impact of synchronous logging; asynchronous logging adapters can be used behind the scenes to mitigate latency.
