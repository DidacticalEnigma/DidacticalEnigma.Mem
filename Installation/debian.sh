#!/bin/bash
set -euxo pipefail

export DOMAIN_NAME_GOES_HERE=TO BE FILLED IN
export LETSENCRYPT_EMAIL=TO BE FILLED IN
export EXPOSED_PORT_GOES_HERE=TO BE FILLED IN
export OPENID_AUTHORITY_GOES_HERE=TO BE FILLED IN
export OPENID_AUDIENCE_GOES_HERE=TO BE FILLED IN
export APPLICATION_PORT_GOES_HERE=5050
export POSTGRES_PASSWORD_GOES_HERE=$(openssl rand -base64 32)

apt update

./01-user.sh
./03-net-core.sh
./04-data-files.sh
./06-deploy.sh
./08-postgres.sh
./09-nginx.sh
./10-letsencrypt.sh
./11-systemd.sh

# HARDENING - EXAMINE THE FILE BEFORE RUNNING IT
./99-hardening.sh