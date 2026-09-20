#!/usr/bin/env bash
#
# One-shot bootstrap for a fresh Ubuntu 24.04 DigitalOcean Droplet that will host
# the emt and emt-identity services behind Caddy. Postgres lives on Neon, so
# nothing database-related is installed here.
#
# Run as root on a brand-new Droplet:
#   scp deploy/bootstrap.sh root@<ip>:/root/
#   ssh root@<ip> 'bash /root/bootstrap.sh deploy'
#
# The argument is the non-root user to create. Re-running is safe.

set -euo pipefail

DEPLOY_USER="${1:-deploy}"
SWAP_SIZE="2G"

log() { printf '\n\033[1;32m==> %s\033[0m\n' "$1"; }
die() { printf '\n\033[1;31mERROR: %s\033[0m\n' "$1" >&2; exit 1; }

[[ $EUID -eq 0 ]] || die "run this as root"

# We disable password auth further down. If the Droplet was created without an
# SSH key we would lock ourselves out, so refuse to continue.
[[ -s /root/.ssh/authorized_keys ]] \
  || die "/root/.ssh/authorized_keys is empty — recreate the Droplet with an SSH key attached"

log "Updating base packages"
export DEBIAN_FRONTEND=noninteractive
apt-get update -qq
apt-get upgrade -y -qq
apt-get install -y -qq ca-certificates curl gnupg unattended-upgrades fail2ban

log "Creating $DEPLOY_USER"
if ! id -u "$DEPLOY_USER" >/dev/null 2>&1; then
  adduser --disabled-password --gecos "" "$DEPLOY_USER"
fi
usermod -aG sudo "$DEPLOY_USER"
install -d -m 700 -o "$DEPLOY_USER" -g "$DEPLOY_USER" "/home/$DEPLOY_USER/.ssh"
install -m 600 -o "$DEPLOY_USER" -g "$DEPLOY_USER" \
  /root/.ssh/authorized_keys "/home/$DEPLOY_USER/.ssh/authorized_keys"
echo "$DEPLOY_USER ALL=(ALL) NOPASSWD:ALL" > "/etc/sudoers.d/90-$DEPLOY_USER"
chmod 440 "/etc/sudoers.d/90-$DEPLOY_USER"

log "Adding ${SWAP_SIZE} swap"
# Headroom for `dotnet build`, which spikes well past what the services need at rest.
if ! swapon --show | grep -q /swapfile; then
  fallocate -l "$SWAP_SIZE" /swapfile
  chmod 600 /swapfile
  mkswap /swapfile >/dev/null
  swapon /swapfile
  echo '/swapfile none swap sw 0 0' >> /etc/fstab
fi
sysctl -qw vm.swappiness=10
echo 'vm.swappiness=10' > /etc/sysctl.d/99-swappiness.conf

log "Hardening SSH"
cat > /etc/ssh/sshd_config.d/99-hardening.conf <<'EOF'
PermitRootLogin no
PasswordAuthentication no
KbdInteractiveAuthentication no
ChallengeResponseAuthentication no
X11Forwarding no
MaxAuthTries 3
EOF
sshd -t || die "sshd config is invalid — not restarting, your session is safe"
systemctl restart ssh

log "Installing Docker"
if ! command -v docker >/dev/null 2>&1; then
  install -m 0755 -d /etc/apt/keyrings
  curl -fsSL https://download.docker.com/linux/ubuntu/gpg \
    -o /etc/apt/keyrings/docker.asc
  chmod a+r /etc/apt/keyrings/docker.asc
  echo "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.asc] \
https://download.docker.com/linux/ubuntu $(. /etc/os-release && echo "$VERSION_CODENAME") stable" \
    > /etc/apt/sources.list.d/docker.list
  apt-get update -qq
  apt-get install -y -qq docker-ce docker-ce-cli containerd.io \
    docker-buildx-plugin docker-compose-plugin
fi
usermod -aG docker "$DEPLOY_USER"

log "Capping container log growth"
# Without this a chatty ASP.NET container will happily fill a 50 GiB disk.
cat > /etc/docker/daemon.json <<'EOF'
{
  "log-driver": "json-file",
  "log-opts": { "max-size": "10m", "max-file": "3" }
}
EOF
systemctl restart docker
systemctl enable --now docker

log "Enabling unattended security upgrades"
dpkg-reconfigure -f noninteractive unattended-upgrades

install -d -o "$DEPLOY_USER" -g "$DEPLOY_USER" "/home/$DEPLOY_USER/emt"

log "Done"
cat <<EOF

  Reconnect as:  ssh $DEPLOY_USER@<droplet-ip>
  App directory: /home/$DEPLOY_USER/emt
  Docker:        $(docker --version)
  Swap:          $(free -h | awk '/Swap:/ {print $2}')

  Root SSH and password auth are now disabled. Verify the new login in a
  SECOND terminal before closing this one.

  Note: no ufw rules are set — the DigitalOcean Cloud Firewall is the single
  place port policy lives. Don't add a second layer that can drift.
EOF
