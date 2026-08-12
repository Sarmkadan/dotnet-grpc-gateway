#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetGrpcGateway.Domain;

/// <summary>
/// Holds reflection metadata discovered from a registered gRPC service's
/// Server Reflection endpoint.
/// </summary>
public class ServiceReflectionInfo
{
    /// <summary>Gets or sets the identifier of the associated <see cref="GrpcService"/>.</summary>
    public int ServiceId { get; set;         public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }, ServiceName = {ServiceName        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }, ServiceFullName = {ServiceFullName        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }, Methods = {Methods        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }, ReflectedAt = {ReflectedAt        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }, IsAvailable = {IsAvailable        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }         public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }";
            public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }

    /// <summary>Gets or sets the short display name of the service.</summary>
    public string ServiceName { get; set;         public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }, ServiceName = {ServiceName        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }, ServiceFullName = {ServiceFullName        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }, Methods = {Methods        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }, ReflectedAt = {ReflectedAt        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }, IsAvailable = {IsAvailable        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }         public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }";
            public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    } = null!;

    /// <summary>Gets or sets the fully-qualified gRPC service name (package.ServiceName).</summary>
    public string ServiceFullName { get; set;         public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }, ServiceName = {ServiceName        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }, ServiceFullName = {ServiceFullName        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }, Methods = {Methods        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }, ReflectedAt = {ReflectedAt        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }, IsAvailable = {IsAvailable        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }         public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }";
            public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    } = null!;

    /// <summary>Gets or sets the RPC method descriptors exposed by this service.</summary>
    public List<ServiceMethodDescriptor> Methods { get; set;         public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }, ServiceName = {ServiceName        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }, ServiceFullName = {ServiceFullName        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }, Methods = {Methods        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }, ReflectedAt = {ReflectedAt        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }, IsAvailable = {IsAvailable        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }         public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }";
            public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    } = new();

    /// <summary>Gets or sets when the reflection data was last retrieved.</summary>
    public DateTime ReflectedAt { get; set;         public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }, ServiceName = {ServiceName        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }, ServiceFullName = {ServiceFullName        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }, Methods = {Methods        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }, ReflectedAt = {ReflectedAt        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }, IsAvailable = {IsAvailable        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }         public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }";
            public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    } = DateTime.UtcNow;

    /// <summary>Gets or sets whether the reflection endpoint responded successfully.</summary>
    public bool IsAvailable { get; set;         public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }, ServiceName = {ServiceName        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }, ServiceFullName = {ServiceFullName        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }, Methods = {Methods        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }, ReflectedAt = {ReflectedAt        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }, IsAvailable = {IsAvailable        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }         public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }";
            public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }

    /// <summary>Gets or sets a diagnostic message when the reflection probe fails.</summary>
    public string? ErrorMessage { get; set;         public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }, ServiceName = {ServiceName        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }, ServiceFullName = {ServiceFullName        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }, Methods = {Methods        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }, ReflectedAt = {ReflectedAt        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }, IsAvailable = {IsAvailable        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }         public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }";
            public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }

    /// <summary>Gets the total number of RPC methods discovered.</summary>
    public int MethodCount => Methods.Count;

    /// <summary>Gets the number of streaming methods (client, server, or bidirectional).</summary>
    public int StreamingMethodCount => Methods.Count(m => m.IsClientStreaming || m.IsServerStreaming);
        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }, ServiceName = {ServiceName        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }, ServiceFullName = {ServiceFullName        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }, Methods = {Methods        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }, ReflectedAt = {ReflectedAt        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }, IsAvailable = {IsAvailable        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }         public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }";
            public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }

/// <summary>
/// Describes a single RPC method within a gRPC service as reported by the
/// Server Reflection protocol.
/// </summary>
public class ServiceMethodDescriptor
{
    /// <summary>Gets or sets the unqualified RPC method name.</summary>
    public string Name { get; set;         public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }, ServiceName = {ServiceName        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }, ServiceFullName = {ServiceFullName        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }, Methods = {Methods        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }, ReflectedAt = {ReflectedAt        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }, IsAvailable = {IsAvailable        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }         public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }";
            public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    } = null!;

    /// <summary>Gets or sets the fully-qualified protobuf type of the request message.</summary>
    public string RequestType { get; set;         public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }, ServiceName = {ServiceName        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }, ServiceFullName = {ServiceFullName        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }, Methods = {Methods        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }, ReflectedAt = {ReflectedAt        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }, IsAvailable = {IsAvailable        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }         public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }";
            public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    } = null!;

    /// <summary>Gets or sets the fully-qualified protobuf type of the response message.</summary>
    public string ResponseType { get; set;         public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }, ServiceName = {ServiceName        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }, ServiceFullName = {ServiceFullName        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }, Methods = {Methods        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }, ReflectedAt = {ReflectedAt        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }, IsAvailable = {IsAvailable        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }         public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }";
            public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    } = null!;

    /// <summary>Gets or sets whether the client sends a stream of request messages.</summary>
    public bool IsClientStreaming { get; set;         public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }, ServiceName = {ServiceName        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }, ServiceFullName = {ServiceFullName        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }, Methods = {Methods        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }, ReflectedAt = {ReflectedAt        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }, IsAvailable = {IsAvailable        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }         public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }";
            public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }

    /// <summary>Gets or sets whether the server sends a stream of response messages.</summary>
    public bool IsServerStreaming { get; set;         public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }, ServiceName = {ServiceName        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }, ServiceFullName = {ServiceFullName        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }, Methods = {Methods        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }, ReflectedAt = {ReflectedAt        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }, IsAvailable = {IsAvailable        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }         public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }";
            public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }

    /// <summary>Gets a human-readable label for the streaming mode of this method.</summary>
    public string StreamingMode =>
        (IsClientStreaming, IsServerStreaming) switch
        {
            (true, true)  => "BidirectionalStreaming",
            (true, false) => "ClientStreaming",
            (false, true) => "ServerStreaming",
            _             => "Unary"
                public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }, ServiceName = {ServiceName        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }, ServiceFullName = {ServiceFullName        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }, Methods = {Methods        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }, ReflectedAt = {ReflectedAt        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }, IsAvailable = {IsAvailable        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }         public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }";
            public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    };
        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }, ServiceName = {ServiceName        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }, ServiceFullName = {ServiceFullName        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }, Methods = {Methods        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }, ReflectedAt = {ReflectedAt        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }, IsAvailable = {IsAvailable        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }         public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }        public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }";
            public override string ToString() => $"ServiceReflectionInfo {{ ServiceId = {ServiceId}, ServiceName = {ServiceName}, ServiceFullName = {ServiceFullName}, Methods = {Methods}, ReflectedAt = {ReflectedAt}, IsAvailable = {IsAvailable} }}";
    }
