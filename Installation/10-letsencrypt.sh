#!/bin/bash
set -euxo pipefail

apt install -y certbot python3-certbot-nginx 

certbot --nginx -n -d "$DOMAIN_NAME_GOES_HERE" --agree-tos --email "$LETSENCRYPT_EMAIL" --redirect

# Certbot needs *some* initial configuration, which is where the "phase 1" comes in
# It can't yet have listening on ssl because of missing certificates,
# but it fucks up the config, by adding a line that listens on 443, which is *not* necessarily
# what we want,
# which is why we unfuck it by replacing it with the "phase 2" configuration.

cp nginx.phase2.conf.template nginx.conf

sed -i "s/DOMAIN_NAME_GOES_HERE/$DOMAIN_NAME_GOES_HERE/" nginx.conf
sed -i "s/EXPOSED_PORT_GOES_HERE/$EXPOSED_PORT_GOES_HERE/" nginx.conf
sed -i "s/APPLICATION_PORT_GOES_HERE/$APPLICATION_PORT_GOES_HERE/" nginx.conf

cp nginx.conf /etc/nginx/sites-available/didacticalenigma-mem.conf

service nginx reload