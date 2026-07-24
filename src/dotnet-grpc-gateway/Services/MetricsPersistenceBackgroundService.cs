#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Threading.Channels;
using DotNetGrpcGateway.Domain;
using DotNetGrpcGateway.Infrastructure;

namespace DotNetGrpcGateway.Services;

/// <summary>
/// A bounded, in-memory queue that decouples request-metric persistence from the
/// proxied request's hot path. Producers enqueue metrics without ever touching the
/// database; a background writer drains and batches them separately. When the queue
/// is saturated, the oldest queued metric is dropped in favour of the newest one so a
/// slow or unavailable persistence layer can never apply back-pressure to live traffic.
/// </summary>
public interface IMetricsIngestQueue
{
    /// <summary>
    /// Enqueues a metric for asynchronous, batched persistence. Never blocks and never
    /// throws on a full queue - if capacity is exceeded, the oldest queued metric is
    /// dropped and <see cref="DroppedCount"/> is incremented.
    /// </summary>
    /// <param name="metric">The metric to enqueue.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="metric"/> is null.</exception>
    void Enqueue(RequestMetric metric);

    /// <summary>
    /// The total number of metrics dropped since process start because the queue was full.
    /// </summary>
    long DroppedCount { get; }
}

/// <summary>
/// Default <see cref="IMetricsIngestQueue"/> implementation backed by a bounded
/// <see cref="Channel{T}"/>.
/// </summary>
public sealed class MetricsIngestQueue : IMetricsIngestQueue
{
    /// <summary>
    /// Maximum number of metrics buffered in memory before the oldest entries start
    /// being dropped to make room for new ones.
    /// </summary>
    public const int Capacity = 5_000;

    private readonly Channel<RequestMetric> _channel;
    private readonly ILogger<MetricsIngestQueue> _logger;
    private long _droppedCount;

    /// <summary>
    /// Initializes a new instance of the <see cref="MetricsIngestQueue"/> class.
    /// </summary>
    /// <param name="logger">The logger used to report dropped metrics.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="logger"/> is null.</exception>
    public MetricsIngestQueue(ILogger<MetricsIngestQueue> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _channel = Channel.CreateBounded<RequestMetric>(new BoundedChannelOptions(Capacity)
        {
            SingleReader = true,
            SingleWriter = false,
            FullMode = BoundedChannelFullMode.DropWrite,
        });
    }

    /// <inheritdoc />
    public long DroppedCount => Interlocked.Read(ref _droppedCount);

    /// <summary>
    /// The channel reader used exclusively by the background writer to drain queued metrics.
    /// </summary>
    internal ChannelReader<RequestMetric> Reader => _channel.Reader;

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="metric"/> is null.</exception>
    public void Enqueue(RequestMetric metric)
    {
        ArgumentNullException.ThrowIfNull(metric);

        var writer = _channel.Writer;

        if (writer.TryWrite(metric))
            return;

        // Queue is saturated: evict the oldest entry to make room, then retry. If a
        // concurrent reader already drained the slot, the retried write simply succeeds.
        if (_channel.Reader.TryRead(out _))
            Interlocked.Increment(ref _droppedCount);

        if (writer.TryWrite(metric))
            return;

        // Extremely unlikely race (queue refilled between the read and the retry):
        // drop the incoming metric rather than block the caller.
        Interlocked.Increment(ref _droppedCount);
        _logger.LogWarning("Metrics ingest queue is saturated; dropping metric for {Service}.{Method}",
            metric.ServiceName, metric.MethodName);
    }
}

/// <summary>
/// Background service that drains <see cref="IMetricsIngestQueue"/> and persists queued
/// request metrics in batches, flushed either when a batch reaches a size threshold or
/// when a flush interval elapses. Persistence failures are logged and never propagate
/// to request processing, since this runs entirely off the request hot path.
/// </summary>
public sealed class MetricsPersistenceBackgroundService : BackgroundService
{
    private const int BatchSize = 100;
    private static readonly TimeSpan FlushInterval = TimeSpan.FromSeconds(2);

    private readonly MetricsIngestQueue _queue;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MetricsPersistenceBackgroundService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MetricsPersistenceBackgroundService"/> class.
    /// </summary>
    /// <param name="queue">The ingest queue to drain.</param>
    /// <param name="serviceProvider">Provider used to resolve a scoped <see cref="IMetricsRepository"/> per flush.</param>
    /// <param name="logger">The logger.</param>
    /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
    public MetricsPersistenceBackgroundService(
        MetricsIngestQueue queue,
        IServiceProvider serviceProvider,
        ILogger<MetricsPersistenceBackgroundService> logger)
    {
        _queue = queue ?? throw new ArgumentNullException(nameof(queue));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Metrics persistence background writer starting");

        var reader = _queue.Reader;
        var batch = new List<RequestMetric>(BatchSize);

        while (!stoppingToken.IsCancellationRequested)
        {
            using var flushTimeoutCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            flushTimeoutCts.CancelAfter(FlushInterval);

            try
            {
                while (batch.Count < BatchSize)
                {
                    if (!await reader.WaitToReadAsync(flushTimeoutCts.Token).ConfigureAwait(false))
                        break;

                    while (batch.Count < BatchSize && reader.TryRead(out var metric))
                        batch.Add(metric);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (OperationCanceledException)
            {
                // Flush interval elapsed; fall through and flush whatever was collected.
            }

            if (batch.Count > 0)
            {
                await FlushAsync(batch, stoppingToken).ConfigureAwait(false);
                batch.Clear();
            }
        }

        // Best-effort final flush of anything left buffered at shutdown.
        while (reader.TryRead(out var metric))
            batch.Add(metric);

        if (batch.Count > 0)
            await FlushAsync(batch, CancellationToken.None).ConfigureAwait(false);

        _logger.LogInformation("Metrics persistence background writer stopped");
    }

    private async Task FlushAsync(List<RequestMetric> batch, CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IMetricsRepository>();
            var persisted = await repository.BulkInsertAsync(batch, cancellationToken).ConfigureAwait(false);

            _logger.LogDebug("Persisted {Persisted}/{Total} queued request metrics", persisted, batch.Count);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            // Persistence failures must never affect already-completed proxied requests;
            // this batch is dropped and the writer resumes on the next flush cycle.
            _logger.LogError(ex, "Failed to persist batch of {Count} request metrics", batch.Count);
        }
    }
}
