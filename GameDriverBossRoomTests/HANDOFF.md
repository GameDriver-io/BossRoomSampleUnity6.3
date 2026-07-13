# QaaS + BossRoom demo — handoff

_Last updated 2026-07-12. Safe to commit (no secrets herein). Deeper history lives in
`~/io.gamedriver.qaas/SPEC.md` and the Claude auto-memory under `~/.claude/projects/-Users-rob/memory/`._

## TL;DR — where things stand

The **priority-1 demo path is live and proven end-to-end**: run the BossRoom GameDriver suite → the
reporter uploads results → they appear in the **staging** QaaS dashboard (`qaas-staging.gamedriver.ai`),
with correct per-domain counts. The **Coverage X-Ray** report also works (local HTML overlay).

- **Staging**: fully working for the demo. Nothing blocking.
- **Prod** (`qaas.gamedriver.ai`): infrastructure provisioned but **scaled down / not deployed** — it's the
  #1 follow-up (see "Next steps"). Deliberately idle to avoid burning sponsorship credits until launch.
- **Standalone ingest subsystem** (`services/ingest`): **deferred** — we use the portal's in-app ingest
  route instead (see "Ingest: why in-app, not the subsystem").

---

## 1. How to run the demo (the important part)

**Prereqs:** Boss Room open in the Unity Editor (GameDriver agent listening on `localhost:19734`), and Rider.

1. **Open Boss Room in the Unity Editor.** The suite drives it via `Connect(autoplay:true)`, but the Editor
   itself must be running. If `lsof -i :19734` shows nothing, it's not up.
2. **The reporter config is a gitignored file** in this directory:
   - `.env.qaas-staging` — holds `QAAS_API_KEY` (the staging ingest key) + endpoint/config. **Never
     committed** (gitignored). If it's missing, recreate it — see "Reporter config" below.
   - `qaas-staging.runsettings` — the same values as a `.runsettings` (also gitignored). **This is what
     Rider uses.**
3. **Point Rider at the runsettings (one-time):** Settings → Build, Execution, Deployment → Unit Testing →
   Test Runner → "Use specific .runsettings/.testsettings file" → select `qaas-staging.runsettings`.
4. **Run fixtures** from the Rider gutter/Explorer as usual. Start with `AudioTests` (fast, 3 tests, no
   fragile game state) to sanity-check the upload, then run more.
5. **Watch it land** in the dashboard at `https://qaas-staging.gamedriver.ai` (sign in via Cloudflare Access
   with an `@gamedriver.io` account).

**Coverage X-Ray** is `XRayCoverageTests.cs` — it's **local**, needs no cloud: run it and open the
`xray-*.html` overlay it writes next to the test run (`bin/Debug/net8.0/`).

### ⚠️ The #1 footgun: the reporter reads ENV VARS, not the file

A `.env` file on disk does **nothing** by itself. The reporter (`gdio.qaas.reporter`) reads
`QAAS_API_KEY` etc. from the **process environment**. If those aren't set, it logs
`[QaaS] QAAS_API_KEY not set; skipping results upload.` to stderr and the run **passes having uploaded
nothing** — no error, no fallback file. This bit us: running the suite from Rider *without* the
`.runsettings` selected silently skipped every upload. So:

- **Rider:** the `.runsettings` must be selected (step 3). That injects the env vars into the test host.
- **CLI:** `dotnet test --settings qaas-staging.runsettings --filter "FullyQualifiedName~AudioTests"`.
- If you see a `qaas-fallback/` dir appear, the upload was *attempted and failed* (different problem — check
  the endpoint/key). No such dir + "skipping" in stderr = the key wasn't in the env.

### Reporter config values (to recreate `.env.qaas-staging` if lost)

