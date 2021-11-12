#!/bin/bash
set -euxo pipefail

apt install -y nginx

sed \
	-e "s/DOMAIN_NAME_GOES_HERE/$DOMAIN_NAME_GOES_HERE/" \
	-e "s/EXPOSED_PORT_GOES_HERE/$EXPOSED_PORT_GOES_HERE/" \
	-e "s/APPLICATION_PORT_GOES_HERE/$APPLICATION_PORT_GOES_HERE/" nginx.phase1.conf.template \
	> /etc/nginx/sites-available/didacticalenigma-mem.conf

ln -s -f /etc/nginx/sites-available/didacticalenigma-mem.conf /etc/nginx/sites-enabled/didacticalenigma-mem.conf