#!/bin/bash
set -euxo pipefail

apt install -y nginx

cp nginx.phase1.conf.template nginx.conf

sed -i "s/DOMAIN_NAME_GOES_HERE/$DOMAIN_NAME_GOES_HERE/" nginx.conf
sed -i "s/EXPOSED_PORT_GOES_HERE/$EXPOSED_PORT_GOES_HERE/" nginx.conf
sed -i "s/APPLICATION_PORT_GOES_HERE/$APPLICATION_PORT_GOES_HERE/" nginx.conf

cp nginx.conf /etc/nginx/sites-available/didacticalenigma-mem.conf
ln -s -f /etc/nginx/sites-available/didacticalenigma-mem.conf /etc/nginx/sites-enabled/didacticalenigma-mem.conf