```
QAAS_API_KEY=<staging key — regenerate in the QaaS portal: project → Settings → Keys>
QAAS_INGEST_URL=https://qaas-staging.gamedriver.ai/api/ingest/v1/runs
QAAS_SHARE_DETAIL=true          # readable test names in the dashboard (else HMAC-hashed)
QAAS_HASH_SECRET=bossroom-staging-demo
QAAS_EXECUTION_TYPE=automated
# QAAS_PROJECT=<slug>           # ONLY if the key is client-wide; project-scoped keys resolve automatically
```
The current staging key is **project-scoped** (generated from the project's own Settings→Keys), so
`QAAS_PROJECT` is not needed. The key is **revocable + staging-only** — low sensitivity, but keep it out of
git. After editing `.env.qaas-staging`, regenerate the runsettings:
```
{ echo '<?xml version="1.0" encoding="utf-8"?>'; echo '<RunSettings><RunConfiguration><EnvironmentVariables>';
  grep -E '^[A-Z][A-Z0-9_]*=' .env.qaas-staging | while IFS='=' read -r k v; do echo "  <$k>$v</$k>"; done;
  echo '</EnvironmentVariables></RunConfiguration></RunSettings>'; } > qaas-staging.runsettings
```

---

## 2. System map — repos & what they are

All under `GameDriver-io` (private) unless noted.

| Repo / path | What | State |
|---|---|---|
| `BossRoomSampleUnity6.3` @ **`qaas-enhanced`** | This Unity project + the `GameDriverBossRoomTests` suite (16 domains) + X-ray integration. `qaas-enhanced` is the **long-lived working branch** (Rob: not merging back to main, not forking yet). | Pushed. Working checkout: `~/Documents/GitHub/BossRoomSampleUnity6.3`. |
| `gdio.qaas.reporter` | NUnit→QaaS ingest uploader (netstandard2.0). Assembly-level `ITestAction` capture + `Reporter.UploadCollected()`. HMAC privacy default. Referenced by the suite's `.csproj`. | Own repo, pushed. Lives at `~/Documents/GitHub/gdio.qaas.reporter`. |
| `gdio.qaas.coverage` | Coverage X-Ray (net8.0). `RecordingApiClient` wrapper + analyzer + HTML overlay. Consumes frozen GameDriver DLLs from `libs/`. Referenced by the suite. | Own repo, pushed. Live-verified. `~/Documents/GitHub/gdio.qaas.coverage`. |
| `io.gamedriver.qaas` | The QaaS portal (Next.js) + the **in-app ingest route** (`app/api/ingest/v1/{runs,builds}`) + `services/ingest` (deferred subsystem). | `~/io.gamedriver.qaas`. `main` has PRs #16 + #17 merged (below). |
| `app-gamedriver-portal` (licensing Portal, separate .NET app) | The license create/download portal. Near-idle. Its own plan. | Untouched by this work — leave it alone. |

**Second BossRoom checkout:** `~/BossRoomSampleUnity6.3` is on the old **`session3`** branch (the earlier
tutorial-lab work — Session1/2/3 labs, smoke/multiplayer tests). It's archived on `origin/session3`; its
working tree was stashed clean (`git stash list` → the cleanup stash). Rob works on `qaas-enhanced` going
forward; session3 stays archived (not merged — the labs are superseded by the domain suite).

**Recently merged to `io.gamedriver.qaas` main (both CI-green, deployed to staging):**
- **PR #16** — `fix(ingest): persist passed/failed/flaky/skipped to GoldDomainKpi`. Real bug: the rollup
  computed per-domain counts but never wrote them, so the dashboard's "Failed"/"Pass Rate" always read 0.
- **PR #17** — `feat(ingest): serve the in-app ingest route on staging`. Allows `QAAS_ENV=staging` (not just
  `local`) to serve `/api/ingest/v1/*`; prod still 410s.

---

## 3. Deployment / infra state (Azure sub `ac761247` "Microsoft Azure Sponsorship", RG `rg-gamedriver-portal`)

### Staging — LIVE
- App: `app-gamedriver-qaas-staging` on plan **`asp-gamedriver-portal` (B2)** — **shared** with the licensing
  Portal (consolidated; see Architecture). `QAAS_ENV=staging`, `DEV_LOGIN_ENABLED=true`.
