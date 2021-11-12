#!/bin/bash
set -euxo pipefail

apt install -y unzip wget

wget https://github.com/DidacticalEnigma/DidacticalEnigma-Data/archive/refs/heads/master.zip
unzip master.zip

mkdir -p /var/www/didacticalenigma

mv DidacticalEnigma-Data-master /var/www/didacticalenigma/dataFiles

rm master.zip

chmod u+rX,go=rX -R /var/www/didacticalenigma
chown didacticalenigma:www-data -R /var/www/didacticalenigma