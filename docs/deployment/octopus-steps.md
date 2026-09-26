# Octopus — Employee-Management-Microservice

The Octopus configuration for the org-domain microservice, recorded here so it
is reviewable in git rather than living only in a web console.

Almost everything is **shared with the identity service**. Only three things are
specific to this service: the image, the container name, and the public URL.

## Reused as-is — do not recreate

| Thing | Value |
|---|---|
| Environment | `Production` |
| SSH account | `emt-deploy` |
| Deployment target | `emt-prod-01` — `159.89.246.38` |
| Lifecycle | `EMT Release Path` — single manual phase |
| Docker feed | `ghcr` |
| Library variable set | `EMT Shared Configs` |

## What to add

**1. A target tag.** Open `emt-prod-01` → Target Tags → add **`emt-microservice`**
alongside the existing `emt-identity`. One machine, two tags — so if this service
later moves to its own droplet, the tag moves and no step changes.

**2. A project** `Employee-Management-Microservice`, lifecycle `EMT Release Path`,
with `EMT Shared Configs` attached and two project variables:

| Name | Value |
|---|---|
| `EMT.Container.Name` | `emt-api` |
| `EMT.PublicUrl` | `https://api.employee-management-tool.com` |

The JWT values come from the shared set and **must be identical to the identity
service's** — this service validates tokens that service mints. A mismatch means
login succeeds and every call here returns 401 with nothing in the logs saying why.

**3. Two script steps**, both **Run a Script**, language **Bash** (the editor
defaults to PowerShell), Execution Location *Run on each deployment target*,
Target Tags `emt-microservice`.

### Step 1 — Deploy microservice with Helm

Timeout 10 minutes. One package reference:

| Field | Value |
|---|---|
| Package feed | `ghcr` |
| Package ID | `yawdev/emt-microservice-api` |
| Name | `emt-microservice-api` |
| Package Acquisition | **The package won't be downloaded** |

> `Octopus.Action.Package[...]` keys on the reference **Name**, not the Package
> ID. A mismatch resolves empty, and the script stops with "Package version is empty".

Same script as the identity project apart from `REPO` and the package reference
name. The reasoning behind each part is in the identity repo's `octopus-steps.md`.
Releases before 1.0.6 have no `helm/` folder at their tag and can't be redeployed.

```bash
set -euo pipefail
# kubectl/helm need this: without it they read the root-only k3s config and fail.
export KUBECONFIG=$HOME/.kube/config

RELEASE="$(get_octopusvariable 'EMT.Container.Name')"
REPO=YawDev/employee-management-microservice
VERSION="$(get_octopusvariable 'Octopus.Action.Package[emt-microservice-api].PackageVersion')"
[ -n "$VERSION" ] || { echo "Package version is empty — check the package reference Name"; exit 1; }

umask 077; WORK="$(mktemp -d)"; trap 'rm -rf "$WORK"' EXIT

# Chart at the release's git tag, so the chart that ships matches the code it was
# built with — and redeploying an old release redeploys its chart too.
curl -fsSL "https://github.com/$REPO/archive/refs/tags/$VERSION.tar.gz" \
  | tar -xz -C "$WORK" --strip-components=1 --wildcards '*/helm/*'
CHART="$WORK/helm"

# Fill each "#{Variable}" in values.prod.yaml from Octopus. Octopus's own file
# substitution is documented for package steps only, so the script does it. Values
# are JSON-quoted, so any character stays valid YAML. A missing variable fails here.
for ph in $(grep -o '"#{[^}]*}"' "$CHART/values.prod.yaml" | sort -u); do
  name="${ph#\"\#\{}"; name="${name%\}\"}"
  value="$(get_octopusvariable "$name")"
  [ -n "$value" ] || { echo "Octopus variable '$name' is empty"; exit 1; }
  PH="$ph" VAL="$value" python3 -c 'import json,os,sys; p=sys.argv[1]; s=open(p).read(); open(p,"w").write(s.replace(os.environ["PH"], json.dumps(os.environ["VAL"])))' "$CHART/values.prod.yaml"
done

# ghcr packages are private; refresh the cluster's pull credentials every deploy so
# a rotated PAT takes effect. Built in a file, so the token never appears in `ps`.
AUTH="$(printf '%s:%s' "$(get_octopusvariable 'EMT.Ghcr.User')" "$(get_octopusvariable 'EMT.Ghcr.Token')" | base64 -w0)"
printf '{"auths":{"ghcr.io":{"auth":"%s"}}}' "$AUTH" > "$WORK/docker.json"
kubectl -n emt create secret generic ghcr-pull --type=kubernetes.io/dockerconfigjson \
  --from-file=.dockerconfigjson="$WORK/docker.json" --dry-run=client -o yaml | kubectl apply -f -

# --wait: block until the new pod passes readiness. --rollback-on-failure (Helm 4's
# name for --atomic): if it doesn't within 4 minutes, go back to the previous release.
helm upgrade --install "$RELEASE" "$CHART" -n emt \
  -f "$CHART/values.prod.yaml" --set image.tag="$VERSION" \
  --wait --timeout 4m --rollback-on-failure
```

### Step 2 — Health check

Timeout 5 minutes, no package reference.

```bash
export KUBECONFIG=$HOME/.kube/config
URL="$(get_octopusvariable 'EMT.PublicUrl')/health"
NAME="$(get_octopusvariable 'EMT.Container.Name')"

for i in $(seq 1 30); do
  if curl -fsS "$URL" | grep -q Healthy; then
    echo "Healthy after $((i * 2))s"
    exit 0
  fi
  sleep 2
done

echo "Health check failed after 60s"
kubectl -n emt logs "deployment/$NAME" --tail 50
exit 1
```

## GitHub secrets

Same three as the identity repo, set on **this** repository:
`OCTOPUS_API_KEY`, `OCTOPUS_URL` (with `https://`, no trailing slash),
`OCTOPUS_SPACE`. No registry credential — ghcr uses the per-run `GITHUB_TOKEN`.

Also create a **`Production`** GitHub environment (Settings → Environments) with
yourself as a required reviewer. That approval is the deploy gate, and it is what
makes the environment status appear on the repo page.

## Notes

- **Editing a step does not change an existing release.** Octopus snapshots the
  process and variables into the release; create a new one after any change.
- The git tag, image tag and Octopus release number are the same string.
- Version starts at `1.0.<run_number>` — unlike the identity project, no releases
  exist here yet, so there is nothing to collide with.