- Behind **Cloudflare Access** (`@gamedriver.io` OTP) for humans — **except** `/api/ingest/*`, which has a
  dedicated **Access Bypass** application so machine (reporter) uploads reach the origin. The route's own
  Bearer key + the Cloudflare-only origin IP-lock are the gate there.
- DB: `GameDriverQaaS-staging` (serverless, auto-pause) on `sqlgdp8b187e`.
- Auto-deploys from `main` via `.github/workflows/deploy-staging.yml`.

### Prod — PROVISIONED, SCALED DOWN, NOT DEPLOYED
- App: `app-gamedriver-qaas-prod` on plan **`asp-gamedriver-qaas-prod` (P0V3)** — **scaled down from P1V3**
  to save credits while idle; **no code deployed yet**.
- DB: `GameDriverQaaS` (**scaled down to serverless + auto-pause**; was provisioned/no-pause — scale back up
  at launch). Contained user `qaas_app_prod`; conn string in Key Vault secret `QaasProdDbUrl`.
- App Insights `appi-gamedriver-qaas-prod` wired. Settings incl. `QAAS_ENV=production`,
  **`DEV_LOGIN_ENABLED=false`** (Rob-mandated off in prod).
- Origin IP-locked to Cloudflare. Deployer SP granted Website Contributor → **`deploy-prod.yml` is no longer
  inert**. Entra redirect URIs confirmed correct.
- **The `staging` deployment slot was deleted** during the cost scale-down — **recreate it at launch**
  (deploy-prod.yml uses slot-swap).

### DNS (Cloudflare) / TLS
- Live + proxied: `qaas.gamedriver.ai` (prod portal), `qaas-staging.gamedriver.ai` (staging). Both bound +
  Verified in Azure. TLS via the **wildcard Cloudflare Origin CA cert `*.gamedriver.ai`** (thumbprint
  `F4486CEC66ABBF9441627422FC760E91D011AC42`) in the RG cert store, expires 2041.
- **Dangling** (point at deleted apps): `ingest.gamedriver.ai`, `staging-ingest.gamedriver.ai` (+ their
  `asuid` TXT). Repoint or remove when/if the standalone ingest subsystem ships.

---

## 4. Architecture decisions (the "why")

- **This is a 3-tier app, not microservices.** One team, one shared Azure SQL, coupled releases. Decision:
  **consolidate onto few boxes, split by environment, keep one deliberate seam (ingest) as a future option.**
- **Box layout:** **one Basic (B2) plan for the low-traffic tier** (licensing Portal + all staging apps) and
  **Premium plans for prod**, all prod apps co-hosted until capacity forces a split. Quota reality forced
  this: this subscription **can't allocate new Basic-tier workers** (scaling into Basic → quota 0), but
  Premium allocates freely. Consumption (serverless) is unavailable entirely (see gotchas).
- **Ingest: why in-app, not the subsystem.** The ingest *logic* (`lib/ingest/*`) is shared. The standalone
  `services/ingest` subsystem is just a scale-hardened *transport* (durable buffer + async worker) around it.
  Its benefits (load isolation of a CI write-firehose from the interactive dashboard, capture-first
  durability) only matter at multi-tenant volume we don't have yet. So: **deploy consolidated (in-app route)
  now, peel ingest onto its own box when real CI volume justifies it.** The seam is already drawn.

---

## 5. Next steps (roughly prioritized)

1. **Prod push** (`qaas.gamedriver.ai`) — the main follow-up:
   - Scale `asp-gamedriver-qaas-prod` P0V3 → **P1V3**; DB `GameDriverQaaS` serverless → **provisioned/no-pause**.
   - **Recreate the `staging` deployment slot** on the prod app.
   - Run **Promote → Production** (`deploy-prod.yml`, manual `workflow_dispatch`) with a validated `main` SHA.
   - Note: prod's in-app ingest route still 410s by design; decide if prod ingest is in-app (flip the gate to
     allow `production`) or the standalone subsystem before customers upload to prod.
