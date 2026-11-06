#!/usr/bin/env bash

# SPDX-License-Identifier: apache-2.0

set -euo pipefail

BASE_URL="${1:-https://capx.arc-devops.condenser.arc.ucl.ac.uk}"
BASE_URL="${BASE_URL%/}"
AUTH_USER="capx-trial"

if [[ "${BASE_URL}" != https://* ]]; then
  echo "The smoke test requires an HTTPS URL." >&2
  exit 1
fi

for command in curl grep; do
  if ! command -v "${command}" >/dev/null 2>&1; then
    echo "Required command not found: ${command}" >&2
    exit 1
  fi
done

if [[ -n "${CAPX_INGRESS_PASSWORD_FILE:-}" ]]; then
  if [[ ! -r "${CAPX_INGRESS_PASSWORD_FILE}" ]]; then
    echo "Cannot read ${CAPX_INGRESS_PASSWORD_FILE}." >&2
    exit 1
  fi
  AUTH_PASSWORD="$(sed -n 's/^password: //p' \
    "${CAPX_INGRESS_PASSWORD_FILE}")"
else
  read -r -s -p "Ingress password for ${AUTH_USER}: " AUTH_PASSWORD
  printf '\n'
fi

if [[ -z "${AUTH_PASSWORD}" ]]; then
  echo "No ingress password was provided." >&2
  exit 1
fi

umask 077
TEMP_DIR="$(mktemp -d)"
trap 'rm -rf "${TEMP_DIR}"' EXIT
AUTH_CONFIG="${TEMP_DIR}/curl.conf"
COOKIE_JAR="${TEMP_DIR}/cookies.txt"
ROOT_PAGE="${TEMP_DIR}/root.html"
LOGIN_PAGE="${TEMP_DIR}/login.html"
NEGOTIATE_RESPONSE="${TEMP_DIR}/negotiate.json"
UNAUTH_HEADERS="${TEMP_DIR}/unauthenticated.headers"

printf 'user = "%s:%s"\n' "${AUTH_USER}" "${AUTH_PASSWORD}" \
  >"${AUTH_CONFIG}"
unset AUTH_PASSWORD
: >"${COOKIE_JAR}"

echo "==> Check unauthenticated requests are rejected"
STATUS="$(curl --silent --show-error --max-time 20 \
  --noproxy '*' --dump-header "${UNAUTH_HEADERS}" --output /dev/null \
  --write-out '%{http_code}' "${BASE_URL}/")"
if [[ "${STATUS}" != 401 ]] \
  || ! grep -qi '^www-authenticate: Basic' "${UNAUTH_HEADERS}"; then
  echo "Expected a Basic Auth challenge (HTTP 401); got ${STATUS}." >&2
  exit 1
fi

echo "==> Check authenticated HTTPS access"
STATUS="$(curl --silent --show-error --max-time 20 \
  --noproxy '*' --config "${AUTH_CONFIG}" --output "${ROOT_PAGE}" \
  --write-out '%{http_code}' "${BASE_URL}/")"
if [[ "${STATUS}" != 200 ]]; then
  echo "Expected HTTP 200 with credentials; got ${STATUS}." >&2
  exit 1
fi
if ! grep -q '_framework/blazor.server.js' "${ROOT_PAGE}"; then
  echo "CapX page loaded, but the Blazor Server client was not present." >&2
  exit 1
fi

echo "==> Check Local admin sign-in sets an authentication cookie"
STATUS="$(curl --silent --show-error --location --max-time 30 \
  --noproxy '*' --config "${AUTH_CONFIG}" \
  --cookie "${COOKIE_JAR}" \
  --cookie-jar "${COOKIE_JAR}" --output "${LOGIN_PAGE}" \
  --write-out '%{http_code}' \
  "${BASE_URL}/Account/Login?username=admin&returnUrl=%2F")"
if [[ "${STATUS}" != 200 ]] || ! grep -q 'CapXAuth' "${COOKIE_JAR}"; then
  echo "Local admin sign-in did not complete or set the CapXAuth cookie." >&2
  exit 1
fi

echo "==> Check Blazor SignalR negotiation through the proxy"
STATUS="$(curl --silent --show-error --max-time 20 \
  --noproxy '*' --config "${AUTH_CONFIG}" \
  --cookie "${COOKIE_JAR}" \
  --cookie-jar "${COOKIE_JAR}" --data '' \
  --output "${NEGOTIATE_RESPONSE}" --write-out '%{http_code}' \
  "${BASE_URL}/_blazor/negotiate?negotiateVersion=1")"
if [[ "${STATUS}" != 200 ]] \
  || ! grep -q 'availableTransports' "${NEGOTIATE_RESPONSE}"; then
  echo "Blazor SignalR negotiation failed (HTTP ${STATUS})." >&2
  exit 1
fi

echo "Smoke checks passed. Complete the browser check in the README."
