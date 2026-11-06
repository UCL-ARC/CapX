# Deployment Notes

## Create A VM

Create the VM using these settings. The resources match the current trial and
are not production sizing recommendations. Default values have not been
included.

### Basics

- Name: a unique name
- Description: add a description
- CPU: 2
- Memory: 8 GiB
- SSH key: select an SSH key

### Volumes

- Image: `harvester-public/Ubuntu 26.04`
- Size: 40 GiB

### Networks

Add one network interface:

- Network: `<namespace>/default`
- Type: `bridge`

Leave Node Scheduling and Virtual Machine Scheduling at their defaults.

### Instance Labels

Add the ingress labels:

| Key                                | Value    |
| ---------------------------------- | -------- |
| `condenser.ingress/isEnabled`      | `true`   |
| `condenser.ingress.capx/hostname`  | `capx`   |
| `condenser.ingress.capx/port`      | `80`     |
| `condenser.ingress.capx/interface` | `enp1s0` |

The VM's KubeVirt interface is named `default`; the Ubuntu guest interface that
receives its IP is `enp1s0`. The explicit `interface` Instance Label tells
ingress which guest interface to use when it sees multiple interfaces.

### Annotations

Set this annotation under **Annotations**:

| Key                                          | Value         |
| -------------------------------------------- | ------------- |
| `harvesterhci.io/waitForLeaseInterfaceNames` | `["default"]` |

The value must match the network interface **Name** above.

## Access The VM Over SSH

Set up SSH access using the
[Condenser SSH guide](https://condenser.arc.ucl.ac.uk/documentation/ssh_login).
Use the `condenser` SSH host alias from that guide. From the repository root
on your workstation, set `VM_IP` to the VM's current IP from Rancher, copy
the preparation script, and connect through the bastion:

```sh
VM_IP=replace-with-vm-ip
scp -o ProxyJump=condenser deployment/ucl/ubuntu-prepare.sh \
  "ubuntu@${VM_IP}:~/ubuntu-prepare.sh"
ssh -J condenser "ubuntu@${VM_IP}"
```

## Prepare The VM

In the SSH session, confirm Ubuntu has an address on `enp1s0`:

```sh
ip -br -4 addr
```

Then run the preparation script:

```sh
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
resource. Condenser terminates HTTPS and sends HTTP to port 80 on the VM. The
VM's Ingress port label must therefore be `80`; it is not the CapX container
port `8080` or the former port-forward port `5001`.

The expected URL is `https://capx.arc-devops.condenser.arc.ucl.ac.uk`. The
service is password protected; the setup script saves the generated credentials
on the VM in `~/.config/capx/ingress-credentials`.

### Smoke Test

From a workstation with access to Condenser, run the smoke test and enter the
Ingress password when prompted:

```sh
./deployment/ucl/smoke-test.sh
```

To avoid retyping, copy the credentials file from the VM over SSH, then pass
its path to the script:

```sh
scp -o ProxyJump=condenser \
  "ubuntu@${VM_IP}:~/.config/capx/ingress-credentials" \
  /tmp/capx-ingress-credentials
chmod 600 /tmp/capx-ingress-credentials
CAPX_INGRESS_PASSWORD_FILE=/tmp/capx-ingress-credentials \
  ./deployment/ucl/smoke-test.sh
rm /tmp/capx-ingress-credentials
```

Then open the URL in a browser, enter the same Ingress credentials, and log in
as **Local Admin**. Confirm the app loads and a live Blazor page responds.
The script checks HTTPS, password enforcement, the local sign-in cookie, and
Blazor SignalR negotiation. In browser developer tools, check that the
`/_blazor` WebSocket request connects, then verify the UI remains interactive.

If the Minikube profile already exists without the port 80 mapping, delete and
recreate it once:

```sh
minikube delete -p capx-dev
```

Then run `deployment/ucl/minikube-up.sh`. The profile's SQLite data is
disposable.
