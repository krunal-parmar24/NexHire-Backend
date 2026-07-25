# Non-Functional Requirements (Backend)

**Derived from:** NFR Guide §1 (Context & Intent), §2 (Performance Targets — API/operation rows), §3 (Scalability & Capacity), §4 (Availability & Reliability), §5 (Observability), §6 (Security Posture), §7 (Data Retention)

## 1. Context

This project is a **portfolio-grade demonstration**, not an enterprise production system — solo developer/agent, 14-day timeline, no dedicated QA team. NFR targets are calibrated for a convincing live recruiter/interviewer demo, not high-scale production traffic.

## 2. Performance Targets (API/Operation Level)

| Operation | Target Response Time | Notes |
|---|---|---|
| `GET /api/jobs` | < 1.5s | Guest browsing/SEO is a P0 feature |
| `GET /api/jobs/{id}` | < 1s | |
| Login / Register | < 1s | Excludes onboarding wizard steps |
| `GET /api/jobs/{id}/match-score` | < 2s | Computed on-demand |
| Resume parse (upload → auto-filled fields) | < 8s | Bounded by GitHub Models GPT-4.1-mini latency + 12k-char extraction cap |
| Agent chat first-token latency (SignalR stream start) | < 3s | Streaming should begin before full response completes |
| `POST /api/applications` | < 1s | Excludes any AI autofill step, which is a separate async flow |

> Actual latency for AI-dependent operations is bounded by GitHub Models' free-tier response time and the 10 req/min Redis token-bucket limit; targets above assume the platform is within its rate-limit budget.

## 3. Scalability & Capacity

- **Concurrent users:** Designed for demo-scale traffic (a handful of concurrent recruiter/interviewer reviewers), not production load. No horizontal-scaling requirement is in scope.
- **Platform AI request cap:** Hard-fixed at 150 requests/day globally and 10 requests/minute via Redis token bucket — this is the binding capacity constraint for all AI features, not application server capacity.
- **Per-user AI credit cap:** 500 credits per rolling 30-day window — bounds per-user AI usage independent of platform capacity.
- **Upload capacity:** Resume/logo uploads capped at 1MB each via Supabase Storage — no bulk-upload or high-throughput storage requirement exists.
- **Database:** Supabase PostgreSQL free/starter tier is assumed sufficient; no read-replica, sharding, or connection-pooling-at-scale requirement is in scope.

## 4. Availability & Reliability

- **Target uptime:** Best-effort; no formal SLA. Render's free/starter web service tier may cold-start after inactivity — accepted trade-off, not a defect.
- **Graceful degradation:** When the AI platform rate limit or a user's credit balance is exhausted, manual (non-AI) job search and application flows must remain 100% operational — hard requirement, not best-effort.
- **AI failure isolation:** A failed or timed-out AI call must never crash the surrounding request; it must return a clear error response.
- **Idempotency:** Duplicate-application prevention (unique `job_id`+`user_id` constraint) ensures the application-submission flow is safe against double-submit/retry.

## 5. Observability

- **Structured logging:** All logs via Serilog, structured (not plain string interpolation), including correlation/request IDs on every request.
- **AI/Agent observability:** Every tool invocation is logged via `ToolCallLogs` (input/output/status/latency) — this table doubles as the primary AI observability mechanism; no separate APM tool is required for this scope.
- **Minimum log events:**
  - Every failed AI tool call (with reason: LLM error, timeout, rate-limit hit, credit exhausted)
  - Every credit deduction and its triggering action
  - Every authentication failure (without logging the attempted password)
  - Every unhandled exception via the global exception middleware
- **No dedicated dashboard/alerting tool** is in scope — Render's built-in log viewer and Supabase's dashboard are sufficient for this project's scale.

## 6. Security Posture

- LLM API keys and JWT signing secrets are server-side only, never exposed to the React frontend or committed to source control — enforced via `.env`/Render environment variables.
- Passwords are hashed with BCrypt before storage — plaintext passwords are never logged or persisted.
- JWT access tokens are short-lived; refresh tokens rotate on use.
- Role permanence (Job Seeker vs. Recruiter) is enforced server-side on every protected endpoint, not just hidden in the UI.
- Recruiter data scoping (a recruiter sees only their own jobs/applicants) is enforced at the repository/query level, not just filtered client-side.
- No OAuth SSO, SMTP/password-reset, or GDPR export/delete flows are in scope — these remain explicitly out of scope and must not be silently introduced during implementation.
- CORS must be explicitly configured to allow only the deployed frontend origin (plus `http://localhost:5173` for local dev) — never a wildcard `*` origin in production, since credentials/JWTs are involved.

## 7. Data Retention

- No automatic data purging/retention policy is required for this portfolio scope; demo data persists indefinitely unless manually reset.
- `ChatMessages`, `ChatEmbeddings`, `AgentMemories`, and `SessionSummaries` accumulate per user with no expiry job in scope — acceptable given the credit/rate-limit caps already bound total volume.
- `ToolCallLogs` may grow indefinitely for the duration of the portfolio demo period; no log-rotation policy is required in scope.

## Implementation Checklist

- [ ] Confirm ATS scoring and job-listing endpoints meet stated response-time targets under normal (non-rate-limited) conditions
- [ ] Verify manual job search/application flows remain fully functional when AI credit balance or platform rate limit is exhausted
- [ ] Configure Serilog structured logging with correlation IDs on all requests
- [ ] Confirm every AI tool call writes a `ToolCallLogs` entry with latency and status
- [ ] Verify no unhandled exception can crash the API — global exception middleware always returns a graceful error response
- [ ] Confirm role-permanence and recruiter-scoping checks exist server-side, not only in the frontend
- [ ] Confirm no secrets appear in the frontend bundle, logs, or source control
- [ ] Confirm BCrypt password hashing is applied before any password persistence
- [ ] Confirm CORS is scoped to specific frontend origins only, never a wildcard, given JWT/credentialed requests
