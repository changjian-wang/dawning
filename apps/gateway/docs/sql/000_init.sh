#!/bin/bash
# =============================================
# MySQL Database Initialization Script
# =============================================
# This script is automatically executed by MySQL container
# during first-time initialization. It executes all SQL files
# in the schema/ and seed/ directories in alphabetical order.
#
# Directory structure:
#   /docker-entrypoint-initdb.d/
#     ├── 000_init.sh          (this script - runs first)
#     ├── schema/              (table definitions)
#     │   ├── 001_users.sql
#     │   ├── 002_xxx.sql
#     │   └── ...
#     └── seed/                (initial data)
#         └── 001_seed_xxx.sql
# =============================================

set -e

SCRIPT_DIR="$(dirname "$0")"

echo "=========================================="
echo "  Dawning Database Initialization"
echo "=========================================="

# Execute schema files in order
echo ""
echo ">>> Executing Schema Files..."
for sql_file in "$SCRIPT_DIR"/schema/*.sql; do
    if [ -f "$sql_file" ]; then
        echo "  - $(basename "$sql_file")"
        mysql -u root -p"$MYSQL_ROOT_PASSWORD" "$MYSQL_DATABASE" < "$sql_file"
    fi
done

# Execute seed files in order
echo ""
echo ">>> Executing Seed Files..."
for sql_file in "$SCRIPT_DIR"/seed/*.sql; do
    if [ -f "$sql_file" ]; then
        echo "  - $(basename "$sql_file")"
        mysql -u root -p"$MYSQL_ROOT_PASSWORD" "$MYSQL_DATABASE" < "$sql_file"
    fi
done

echo ""
echo "=========================================="
echo "  Database Initialization Complete!"
echo "=========================================="
