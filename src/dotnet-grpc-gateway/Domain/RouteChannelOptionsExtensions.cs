#nullable enable

namespace DotNetGrpcGateway.Domain;

/// <summary>
/// Extension methods for <see cref="RouteChannelOptions"/> that provide convenient
/// ways to configure gRPC channel options.
/// </summary>
public static class RouteChannelOptionsExtensions
{
    /// <summary>
    /// Sets the call timeout to the specified milliseconds.
    /// </summary>
    /// <param name="options">The route channel options to configure.</param>
    /// <param name="timeoutMs">The timeout in milliseconds.</param>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    /// <returns>The configured <see cref="RouteChannelOptions"/> for method chaining.</returns>
    public static RouteChannelOptions WithCallTimeoutMs(this RouteChannelOptions options, int timeoutMs)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.CallTimeout = TimeSpan.FromMilliseconds(timeoutMs);
        return options;
    }

    /// <summary>
    /// Sets the maximum receive message size in bytes.
    /// </summary>
    /// <param name="options">The route channel options to configure.</param>
    /// <param name="maxSize">The maximum receive message size in bytes.</param>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxSize"/> is less than 0.</exception>
    /// <returns>The configured <see cref="RouteChannelOptions"/> for method chaining.</returns>
    public static RouteChannelOptions WithMaxReceiveMessageSize(this RouteChannelOptions options, int maxSize)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentOutOfRangeException.ThrowIfNegative(maxSize);

        options.MaxReceiveMessageSize = maxSize;
        return options;
    }

    /// <summary>
    /// Sets the maximum send message size in bytes.
    /// </summary>
    /// <param name="options">The route channel options to configure.</param>
    /// <param name="maxSize">The maximum send message size in bytes.</param>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxSize"/> is less than 0.</exception>
    /// <returns>The configured <see cref="RouteChannelOptions"/> for method chaining.</returns>
    public static RouteChannelOptions WithMaxSendMessageSize(this RouteChannelOptions options, int maxSize)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentOutOfRangeException.ThrowIfNegative(maxSize);

        options.MaxSendMessageSize = maxSize;
        return options;
    }

    /// <summary>
    /// Adds a header to the additional headers collection.
    /// </summary>
    /// <param name="options">The route channel options to configure.</param>
    /// <param name="name">The header name.</param>
    /// <param name="value">The header value.</param>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> or <paramref name="name"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="name"/> is <see cref="string.Empty"/>.</exception>
    /// <returns>The configured <see cref="RouteChannelOptions"/> for method chaining.</returns>
    public static RouteChannelOptions WithHeader(this RouteChannelOptions options, string name, string value)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(name);
        ArgumentException.ThrowIfNullOrEmpty(name);

        options.AdditionalHeaders[name] = value;
        return options;
    }

    /// <summary>
    /// Sets the TLS target name for the upstream service.
    /// </summary>
    /// <param name="options">The route channel options to configure.</param>
    /// <param name="targetName">The TLS target name.</param>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="targetName"/> is <see cref="string.Empty"/>.</exception>
    /// <returns>The configured <see cref="RouteChannelOptions"/> for method chaining.</returns>
    public static RouteChannelOptions WithTlsTargetName(this RouteChannelOptions options, string targetName)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrEmpty(targetName);

        options.TlsTargetName = targetName;
        return options;
    }

    /// <summary>
    /// Configures the channel to skip TLS verification (for development only).
    /// </summary>
    /// <param name="options">The route channel options to configure.</param>
    /// <param name="skip">Whether to skip TLS verification.</param>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    /// <returns>The configured <see cref="RouteChannelOptions"/> for method chaining.</returns>
    public static RouteChannelOptions WithSkipTlsVerification(this RouteChannelOptions options, bool skip = true)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.SkipTlsVerification = skip;
        return options;
    }

    /// <summary>
    /// Copies all non-null values from another <see cref="RouteChannelOptions"/> instance.
    /// </summary>
    /// <param name="options">The target route channel options.</param>
    /// <param name="source">The source route channel options to copy from.</param>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    /// <returns>The configured <see cref="RouteChannelOptions"/> for method chaining.</returns>
    public static RouteChannelOptions UpdateFrom(this RouteChannelOptions options, RouteChannelOptions? source)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (source == null)
            return options;

        if (source.CallTimeout.HasValue)
            options.CallTimeout = source.CallTimeout;

        if (source.MaxReceiveMessageSize.HasValue)
            options.MaxReceiveMessageSize = source.MaxReceiveMessageSize;

        if (source.MaxSendMessageSize.HasValue)
            options.MaxSendMessageSize = source.MaxSendMessageSize;

        if (source.AdditionalHeaders != null)
            options.AdditionalHeaders = new Dictionary<string, string>(source.AdditionalHeaders);

        options.SkipTlsVerification = source.SkipTlsVerification;
        options.TlsTargetName = source.TlsTargetName;

        return options;
    }
}