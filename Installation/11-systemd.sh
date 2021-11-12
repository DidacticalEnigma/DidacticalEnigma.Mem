#!/bin/bash
set -euxo pipefail

cp systemd.service.template systemd.service
sed -i "s/APPLICATION_PORT_GOES_HERE/$APPLICATION_PORT_GOES_HERE/" systemd.service

cp systemd.service /etc/systemd/system/didacticalenigma-mem.service

systemctl start didacticalenigma-mem.service