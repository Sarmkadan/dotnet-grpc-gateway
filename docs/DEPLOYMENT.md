# Deployment Guide

Production deployment strategies for dotnet-grpc-gateway.

## Quick Deployment Options

### Option 1: Docker Compose (Development/Staging)

```bash
docker-compose up -d
```

See [../docker-compose.yml](../docker-compose.yml) for full configuration.

### Option 2: Kubernetes (Production Recommended)

Deploy using provided Kubernetes manifests:

```bash
kubectl apply -f k8s/namespace.yaml
kubectl apply -f k8s/configmap.yaml
kubectl apply -f k8s/deployment.yaml
kubectl apply -f k8s/service.yaml
kubectl apply -f k8s/hpa.yaml
```

### Option 3: Docker Build & Deploy

```bash
# Build image
docker build -t dotnet-grpc-gateway:latest .

# Push to registry
docker tag dotnet-grpc-gateway:latest myregistry.azurecr.io/dotnet-grpc-gateway:v1.0.0
docker push myregistry.azurecr.io/dotnet-grpc-gateway:v1.0.0

# Run container
docker run -d \
  -p 5000:5000 \
  -e Gateway__Port=5000 \
  -e ConnectionStrings__DefaultConnection="Host=db.example.com;..." \
  myregistry.azurecr.io/dotnet-grpc-gateway:v1.0.0
```

---

## Kubernetes Deployment

### Namespace & RBAC

```yaml
apiVersion: v1
kind: Namespace
metadata:
  name: grpc-gateway

---
apiVersion: v1
kind: ServiceAccount
metadata:
  name: grpc-gateway
  namespace: grpc-gateway
```

### ConfigMap

```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: grpc-gateway-config
  namespace: grpc-gateway
data:
  appsettings.json: |
    {
      "Gateway": {
        "Port": 5000,
        "MaxConcurrentConnections": 2000,
        "RequestTimeoutMs": 30000,
        "HealthCheck": {
          "IntervalSeconds": 30,
          "TimeoutMs": 5000,
          "FailureThreshold": 3
        },
        "Metrics": {
          "EnableMetrics": true,
          "RetentionDays": 30
        }
      }
    }
```

### Deployment

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: grpc-gateway
  namespace: grpc-gateway
spec:
  replicas: 3
  strategy:
    type: RollingUpdate
    rollingUpdate:
      maxSurge: 1
      maxUnavailable: 1
  selector:
    matchLabels:
      app: grpc-gateway
  template:
    metadata:
      labels:
        app: grpc-gateway
    spec:
      serviceAccountName: grpc-gateway
      containers:
      - name: gateway
        image: myregistry.azurecr.io/dotnet-grpc-gateway:v1.0.0
        imagePullPolicy: IfNotPresent
        ports:
        - name: http
          containerPort: 5000
          protocol: TCP
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        - name: ASPNETCORE_URLS
          value: "http://0.0.0.0:5000"
        - name: Gateway__Port
          value: "5000"
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: grpc-gateway-secrets
              key: db-connection-string
        resources:
          requests:
            cpu: 250m
            memory: 256Mi
          limits:
            cpu: 1000m
            memory: 512Mi
        livenessProbe:
          httpGet:
            path: /api/health/live
            port: http
          initialDelaySeconds: 30
          periodSeconds: 10
          timeoutSeconds: 5
          failureThreshold: 3
        readinessProbe:
          httpGet:
            path: /api/health/ready
            port: http
          initialDelaySeconds: 10
          periodSeconds: 5
          timeoutSeconds: 3
          failureThreshold: 2
        volumeMounts:
        - name: logs
          mountPath: /app/logs
      volumes:
      - name: logs
        emptyDir: {}
```

### Service

```yaml
apiVersion: v1
kind: Service
metadata:
  name: grpc-gateway
  namespace: grpc-gateway
spec:
  type: ClusterIP
  ports:
  - name: http
    port: 5000
    targetPort: 5000
    protocol: TCP
  selector:
    app: grpc-gateway
```

### Horizontal Pod Autoscaler

```yaml
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: grpc-gateway-hpa
  namespace: grpc-gateway
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: grpc-gateway
  minReplicas: 3
  maxReplicas: 10
  metrics:
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: 70
  - type: Resource
    resource:
      name: memory
      target:
        type: Utilization
        averageUtilization: 80
```

### Ingress

```yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: grpc-gateway-ingress
  namespace: grpc-gateway
spec:
  ingressClassName: nginx
  tls:
  - hosts:
    - api.example.com
    secretName: tls-secret
  rules:
  - host: api.example.com
    http:
      paths:
      - path: /
        pathType: Prefix
        backend:
          service:
            name: grpc-gateway
            port:
              number: 5000
```

### Secrets

```bash
# Create secrets
kubectl create secret generic grpc-gateway-secrets \
  --from-literal=db-connection-string="Host=postgres.default;Port=5432;Database=grpc_gateway;Username=postgres;Password=secure_password" \
  -n grpc-gateway
