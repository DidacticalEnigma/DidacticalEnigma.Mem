#!/bin/bash
set -euxo pipefail

# Firewall rules
apt install -y ufw fail2ban
ufw allow 22 # SSH
ufw allow 80 # HTTP
ufw allow 443 # HTTPS
ufw allow $EXPOSED_PORT_GOES_HERE
ufw enable