2. **Verify domain grouping in the dashboard** — the reporter's `Capture.cs` `FirstCategory()` had a bug
   where a fixture-level `[Category]` wasn't picked up, so runs could land under **"Uncategorized"** instead
   of the real domain. A background task (`task_32b9e57f`) was spawned for it; **status uncertain** — the
   reporter repo showed no uncommitted changes at last check. **Verify the demo's domains are correct; if
   they're "Uncategorized", fix `FirstCategory()`.**
3. **Standalone ingest subsystem** (`services/ingest`) — deferred. If pursued: deploy via **GitHub Actions**
   (cloud runner — sidesteps the local-Azure-start problems) or open an Azure support ticket for the
   Consumption/VNet issues (below). Then repoint the dangling `ingest.*` / `staging-ingest.*` DNS.
4. **Run the rest of the BossRoom suite against staging** and curate the demo dataset (the suite is
   deliberately a mix of pass/fail/flaky/blocked across domains — that's intentional, see the demo memory).

---

## 6. Gotchas / hard-won lessons (don't re-learn these)

- **Reporter silently skips upload when env vars aren't set** — see §1. The `.runsettings` (Rider) or
  `--settings` (CLI) is mandatory; the `.env` file alone does nothing.
- **Cloudflare Access blocks machine uploads** to any `*.gamedriver.ai` host by default. The fix is a
  **separate, path-scoped Access application** for `/api/ingest/*` with a **Bypass/Everyone** policy (a
  policy on the main app would unlock the whole site). Already done for staging; needed again for prod ingest.
- **Consumption Function Apps do not work on this Azure account.** Linux Consumption never allocates a worker
  (perpetual 503, both subs, both regions); Windows Consumption allocates but the v4 Node model won't index
  (404, 0 functions); a B1 dedicated co-host hit a platform `VNETFailure`. Root cause looks account/region
  environmental. **Use dedicated App Service plans** (what the portal runs on) or GitHub-Actions-driven
  deploys.
- **Staging/prod DB access:** the DB is firewall-gated on `sqlgdp8b187e`. Add a temp firewall rule for your
  IP, do the work, **remove it**. The connection string in `~/io.gamedriver.qaas/.env.staging.local`
  (`STAGING_DATABASE_URL`) is **unquoted and full of `;`** — do **not** `source` it in bash (it truncates at
  the first `;`); extract the literal value (`grep … | sed 's/^STAGING_DATABASE_URL=//'`).
- **Unity + git-LFS noise:** opening the project reserializes scenes/`ProjectSettings`/materials, and without
  git-lfs installed a diff shows huge fake changes (LFS pointer vs content). Most "changes" in the BossRoom
  repo are this noise — check with `git-lfs` installed before believing a diff. The `qaas-enhanced` branch
  intentionally keeps only test-project changes + two confirmed game-config exceptions (Unity Cloud project
  linking + Network Simulator config).
- **Concurrent Claude/agent sessions** were editing shared repos this session — be careful committing in
  `io.gamedriver.qaas` (verify whose work is in the tree before `git add`).
- **The prod DB/plan are idle-scaled-down on purpose.** If prod seems "broken," it's because it's not
  deployed + scaled to minimum. That's intentional, not a bug.

---

## 7. Reference

- QaaS spec: `~/io.gamedriver.qaas/SPEC.md`. CI/CD runbook: `~/io.gamedriver.qaas/docs/CICD.md`.
- Deeper context (decisions, gotchas) in Claude auto-memory: `project_bossroom_qaas_demo`,
  `project_qaas_reporter`, `project_qaas_coverage_xray`, `project_qaas_portal` under
  `~/.claude/projects/-Users-rob/memory/`.
- GameDriver↔BossRoom test-authoring gotchas (right-click attack, no `NavAgentMoveToPoint`, custom-enum
  serialization, session rate-limit, HPath cache flush, etc.) are documented in `project_bossroom_qaas_demo`.
