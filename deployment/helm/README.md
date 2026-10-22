# CapX Helm chart

This Helm chart deploys the CapX web application into an **existing Kubernetes
workload cluster**. It does not provision the underlying infrastructure or
cluster. Confirm that the target cluster provides the ingress, storage, and
database services your deployment requires. The chart expects a PostgreSQL or
SQL Server database reachable from the cluster; SQLite on pod-local storage is
not suitable for a durable Kubernetes deployment. The chart deliberately runs
one replica because CapX currently applies EF Core migrations as part of app
startup. Before enabling more replicas, move migrations to a one-shot release
job and verify the app's shared state requirements.

## Local Minikube

For local development, run [`../minikube-up.sh`](../minikube-up.sh) from the
repository root. It builds and loads the `Local` image, installs or upgrades the
chart, and forwards the app to <http://localhost:5001>. This uses SQLite stored
in the pod, so local data is disposable. The manual instructions below are for
installing into an existing workload cluster.

## Configure secrets

Create a Secret in the target namespace. `connection-string` must be the
`PPMToolContextConnection` connection string for the selected provider.
`leave-bookings-connection-string` is required by CapX even if that optional
integration is unused; provide a nonblank placeholder in that case.

```sh
kubectl create namespace capx --dry-run=client -o yaml | kubectl apply -f -
kubectl --namespace capx create secret generic capx-secrets \
  --from-literal=connection-string='Host=db.example;Database=capx;Username=capx;Password=REPLACE_ME' \
  --from-literal=leave-bookings-connection-string='Host=unused;Database=unused;Username=unused;Password=unused' \
  --from-literal=api-key-secret='REPLACE_WITH_AT_LEAST_32_RANDOM_CHARACTERS' \
  --from-literal=entra-client-secret='REPLACE_ME'
```

Use an external secret manager in production where available. Avoid committing
secret values to a values file. If using CAS instead of Entra ID, the unused
Entra secret key can be an empty value.

## Install

Edit a local values file for the image, ingress host/TLS, authentication
settings, superuser details, and optional mail settings. Then install or
upgrade:

```sh
helm upgrade --install capx deployment/helm \
  --values /path/to/your-values.yaml \
  --namespace capx --create-namespace \
  --set image.repository=ghcr.io/your-org/capx \
  --set image.tag=YOUR_IMMUTABLE_TAG \
  --set ingress.enabled=true \
  --set ingress.host=capx.example.com \
  --set config.authHostUrl=https://capx.example.com \
  --set config.dbProvider=postgresql \
  --set config.superuserName='CapX Administrator' \
  --set config.superuserUsername=admin \
  --set config.superuserEmail=admin@example.com
```

Supply TLS under `ingress.tls` in your local values file, matching the secret
created by your cluster's certificate workflow. The data-protection key PVC is
enabled by default; confirm the cluster has a default StorageClass or set
`dataProtection.storageClassName`. Keep the PVC and its backup when uninstalling
or reinstalling the release.

Set `config.seedDummyData` only for disposable environments: startup seeding
deletes and recreates the database contents.
