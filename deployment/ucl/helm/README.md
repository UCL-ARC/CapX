# CapX Helm Chart

This chart is installed by [`../minikube-up.sh`](../minikube-up.sh) as part of
the Condenser VM setup. Follow the [VM guide](../README.md) to create and
prepare the VM. The script builds the CapX image, starts Minikube, enables
Traefik, and installs the chart with an Ingress rule. Condenser terminates
HTTPS and forwards HTTP to the VM on port 80.

## Trial Limitations

- The script builds the `Local` configuration, which uses development
  sign-in and does not provide Entra or CAS authentication. Since this mode
  lets a visitor select any seeded account, the chart protects the Ingress
  with HTTP Basic Authentication. The script generates a random password
  and saves it in `~/.config/capx/ingress-credentials`. Enter those
  credentials at the browser prompt before the CapX page loads. Keep the
  file private and share the credentials only with trial users. This is a
  temporary access gate, not a substitute for configuring the app's normal
  sign-in before wider use.
- CapX uses SQLite inside the application pod. Replacing the pod or deleting
  the Minikube profile can discard the database; this is disposable trial
  data, not durable storage.
- The chart runs one replica because CapX applies database migrations during
  startup.
- Data-protection keys are stored in a persistent volume claim. Helm keeps
  that claim if the release is uninstalled; preserve it across reinstalls if
  the keys need to persist.
