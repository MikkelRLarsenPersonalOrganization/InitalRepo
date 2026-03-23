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
    - port: 9081
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
    spec:
      containers:
        - name: cicdexample
          image: ghcr.io/mikkelrlarsenpersonalorganization/cicdexample:f9ac989d76008ff99651a8e549290ca11ad12573
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
