# Deployment Notes

## Create A VM

Create the VM in the `arc-devops-ns` namespace using these settings. The
resources match the current trial and are not production sizing
recommendations.

### Basics

- Name: `paddy-capx` (or another unique VM name)
- CPU: 2
- Memory: 8 GiB
- SSH key: `arc-devops-ns/paddy`
- Leave CPU and memory hotplug disabled.

### Volumes

- Name: `disk-0`
- Type: `disk`
- Image: `harvester-public/Ubuntu 26.04`
- Size: 40 GiB
- Bus: VirtIO

### Networks

Add one network interface:

- Name: `default`
- Model: `virtio`
- Network: `arc-devops-ns/default`
- Type: `bridge`

Leave Node Scheduling and Virtual Machine Scheduling at their defaults.

### Labels

Leave **Labels** empty. Add the ingress labels under **Instance Labels**:

| Key                                | Value    |
|------------------------------------|----------|
| `condenser.ingress/isEnabled`      | `true`   |
| `condenser.ingress.capx/hostname`  | `capx`   |
| `condenser.ingress.capx/port`      | `80`     |
| `condenser.ingress.capx/interface` | `enp1s0` |

The VM's KubeVirt interface is named `default`; the Ubuntu guest interface
that receives its IP is `enp1s0`. The explicit `interface` Instance Label
tells ingress which guest interface to use when it sees multiple interfaces.
Do not add a plain `interface` label under **Labels**. Harvester adds
`harvesterhci.io/vmName` automatically.

### Annotations

Set this annotation under **Annotations**, or enable **Wait for Lease** for
this interface if the form exposes that option:

| Key                                          | Value         |
|----------------------------------------------|---------------|
| `harvesterhci.io/waitForLeaseInterfaceNames` | `["default"]` |

The value must match the network interface **Name** above.

### Advanced Options And Cloud Configuration

- Keep the default OS and machine settings; use `q35` if a machine type is
  required.
- Set the guest hostname to `paddy-capx` if prompted.
- In Cloud Configuration **User Data**, install and start the QEMU guest
  agent:

  ```yaml
  #cloud-config
  package_update: true
  packages:
    - qemu-guest-agent
  runcmd:
    - - systemctl
      - enable
      - --now
      - qemu-guest-agent.service
  ```

- Keep the `ssh_authorized_keys` entry for the selected SSH key if it is
  included in the generated User Data. It must be a complete public key on one
  YAML list item; do not put a private key here.
- Leave Network Data at its default or blank when the network provides DHCP.

## Prepare The VM

After the VM starts, connect using its IP and confirm Ubuntu has an address on
`enp1s0`:

```sh
ip -br -4 addr
```

From the repository root on your workstation, copy the preparation script to
the VM, then run it over SSH:

```sh
scp deployment/ucl/ubuntu-prepare.sh ubuntu@<VM-IP>:~/ubuntu-prepare.sh
ssh ubuntu@<VM-IP>
chmod +x ~/ubuntu-prepare.sh
~/ubuntu-prepare.sh
```

The script installs Docker, Helm, kubectl, and Minikube, then clones the
configured repository and branch into `~/capx`. Set `CAPX_REPO_URL` or
`CAPX_REF` first if you need different defaults. It adds your account to the
`docker` group; log out of SSH and reconnect before running Minikube:

```sh
cd ~/capx
./deployment/ucl/minikube-up.sh
```

## Configure Condenser Ingress

The Minikube script starts the Docker-based profile with port 80 published,
enables the Traefik Ingress controller, and installs the chart's Ingress
resource. Condenser terminates HTTPS and sends HTTP to port 80 on the VM.
The VM's Ingress port label must therefore be `80`; it is not the CapX
container port `8080` or the former port-forward port `5001`.

The expected URL is
`https://capx.arc-devops.condenser.arc.ucl.ac.uk`. If the Minikube profile
already exists without the port 80 mapping, delete and recreate it once:

```sh
minikube delete -p capx-dev
```

Then run `deployment/ucl/minikube-up.sh`. The profile's SQLite data is
disposable.
