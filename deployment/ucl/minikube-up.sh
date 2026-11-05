#!/usr/bin/env bash

# SPDX-License-Identifier: apache-2.0

set -euo pipefail

SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
REPO_DIR="$(cd -- "${SCRIPT_DIR}/../.." && pwd)"

PROFILE="${MINIKUBE_PROFILE:-capx-dev}"
NAMESPACE="${CAPX_NAMESPACE:-capx-local}"
RELEASE="${CAPX_RELEASE:-capx}"
INGRESS_HOST="${CAPX_INGRESS_HOST:-capx.arc-devops.condenser.arc.ucl.ac.uk}"
APP_URL="${CAPX_APP_URL:-https://${INGRESS_HOST}}"
IMAGE="capx:dev"
SECRET="capx-secrets"
SERVICE="${RELEASE}-capx"

for command in docker helm kubectl minikube openssl; do
  if ! command -v "${command}" >/dev/null 2>&1; then
    echo "Required command not found: ${command}" >&2
    exit 1
  fi
done

echo "==> Start Minikube profile ${PROFILE}"
minikube start -p "${PROFILE}" --driver=docker --ports=80:80

if ! docker port "${PROFILE}" 80/tcp >/dev/null 2>&1; then
  echo "Minikube profile ${PROFILE} does not publish port 80." >&2
  echo "Recreate it with: minikube delete -p ${PROFILE}" >&2
  echo "Then rerun this script." >&2
  exit 1
fi

echo "==> Enable Traefik Ingress controller"
minikube addons disable ingress -p "${PROFILE}"
minikube addons enable traefik -p "${PROFILE}"

echo "==> Build CapX local image"
docker build --build-arg BUILD_CONFIG=Local -t "${IMAGE}" "${REPO_DIR}"

echo "==> Load image into Minikube"
minikube image load "${IMAGE}" -p "${PROFILE}"

echo "==> Ensure namespace ${NAMESPACE} exists"
kubectl --context "${PROFILE}" create namespace "${NAMESPACE}" \
  --dry-run=client -o yaml | kubectl --context "${PROFILE}" apply -f -

if ! kubectl --context "${PROFILE}" --namespace "${NAMESPACE}" \
  get secret "${SECRET}" >/dev/null 2>&1; then
  echo "==> Create local development secret ${SECRET}"
  kubectl --context "${PROFILE}" --namespace "${NAMESPACE}" \
    create secret generic "${SECRET}" \
    --from-literal=connection-string='Data Source=/app/state/PPMTool.db' \
    --from-literal=leave-bookings-connection-string=unused \
    --from-literal=api-key-secret="$(openssl rand -hex 32)"
fi

echo "==> Install or upgrade Helm release ${RELEASE}"
helm upgrade --install "${RELEASE}" "${SCRIPT_DIR}/helm" \
  --kube-context "${PROFILE}" \
  --namespace "${NAMESPACE}" \
  --create-namespace \
  --set image.repository=capx \
  --set image.tag=dev \
  --set image.pullPolicy=Never \
  --set config.aspnetcoreEnvironment=Development \
  --set config.dbProvider=sqlite \
  --set ingress.enabled=true \
  --set ingress.className=traefik \
  --set-string ingress.host="${INGRESS_HOST}" \
  --set-string config.authHostUrl="${APP_URL}" \
  --set-string config.superuserName='Local Admin' \
  --set-string config.superuserUsername=admin \
  --set-string config.superuserEmail=admin@example.org \
  --wait --timeout 5m

kubectl --context "${PROFILE}" --namespace "${NAMESPACE}" \
  rollout restart "deployment/${SERVICE}"
kubectl --context "${PROFILE}" --namespace "${NAMESPACE}" \
  rollout status "deployment/${SERVICE}" --timeout=5m

echo "CapX is available at ${APP_URL}"
echo "The Minikube Ingress controller accepts HTTP on port 80."
