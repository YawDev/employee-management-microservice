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

### Step 1 — Deploy microservice container

Timeout 10 minutes. One package reference:

| Field | Value |
|---|---|
| Package feed | `ghcr` |
| Package ID | `yawdev/emt-microservice-api` |
| Name | `emt-microservice-api` |
| Package Acquisition | **The package won't be downloaded** |

> `Octopus.Action.Package[...]` keys on the reference **Name**, not the Package
> ID. A mismatch resolves empty and docker fails with `invalid reference format`
> — right after a successful login, so it reads like a credentials problem.

```bash
set -euo pipefail

VERSION="$(get_octopusvariable 'Octopus.Action.Package[emt-microservice-api].PackageVersion')"
NAME="$(get_octopusvariable 'EMT.Container.Name')"

if [ -z "$VERSION" ]; then
  echo "Package version is empty — check the package reference Name matches"
  exit 1
fi

IMAGE="ghcr.io/yawdev/emt-microservice-api:$VERSION"

# ghcr packages are private by default, so the droplet needs credentials to pull.
echo "$(get_octopusvariable 'EMT.Ghcr.Token')" \
  | docker login ghcr.io -u "$(get_octopusvariable 'EMT.Ghcr.User')" --password-stdin

docker pull "$IMAGE"
docker rm -f "$NAME" 2>/dev/null || true

# No published ports: Caddy reaches this container by name over the `emt`
# network. Publishing to the host would expose the API over plain HTTP.
docker run -d --name "$NAME" --restart unless-stopped \
  --network emt \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ConnectionStrings__DefaultConnection="$(get_octopusvariable 'EMT.Db.ConnectionString')" \
  -e Jwt__Key="$(get_octopusvariable 'EMT.Jwt.Key')" \
  -e Jwt__Issuer="$(get_octopusvariable 'EMT.Jwt.Issuer')" \
  -e Jwt__Audience="$(get_octopusvariable 'EMT.Jwt.Audience')" \
  -e CorsOriginSettings__DomainList__0="$(get_octopusvariable 'EMT.Cors.Origin.App')" \
  -e CorsOriginSettings__DomainList__1="$(get_octopusvariable 'EMT.Cors.Origin.Sys')" \
  "$IMAGE"

docker image prune -af --filter "until=168h"
```

### Step 2 — Health check

Timeout 5 minutes, no package reference.

```bash
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
docker logs --tail 50 "$NAME"
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