```

---

## Database Setup

### PostgreSQL on Kubernetes

```yaml
apiVersion: v1
kind: PersistentVolumeClaim
metadata:
  name: postgres-pvc
  namespace: grpc-gateway
spec:
  accessModes:
    - ReadWriteOnce
  resources:
    requests:
      storage: 20Gi

---
apiVersion: apps/v1
kind: StatefulSet
metadata:
  name: postgres
  namespace: grpc-gateway
spec:
  serviceName: postgres
  replicas: 1
  selector:
    matchLabels:
      app: postgres
  template:
    metadata:
      labels:
        app: postgres
    spec:
      containers:
      - name: postgres
        image: postgres:15-alpine
        ports:
        - containerPort: 5432
        env:
        - name: POSTGRES_DB
          value: grpc_gateway
        - name: POSTGRES_PASSWORD
          valueFrom:
            secretKeyRef:
              name: postgres-secret
              key: password
        volumeMounts:
        - name: postgres-storage
          mountPath: /var/lib/postgresql/data
      volumes:
      - name: postgres-storage
        persistentVolumeClaim:
          claimName: postgres-pvc
```

### Database Migrations

```bash
# Run migrations before deployment
dotnet run --project src/dotnet-grpc-gateway -- migrate

# Or in Kubernetes:
kubectl run migrations \
  --image=myregistry.azurecr.io/dotnet-grpc-gateway:v1.0.0 \
  --env="ConnectionStrings__DefaultConnection=..." \
  -- --migrate
```

---

## Environment Configuration

### Production

```bash
export ASPNETCORE_ENVIRONMENT=Production
export Gateway__MaxConcurrentConnections=5000
export Gateway__RequestTimeoutMs=30000
export Logging__LogLevel__Default=Warning
export ConnectionStrings__DefaultConnection="Host=prod-db.example.com;Port=5432;Database=grpc_gateway;Username=produser;Password=strong_password"
```

### Staging

```bash
export ASPNETCORE_ENVIRONMENT=Staging
export Gateway__MaxConcurrentConnections=2000
export Logging__LogLevel__Default=Information
export ConnectionStrings__DefaultConnection="Host=staging-db.example.com;..."
```

### Load Balancer Configuration

**Nginx:**

```nginx
upstream grpc_gateway {
    server gateway1:5000;
    server gateway2:5000;
    server gateway3:5000;
}

