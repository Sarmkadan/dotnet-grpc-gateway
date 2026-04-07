#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using DotNetGrpcGateway.Domain;

namespace DotNetGrpcGateway.Services;

/// <summary>
/// Stores and provides queryable access to recent gateway request/response log entries.
/// </summary>
public interface IRequestLogService
{
    /// <summary>Appends a new log entry to the ring buffer.</summary>
    void Append(RequestLogEntry entry);

    /// <summary>Returns the most recent log entries, newest first.</summary>
    IReadOnlyList<RequestLogEntry> GetRecent(int limit = 100);

    /// <summary>Returns entries filtered by gRPC method substring.</summary>
    IReadOnlyList<RequestLogEntry> Search(
        string? methodFilter = null,
        int? statusCode = null,
        DateTime? from = null,
        DateTime? to = null,
        int limit = 100);

    /// <summary>Returns aggregate statistics across all retained entries.</summary>
    RequestLogSummary GetSummary();

    /// <summary>Clears all retained log entries.</summary>
    void Clear();
}

/// <summary>Aggregate statistics over retained log entries.</summary>
public class RequestLogSummary
{
    public int TotalEntries { get; set; }
    public int SuccessCount { get; set; }
    public int ErrorCount { get; set; }
    public double SuccessRatePct { get; set; }
    public double AverageDurationMs { get; set; }
    public long MinDurationMs { get; set; }
    public long MaxDurationMs { get; set; }
    public DateTime? OldestEntry { get; set; }
    public DateTime? NewestEntry { get; set; }
}

/// <summary>
/// Thread-safe, fixed-capacity ring buffer implementation of <see cref="IRequestLogService"/>.
/// Older entries are dropped when the buffer is full.
/// </summary>
public class RequestLogService : IRequestLogService
{
    private readonly int _capacity;
    private readonly ConcurrentQueue<RequestLogEntry> _queue = new();
    private int _count;

    public RequestLogService(int capacity = 10_000)
    {
        if (capacity <= 0)
            throw new ArgumentOutOfRangeException(nameof(capacity));

        _capacity = capacity;
    }

    /// <inheritdoc/>
    public void Append(RequestLogEntry entry)
    {
        if (entry is null)
            return;

        _queue.Enqueue(entry);
        Interlocked.Increment(ref _count);

        while (Volatile.Read(ref _count) > _capacity && _queue.TryDequeue(out _))
            Interlocked.Decrement(ref _count);
    }

    /// <inheritdoc/>
    public IReadOnlyList<RequestLogEntry> GetRecent(int limit = 100)
    {
        return _queue
            .OrderByDescending(entry => entry.Timestamp)
            .Take(Math.Max(1, limit))
            .ToList()
            .AsReadOnly();
    }

    /// <inheritdoc/>
    public IReadOnlyList<RequestLogEntry> Search(
        string? methodFilter = null,
        int? statusCode = null,
        DateTime? from = null,
        DateTime? to = null,
        int limit = 100)
    {
        var query = _queue.AsEnumerable();

        if (!string.IsNullOrEmpty(methodFilter))
        {
            query = query.Where(entry =>
                entry.GrpcMethod.Contains(methodFilter, StringComparison.OrdinalIgnoreCase));
        }

        if (statusCode.HasValue)
            query = query.Where(entry => entry.StatusCode == statusCode.Value);

        if (from.HasValue)
            query = query.Where(entry => entry.Timestamp >= from.Value);

        if (to.HasValue)
            query = query.Where(entry => entry.Timestamp <= to.Value);

        return query
            .OrderByDescending(entry => entry.Timestamp)
            .Take(Math.Max(1, limit))
            .ToList()
            .AsReadOnly();
    }

    /// <inheritdoc/>
    public RequestLogSummary GetSummary()
    {
        var entries = _queue.ToList();
        if (entries.Count == 0)
            return new RequestLogSummary();

        var successCount = entries.Count(entry => entry.IsSuccess);
        var durations = entries.Select(entry => entry.DurationMs).ToList();

        return new RequestLogSummary
        {
            TotalEntries = entries.Count,
            SuccessCount = successCount,
            ErrorCount = entries.Count - successCount,
            SuccessRatePct = entries.Count > 0 ? (double)successCount / entries.Count * 100 : 0,
            AverageDurationMs = durations.Average(),
            MinDurationMs = durations.Min(),
            MaxDurationMs = durations.Max(),
            OldestEntry = entries.Min(entry => entry.Timestamp),
            NewestEntry = entries.Max(entry => entry.Timestamp)
        };
    }

    /// <inheritdoc/>
    public void Clear()
    {
        while (_queue.TryDequeue(out _))
        {
        }

        Interlocked.Exchange(ref _count, 0);
    }
}
