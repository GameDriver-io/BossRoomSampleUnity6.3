#!/usr/bin/env bash
# Run the BossRoom GameDriver suite against QaaS STAGING ingest.
#
# Loads the gitignored .env.qaas-staging (which holds the API key — never committed)
# into the environment, then runs the suite. The reporter's OneTimeTearDown uploads
# results to qaas-staging on the way out. Requires a Unity Editor running the Boss Room
# project with the GameDriver agent on localhost:19734.
#
# Extra args pass through to dotnet test, e.g.:
#   ./run-staging-demo.sh --filter "FullyQualifiedName~AudioTests"
set -euo pipefail
cd "$(dirname "$0")"

if [ ! -f .env.qaas-staging ]; then
  echo "missing .env.qaas-staging (gitignored) — create it with QAAS_API_KEY=..." >&2
  exit 1
fi
if grep -q "PASTE_STAGING_KEY_HERE" .env.qaas-staging; then
  echo "edit .env.qaas-staging: replace PASTE_STAGING_KEY_HERE with the real staging key" >&2
  exit 1
fi

set -a; source .env.qaas-staging; set +a
dotnet test GameDriverBossRoomTests/GameDriverBossRoomTests.csproj "$@"
