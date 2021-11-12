#!/bin/bash
set -euxo pipefail

apt install -y unzip wget

wget https://github.com/DidacticalEnigma/DidacticalEnigma-Data/archive/refs/heads/master.zip
unzip master.zip

mkdir -p /var/www/didacticalenigma/dataFiles

cp -r DidacticalEnigma-Data-master/* /var/www/didacticalenigma/dataFiles

chmod u+rX,go+rX,go-w -R /var/www/didacticalenigma
chown didacticalenigma:www-data -R /var/www/didacticalenigma