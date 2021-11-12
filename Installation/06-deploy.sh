#!/bin/bash
set -euxo pipefail

mkdir -p /var/www/didacticalenigma-mem

cp -r DidacticalEnigma.Mem/* /var/www/didacticalenigma-mem

cp appsettings.json.template appsettings.json

sed -i "s/POSTGRES_PASSWORD_GOES_HERE/$POSTGRES_PASSWORD_GOES_HERE/" appsettings.json
sed -i "s#OPENID_AUTHORITY_GOES_HERE#$OPENID_AUTHORITY_GOES_HERE#" appsettings.json
sed -i "s#OPENID_AUDIENCE_GOES_HERE#$OPENID_AUDIENCE_GOES_HERE#" appsettings.json

cp appsettings.json /var/www/didacticalenigma-mem

chmod u+rX,go+rX,go-rw -R /var/www/didacticalenigma-mem/*
chown didacticalenigma:www-data -R /var/www/didacticalenigma-mem