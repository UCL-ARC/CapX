# Deployment Notes

## Create A VM

Create the VM using these settings. The resources match the current trial and
are not production sizing recommendations.

### Basics

- Name: a unique name
- CPU: 2
- Memory: 8 GiB
- SSH key: select an SSH key

### Volumes

- Type: `disk`
- Image: `harvester-public/Ubuntu 26.04`
- Size: 40 GiB
- Bus: VirtIO

### Networks

Add one network interface:

- Model: `virtio`
- Network: `<namespace>/default`
- Type: `bridge`

Leave Node Scheduling and Virtual Machine Scheduling at their defaults.

### Instance Labels

Add the ingress labels:

| Key                                | Value    |
|------------------------------------|----------|
| `condenser.ingress/isEnabled`      | `true`   |
| `condenser.ingress.capx/hostname`  | `capx`   |
| `condenser.ingress.capx/port`      | `80`     |
| `condenser.ingress.capx/interface` | `enp1s0` |

The VM's KubeVirt interface is named `default`; the Ubuntu guest interface
that receives its IP is `enp1s0`. The explicit `interface` Instance Label
tells ingress which guest interface to use when it sees multiple interfaces.

### Annotations

Set this annotation under **Annotations**, or enable **Wait for Lease** for
this interface if the form exposes that option:

| Key                                          | Value         |
|----------------------------------------------|---------------|
| `harvesterhci.io/waitForLeaseInterfaceNames` | `["default"]` |

The value must match the network interface **Name** above.

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
