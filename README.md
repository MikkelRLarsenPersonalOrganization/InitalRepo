### Postgres with application and dapr sidecar
```
# PersistentVolumeClaim til PostgreSQL
apiVersion: v1
kind: PersistentVolumeClaim
metadata:
  name: demo-pvc
spec:
  accessModes:
    - ReadWriteOnce
  resources:
    requests:
      storage: 1Gi
  storageClassName: standard

---
# PostgreSQL Deployment
apiVersion: apps/v1
kind: Deployment
metadata:
  name: postgres
spec:
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
          image: postgres:15
          ports:
            - containerPort: 5432
          env:
            - name: POSTGRES_USER
              value: "user"
            - name: POSTGRES_PASSWORD
              value: "root"
            - name: POSTGRES_DB
              value: "demodb"
          volumeMounts:
            - name: postgres-storage
              mountPath: /var/lib/postgresql/data
      volumes:
        - name: postgres-storage
          persistentVolumeClaim:
            claimName: demo-pvc

---
# PostgreSQL Service
apiVersion: v1
kind: Service
metadata:
  name: postgres-db
spec:
  selector:
    app: postgres
  ports:
    - port: 5432
      targetPort: 5432

---
# pgAdmin Deployment
apiVersion: apps/v1
kind: Deployment
metadata:
  name: pgadmin
spec:
  replicas: 1
  selector:
    matchLabels:
      app: pgadmin
  template:
    metadata:
      labels:
        app: pgadmin
    spec:
      containers:
        - name: pgadmin
          image: dpage/pgadmin4:7.8
          ports:
            - containerPort: 80
          env:
            - name: PGADMIN_DEFAULT_EMAIL
              value: "admin@admin.com"
            - name: PGADMIN_DEFAULT_PASSWORD
              value: "admin"

---
# pgAdmin Service (LoadBalancer)
apiVersion: v1
kind: Service
metadata:
  name: pgadmin-service
spec:
  type: LoadBalancer
  selector:
    app: pgadmin
  ports:
    - port: 9091
      targetPort: 80

---
# Secret med PostgreSQL connection string
apiVersion: v1
kind: Secret
metadata:
  name: postgres-db-secret
type: Opaque
stringData:
  connectionString: "Host=postgres-db;Port=5432;Database=demodb;Username=user;Password=root"

---
# CI/CD App Service (LoadBalancer)
apiVersion: v1
kind: Service
metadata:
  name: cicdexample
  labels:
    app: cicdexample
spec:
  selector:
    app: cicdexample
  ports:
    - protocol: TCP
      port: 8001
      targetPort: 8001
  type: LoadBalancer

---
# CI/CD App Deployment
apiVersion: apps/v1
kind: Deployment
metadata:
  name: cicdexample
  labels:
    app: cicdexample
spec:
  replicas: 1
  selector:
    matchLabels:
      app: cicdexample
  template:
    metadata:
      labels:
        app: cicdexample
      annotations:
        dapr.io/enabled: "true"
        dapr.io/app-id: "cicdexample"
        dapr.io/app-port: "8001"
        dapr.io/enable-api-logging: "true"
    spec:
      containers:
        - name: cicdexample
          image: ghcr.io/mikkelrlarsenpersonalorganization/cicdexample:5c47fcdcf09cd1cb347833ff0f9fbec3cb11217e
          ports:
            - containerPort: 8001
          imagePullPolicy: Always
          env:
            - name: ConnectionStrings__DefaultConnection
              valueFrom:
                secretKeyRef:
                  name: postgres-db-secret
                  key: connectionString
```

### Dapr Statestore and pubsub with Redis
```
apiVersion: v1
kind: PersistentVolumeClaim
metadata:
  name: daprstatestore-pvc
spec:
  accessModes:
    - ReadWriteOnce
  resources:
    requests:
      storage: 1Gi
  storageClassName: standard

---
apiVersion: apps/v1
kind: Deployment
metadata:
  name: redis
spec:
  replicas: 1
  selector:
    matchLabels:
      app: redis
  template:
    metadata:
      labels:
        app: redis
    spec:
      containers:
        - name: redis
          image: redis:6
          args: ["--appendonly", "yes"]
          ports:
            - containerPort: 6379
          volumeMounts:
            - name: data
              mountPath: /data
      volumes:
        - name: data
          persistentVolumeClaim:
            claimName: daprstatestore-pvc
 
---
apiVersion: v1
kind: Service
metadata:
  name: redis-db
spec:
  selector:
    app: redis
  ports:
    - port: 6379
      targetPort: 6379

---
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: daprstatestore
spec:
  type: state.redis
  version: v1
  metadata:
  - name: redisHost
    value: redis-db:6379
  - name: redisPassword
    value: ""
  - name: actorStateStore
    value: "true"

---
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: daprpubsub
spec:
  type: pubsub.redis
  version: v1
  metadata:
  - name: redisHost
    value: redis-db:6379
  - name: redisPassword
    value: ""
    
---
apiVersion: v1
kind: Service
metadata:
  name: redisinsight-service 
spec:
  type: LoadBalancer
  ports:
    - port: 9081
      targetPort: 5540
  selector:
    app: redisinsight
    
---
apiVersion: apps/v1
kind: Deployment
metadata:
  name: redisinsight 
  labels:
    app: redisinsight 
spec:
  replicas: 1 
  selector:
    matchLabels:
      app: redisinsight
  template: 
    metadata:
      labels:
        app: redisinsight 
    spec:
      containers:
      - name:  redisinsight 
        image: redis/redisinsight:latest 
        imagePullPolicy: IfNotPresent
        ports:
        - containerPort: 5540
```