server {
    listen 80;
    server_name api.example.com;

    location / {
        proxy_pass http://grpc_gateway;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

**HAProxy:**

```
global
    log stdout local0
    maxconn 50000

defaults
    mode http
    timeout connect 5000ms
    timeout client 50000ms
    timeout server 50000ms

frontend http-in
    bind *:80
    default_backend grpc_gateway

backend grpc_gateway
    balance roundrobin
    server gw1 gateway1:5000 check
    server gw2 gateway2:5000 check
    server gw3 gateway3:5000 check
```

---

## Health Checks & Monitoring

### Kubernetes Probes

The deployment includes three probe types:

1. **Liveness Probe** (`/api/health/live`): Is the container alive?
2. **Readiness Probe** (`/api/health/ready`): Is the gateway ready to serve traffic?
3. **Startup Probe** (optional): Has the application started?

```yaml
livenessProbe:
  httpGet:
    path: /api/health/live
    port: 5000
  initialDelaySeconds: 30
  periodSeconds: 10

readinessProbe:
  httpGet:
    path: /api/health/ready
    port: 5000
  initialDelaySeconds: 10
  periodSeconds: 5
```

### Monitoring & Alerting

**Prometheus Metrics Endpoint:**

```bash
curl http://localhost:5000/api/metrics/performance
```

**Alert Rules:**

```yaml
groups:
- name: grpc_gateway_alerts
  interval: 30s
  rules:
  - alert: HighErrorRate
    expr: grpc_gateway_error_rate > 0.05
    for: 5m
    annotations:
      summary: "High error rate detected"

  - alert: HighLatency
    expr: grpc_gateway_p99_latency_ms > 5000
    for: 5m
    annotations:
      summary: "P99 latency exceeds 5 seconds"

  - alert: LowCacheHitRate
    expr: grpc_gateway_cache_hit_rate < 0.5
    for: 10m
    annotations:
      summary: "Cache hit rate below 50%"
```

---

## Scaling

### Horizontal Scaling

Deploy multiple gateway instances behind a load balancer:

```bash
# Scale to 5 replicas
kubectl scale deployment grpc-gateway --replicas=5 -n grpc-gateway

# Or update HPA max replicas
kubectl patch hpa grpc-gateway-hpa --patch '{"spec":{"maxReplicas":15}}' -n grpc-gateway
```

### Vertical Scaling

Increase per-instance resources:

```yaml
resources:
  requests:
    cpu: 500m        # Increased from 250m
    memory: 512Mi    # Increased from 256Mi
  limits:
    cpu: 2000m       # Increased from 1000m
    memory: 1Gi      # Increased from 512Mi
```

### Database Scaling

For high-volume deployments:

```bash
# Create read replica
kubectl apply -f postgres-replica.yaml

# Or use managed database (RDS, CloudSQL, etc.)
```

---

## Security

### TLS/SSL

Enable HTTPS in production:

```yaml
env:
- name: ASPNETCORE_URLS
  value: "https://0.0.0.0:5000"
- name: ASPNETCORE_Kestrel__Certificates__Default__Path
  value: "/secrets/tls.crt"
- name: ASPNETCORE_Kestrel__Certificates__Default__KeyPath
  value: "/secrets/tls.key"
```

### Network Policies

Restrict ingress/egress:

```yaml
apiVersion: networking.k8s.io/v1
kind: NetworkPolicy
metadata:
  name: grpc-gateway-netpolicy
  namespace: grpc-gateway
spec:
  podSelector:
    matchLabels:
      app: grpc-gateway
  policyTypes:
  - Ingress
  - Egress
  ingress:
  - from:
    - namespaceSelector:
        matchLabels:
          name: ingress-nginx
    ports:
    - protocol: TCP
      port: 5000
  egress:
  - to:
    - namespaceSelector: {}
    ports:
    - protocol: TCP
      port: 5432  # PostgreSQL
```

### Pod Security

```yaml
apiVersion: policy/v1beta1
kind: PodSecurityPolicy
metadata:
  name: grpc-gateway-psp
spec:
  privileged: false
  runAsUser:
    rule: 'MustRunAsNonRoot'
  fsGroup:
    rule: 'RunAsAny'
  volumes:
  - 'configMap'
  - 'emptyDir'
  - 'projected'
  - 'secret'
  - 'downwardAPI'
  - 'persistentVolumeClaim'
```

---

## Disaster Recovery

### Backup Strategy

```bash
# Backup PostgreSQL
kubectl exec -it postgres-0 -n grpc-gateway -- \
  pg_dump -U postgres grpc_gateway > backup-$(date +%Y%m%d).sql

# Backup to cloud storage (AWS S3)
kubectl exec -it postgres-0 -n grpc-gateway -- \
  pg_dump -U postgres grpc_gateway | \
  aws s3 cp - s3://my-backups/grpc-gateway/backup-$(date +%Y%m%d).sql
```

### Restore Procedure

```bash
# Restore from backup
kubectl exec -it postgres-0 -n grpc-gateway -- \
  psql -U postgres grpc_gateway < backup-20260504.sql
```

### High Availability

Deploy with multiple replicas and persistent storage:

```yaml
spec:
  replicas: 5  # Minimum 3 for HA
  podAntiAffinity:
    preferredDuringSchedulingIgnoredDuringExecution:
    - weight: 100
      podAffinityTerm:
        labelSelector:
          matchExpressions:
          - key: app
            operator: In
            values:
            - grpc-gateway
        topologyKey: kubernetes.io/hostname
```

---

## Performance Tuning

### Connection Pool Size

```json
{
  "Gateway": {
    "MaxConcurrentConnections": 5000
  }
}
```

### Cache Configuration

```json
{
  "Gateway": {
    "Metrics": {
      "RetentionDays": 7,
      "CollectionIntervalSeconds": 30
    }
  }
}
```

### Logging Level

Production: `Warning`
Staging: `Information`
Development: `Debug`

---

## Troubleshooting

### Pod Not Starting

```bash
kubectl describe pod <pod-name> -n grpc-gateway
kubectl logs <pod-name> -n grpc-gateway
```

### Database Connection Issues

```bash
# Test connection
kubectl exec -it <pod-name> -n grpc-gateway -- \
  psql -h postgres -U postgres -d grpc_gateway -c "SELECT 1"
```

### High Memory Usage

```bash
# Check memory usage
kubectl top pod -n grpc-gateway

# Reduce cache retention
kubectl set env deployment/grpc-gateway \
  -c gateway \
  Gateway__Metrics__RetentionDays=7 \
  -n grpc-gateway
```

---

## Rollback

```bash
# View rollout history
kubectl rollout history deployment/grpc-gateway -n grpc-gateway

# Rollback to previous version
kubectl rollout undo deployment/grpc-gateway -n grpc-gateway

# Rollback to specific revision
kubectl rollout undo deployment/grpc-gateway --to-revision=2 -n grpc-gateway
```

---

## Conclusion

For production deployments:

1. ✓ Use Kubernetes or managed container service
2. ✓ Configure health checks and autoscaling
3. ✓ Set up monitoring and alerting
4. ✓ Implement backup/restore procedures
5. ✓ Use TLS/SSL for all communications
6. ✓ Keep database separate and replicated
7. ✓ Test disaster recovery procedures regularly
