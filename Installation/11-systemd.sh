#!/bin/bash
set -euxo pipefail

sed
	-e "s/APPLICATION_PORT_GOES_HERE/$APPLICATION_PORT_GOES_HERE/" \
	systemd.service.template \
	> /etc/systemd/system/didacticalenigma-mem.service

systemctl daemon-reload

systemctl enable --now didacticalenigma-mem
