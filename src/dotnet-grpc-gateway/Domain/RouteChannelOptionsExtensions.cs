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
    /// <returns>The configured <see cref="RouteChannelOptions"/> for method chaining.</returns>
    public static RouteChannelOptions WithCallTimeoutMs(this RouteChannelOptions options, int timeoutMs)
    {
        options.CallTimeout = TimeSpan.FromMilliseconds(timeoutMs);
        return options;
    }

    /// <summary>
    /// Sets the maximum receive message size in bytes.
    /// </summary>
    /// <param name="options">The route channel options to configure.</param>
    /// <param name="maxSize">The maximum receive message size in bytes.</param>
    /// <returns>The configured <see cref="RouteChannelOptions"/> for method chaining.</returns>
    public static RouteChannelOptions WithMaxReceiveMessageSize(this RouteChannelOptions options, int maxSize)
    {
        options.MaxReceiveMessageSize = maxSize;
        return options;
    }

    /// <summary>
    /// Sets the maximum send message size in bytes.
    /// </summary>
    /// <param name="options">The route channel options to configure.</param>
    /// <param name="maxSize">The maximum send message size in bytes.</param>
    /// <returns>The configured <see cref="RouteChannelOptions"/> for method chaining.</returns>
    public static RouteChannelOptions WithMaxSendMessageSize(this RouteChannelOptions options, int maxSize)
    {
        options.MaxSendMessageSize = maxSize;
        return options;
    }

    /// <summary>
    /// Adds a header to the additional headers collection.
    /// </summary>
    /// <param name="options">The route channel options to configure.</param>
    /// <param name="name">The header name.</param>
    /// <param name="value">The header value.</param>
    /// <returns>The configured <see cref="RouteChannelOptions"/> for method chaining.</returns>
    public static RouteChannelOptions WithHeader(this RouteChannelOptions options, string name, string value)
    {
        options.AdditionalHeaders[name] = value;
        return options;
    }

    /// <summary>
    /// Sets the TLS target name for the upstream service.
    /// </summary>
    /// <param name="options">The route channel options to configure.</param>
    /// <param name="targetName">The TLS target name.</param>
    /// <returns>The configured <see cref="RouteChannelOptions"/> for method chaining.</returns>
    public static RouteChannelOptions WithTlsTargetName(this RouteChannelOptions options, string targetName)
    {
        options.TlsTargetName = targetName;
        return options;
    }

    /// <summary>
    /// Configures the channel to skip TLS verification (for development only).
    /// </summary>
    /// <param name="options">The route channel options to configure.</param>
    /// <param name="skip">Whether to skip TLS verification.</param>
    /// <returns>The configured <see cref="RouteChannelOptions"/> for method chaining.</returns>
    public static RouteChannelOptions WithSkipTlsVerification(this RouteChannelOptions options, bool skip = true)
    {
        options.SkipTlsVerification = skip;
        return options;
    }

    /// <summary>
    /// Copies all non-null values from another <see cref="RouteChannelOptions"/> instance.
    /// </summary>
    /// <param name="options">The target route channel options.</param>
    /// <param name="source">The source route channel options to copy from.</param>
    /// <returns>The configured <see cref="RouteChannelOptions"/> for method chaining.</returns>
    public static RouteChannelOptions UpdateFrom(this RouteChannelOptions options, RouteChannelOptions? source)
    {
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