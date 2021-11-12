#!/bin/bash
set -euxo pipefail

mkdir -p /var/www/didacticalenigma-mem

cp -r DidacticalEnigma.Mem/* /var/www/didacticalenigma-mem

sed \
	-e "s/POSTGRES_PASSWORD_GOES_HERE/$POSTGRES_PASSWORD_GOES_HERE/" \
	-e "s#OPENID_AUTHORITY_GOES_HERE#$OPENID_AUTHORITY_GOES_HERE#" \
	-e "s#OPENID_AUDIENCE_GOES_HERE#$OPENID_AUDIENCE_GOES_HERE#" appsettings.json.template \
	> /var/www/didacticalenigma-mem/appsettings.json

chmod u+rX,go=X -R /var/www/didacticalenigma-mem/*
chown didacticalenigma:www-data -R /var/www/didacticalenigma-mem