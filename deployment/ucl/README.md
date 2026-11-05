# Deployment Notes

## Trial VM

The trial VM was created through Condenser with:

- Ubuntu 26.04
- 2 vCPUs
- 8 GiB RAM
- 40 GiB disk

These are the resources used for the current trial, not production sizing
recommendations.

## VM Provisioning And Application Deployment

Helm deploys CapX into the Minikube cluster running inside the VM. It does not
create or configure the VM. Terraform can manage that Harvester VM separately;
switching the CapX deployment from Helm to Terraform would not replace the
Helm chart.

The Minikube profile must publish port 80 from its Docker node to port 80 on
the VM. The setup script does this with `--ports=80:80`, enables Traefik, and
installs the chart's Ingress resource. If the profile was created before this
port mapping was added, delete and recreate it once with
`minikube delete -p capx-dev` before running the script.

Configure the VM's Instance Labels so Condenser ingress targets the VM's
`enp1s0` guest interface and HTTP port 80:

```yaml
condenser.ingress/isEnabled: "true"
condenser.ingress.capx/hostname: capx
condenser.ingress.capx/port: "80"
condenser.ingress.capx/interface: enp1s0
```

The hostname resolves as
`https://capx.arc-devops.condenser.arc.ucl.ac.uk`. Condenser terminates TLS and
forwards HTTP to port 80 on the VM. The port label refers to the Minikube
Ingress controller's listening port, not CapX's container port 8080 or the
service's previous port-forward port 5001.

The `interface` label is needed because the VM has multiple network
interfaces. The guest interface `enp1s0` holds the VM address; the Harvester
interface name `default` is not the guest OS interface name used for routing.
