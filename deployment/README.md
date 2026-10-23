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

For the current VM, test the network lease setting used by the workshop before
recreating the VM with Terraform. The guest network interface is named
`default`, so the VM template annotations should include:

```yaml
harvesterhci.io/waitForLeaseInterfaceNames: '["default"]'
```

The Condenser ingress labels belong on the VM template labels. Keep the
hostname and port labels used by this deployment, and confirm that the VM
reports an IP address in Harvester after it restarts. The guest's `ip addr`
output and a successful `curl` from inside the VM confirm guest networking,
but do not confirm that the ingress controller has discovered the VM.

After the VM reports its IP, keep the port-forward process running and test
the ingress hostname from a client on the required network. If TLS still
returns an `unrecognized name` alert, the request is reaching the shared TLS
endpoint but its SNI hostname is not configured there; that needs ingress
configuration checked independently of the Helm release.
