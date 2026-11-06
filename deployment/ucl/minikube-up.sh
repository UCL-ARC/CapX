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
BUILD_CONFIG="${CAPX_BUILD_CONFIG:-Local}"
case "${BUILD_CONFIG}" in
  Local)
    IMAGE="capx:dev"
    ASPNETCORE_ENVIRONMENT=Development
    AUTH_TYPE=CAS
    INGRESS_BASIC_AUTH=true
    ;;
  Release)
    IMAGE="capx:release"
    ASPNETCORE_ENVIRONMENT=Production
    AUTH_TYPE=AzureAd
    INGRESS_BASIC_AUTH=false
    ;;
  *)
    echo "CAPX_BUILD_CONFIG must be Local or Release." >&2
    exit 1
    ;;
esac
SECRET="capx-secrets"
INGRESS_SECRET="capx-ingress-auth"
ENTRA_SECRET="${CAPX_ENTRA_SECRET_NAME:-capx-entra-secret}"
SERVICE="${RELEASE}-capx"
INGRESS_CREDENTIALS_FILE="${HOME}/.config/capx/ingress-credentials"

for command in docker helm kubectl minikube openssl; do
  if ! command -v "${command}" >/dev/null 2>&1; then
    echo "Required command not found: ${command}" >&2
    exit 1
  fi
done

if [[ "${BUILD_CONFIG}" == "Release" ]]; then
  for variable in CAPX_ENTRA_DOMAIN CAPX_ENTRA_TENANT_ID \
    CAPX_ENTRA_CLIENT_ID CAPX_ENTRA_CLIENT_SECRET_FILE \
    CAPX_SUPERUSER_NAME CAPX_SUPERUSER_USERNAME CAPX_SUPERUSER_EMAIL; do
    if [[ -z "${!variable:-}" ]]; then
      echo "Set ${variable} to use the Release configuration." >&2
      exit 1
    fi
  done
  if [[ ! -s "${CAPX_ENTRA_CLIENT_SECRET_FILE}" || \
    ! -r "${CAPX_ENTRA_CLIENT_SECRET_FILE}" ]]; then
    echo "CAPX_ENTRA_CLIENT_SECRET_FILE must be readable and non-empty." >&2
    exit 1
  fi
fi

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

echo "==> Build CapX ${BUILD_CONFIG} image"
docker build --build-arg "BUILD_CONFIG=${BUILD_CONFIG}" \
  -t "${IMAGE}" "${REPO_DIR}"

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

if [[ "${BUILD_CONFIG}" == "Release" ]]; then
  echo "==> Store the Entra client secret in Kubernetes"
  kubectl --context "${PROFILE}" --namespace "${NAMESPACE}" \
    create secret generic "${ENTRA_SECRET}" \
    --from-file="entra-client-secret=${CAPX_ENTRA_CLIENT_SECRET_FILE}" \
    --dry-run=client -o yaml | kubectl --context "${PROFILE}" \
    --namespace "${NAMESPACE}" apply -f -
else
  echo "==> Ensure password protection for the Ingress"
  umask 077
  mkdir -p "$(dirname -- "${INGRESS_CREDENTIALS_FILE}")"
  chmod 700 "$(dirname -- "${INGRESS_CREDENTIALS_FILE}")"
  if [[ ! -s "${INGRESS_CREDENTIALS_FILE}" ]]; then
    INGRESS_PASSWORD="$(openssl rand -hex 24)"
    printf 'username: capx-trial\npassword: %s\n' \
      "${INGRESS_PASSWORD}" >"${INGRESS_CREDENTIALS_FILE}"
  else
    INGRESS_PASSWORD="$(sed -n 's/^password: //p' \
      "${INGRESS_CREDENTIALS_FILE}")"
    if [[ -z "${INGRESS_PASSWORD}" ]]; then
      echo "No password found in ${INGRESS_CREDENTIALS_FILE}." >&2
      exit 1
    fi
  fi
  chmod 600 "${INGRESS_CREDENTIALS_FILE}"
  INGRESS_PASSWORD_HASH="$(openssl passwd -apr1 \
    -salt "$(openssl rand -hex 4)" "${INGRESS_PASSWORD}")"
  kubectl --context "${PROFILE}" --namespace "${NAMESPACE}" \
    create secret generic "${INGRESS_SECRET}" \
    --from-literal="users=capx-trial:${INGRESS_PASSWORD_HASH}" \
    --dry-run=client -o yaml | kubectl --context "${PROFILE}" \
    --namespace "${NAMESPACE}" apply -f -
fi

echo "==> Install or upgrade Helm release ${RELEASE}"
HELM_ARGS=(upgrade --install "${RELEASE}" "${SCRIPT_DIR}/helm"
  --kube-context "${PROFILE}"
  --namespace "${NAMESPACE}"
  --create-namespace
  --set image.repository=capx
  --set "image.tag=${IMAGE##*:}"
  --set image.pullPolicy=Never
  --set "config.aspnetcoreEnvironment=${ASPNETCORE_ENVIRONMENT}"
  --set config.dbProvider=sqlite
  --set ingress.enabled=true
  --set ingress.className=traefik
  --set "ingress.basicAuth.enabled=${INGRESS_BASIC_AUTH}"
  --set-string "ingress.host=${INGRESS_HOST}"
  --set-string "config.authHostUrl=${APP_URL}"
  --set "config.authType=${AUTH_TYPE}"
  --wait --timeout 5m)

if [[ "${BUILD_CONFIG}" == "Release" ]]; then
  HELM_ARGS+=(
    --set-string "config.entraDomain=${CAPX_ENTRA_DOMAIN}"
    --set-string "config.entraTenantId=${CAPX_ENTRA_TENANT_ID}"
    --set-string "config.entraClientId=${CAPX_ENTRA_CLIENT_ID}"
    --set-string "config.entraClientSecretSecretName=${ENTRA_SECRET}"
    --set-string "config.superuserName=${CAPX_SUPERUSER_NAME}"
    --set-string "config.superuserUsername=${CAPX_SUPERUSER_USERNAME}"
    --set-string "config.superuserEmail=${CAPX_SUPERUSER_EMAIL}"
  )
else
  HELM_ARGS+=(
    --set-string ingress.basicAuth.secretName="${INGRESS_SECRET}"
    --set-string config.superuserName='Local Admin'
    --set-string config.superuserUsername=admin
    --set-string config.superuserEmail=admin@example.org
  )
fi

helm "${HELM_ARGS[@]}"

kubectl --context "${PROFILE}" --namespace "${NAMESPACE}" \
  rollout restart "deployment/${SERVICE}"
kubectl --context "${PROFILE}" --namespace "${NAMESPACE}" \
  rollout status "deployment/${SERVICE}" --timeout=5m

echo "CapX is available at ${APP_URL}"
if [[ "${BUILD_CONFIG}" == "Local" ]]; then
  echo "Ingress credentials are in ${INGRESS_CREDENTIALS_FILE}"
else
  echo "Entra single sign-on is enabled."
fi
echo "The Minikube Ingress controller accepts HTTP on port 80."
