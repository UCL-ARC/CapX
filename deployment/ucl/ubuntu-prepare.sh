#!/usr/bin/env bash

# SPDX-License-Identifier: apache-2.0

set -euo pipefail

CAPX_REPO_URL="${CAPX_REPO_URL:-https://github.com/UCL-ARC/CapX.git}"
CAPX_REF="${CAPX_REF:-paddy/issue-1}"
CAPX_DIR="${CAPX_DIR:-${HOME}/capx}"
ARCH="$(dpkg --print-architecture)"

sudo apt-get update
sudo apt-get install -y ca-certificates curl git

curl -fsSL https://get.docker.com | sudo sh
sudo usermod -aG docker "$(id -un)"

curl -fsSL \
  https://raw.githubusercontent.com/helm/helm/main/scripts/get-helm-3 \
  | bash

KUBECTL_VERSION="$(curl -fsSL https://dl.k8s.io/release/stable.txt)"
sudo curl -fsSL \
  "https://dl.k8s.io/release/${KUBECTL_VERSION}/bin/linux/${ARCH}/kubectl" \
  -o /usr/local/bin/kubectl
sudo chmod +x /usr/local/bin/kubectl

MINIKUBE_DEB="/tmp/minikube_${ARCH}.deb"
curl -fsSL \
  "https://storage.googleapis.com/minikube/releases/latest/\
minikube_latest_${ARCH}.deb" \
  -o "${MINIKUBE_DEB}"
sudo dpkg -i "${MINIKUBE_DEB}"
rm "${MINIKUBE_DEB}"

git clone --branch "${CAPX_REF}" --single-branch \
  "${CAPX_REPO_URL}" "${CAPX_DIR}"

echo "Log out and back in, then run: ${CAPX_DIR}/deployment/ucl/minikube-up.sh"
