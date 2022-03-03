using System;
using System.Collections.Generic;

public static class RequestContextExtensions
{
    public static bool HasUserId(this RequestContext requestContext)
    {
        return !string.IsNullOrEmpty(requestContext.UserId);
    }

    public static string GetClientInfo(this RequestContext requestContext)
    {
        return $"Client IP: {requestContext.ClientIp}, User ID: {requestContext.UserId ?? "Unknown"}";
    }

    public static void SetStartTime(this RequestContext requestContext, DateTime startTime)
    {
        requestContext.Properties["StartTime"] = startTime;
    }

    public static DateTime? GetStartTime(this RequestContext requestContext)
    {
        if (requestContext.Properties.TryGetValue("StartTime", out object startTime))
        {
            return (DateTime)startTime;
        }
        return null;
    }
}
