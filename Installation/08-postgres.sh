#!/bin/bash
set -euxo pipefail

apt install -y postgresql dos2unix

cp initial-db-create.sql.template initial-db-create.sql

sed -i "s/POSTGRES_PASSWORD_GOES_HERE/$POSTGRES_PASSWORD_GOES_HERE/" initial-db-create.sql

sudo -u postgres psql -f initial-db-create.sql

rm initial-db-create.sql

dos2unix initial-migration.sql # unfucks BOM, if it exists

sudo -u postgres PGPASSWORD="$POSTGRES_PASSWORD_GOES_HERE" psql --username=didacticalenigma -h localhost -p 5432 -d didacticalenigma -f initial-migration.sql