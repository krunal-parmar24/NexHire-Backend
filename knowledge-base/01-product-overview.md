# Product Overview (Backend)

**Derived from:** SRS v3.1 (§1), Implementation Plan v1.0 (§1)

> This file orients a Backend-only AI coding agent on what the product is, why it exists, and the full backend-owned tech stack. It does not restate frontend implementation details beyond the integration points the backend must expose.

## 1. Purpose

The Agentic AI-Based Job Portal is a portfolio-grade demonstration of Agentic AI workflows: tool calling, human-in-the-loop (HITL) governance, streaming AI reasoning traces, and interactive candidate validation loops, presented to recruiters and interviewers. The backend is responsible for all business logic, data integrity, AI orchestration, and security guarantees the demo depends on.

## 2. Tech Stack (Backend Slice)

| Layer | Technology / Detail |
|---|---|
| Backend framework | ASP.NET Core (C#) Web API with Entity Framework Core (EF Core) |
| AI Orchestration | Microsoft Agent Framework (Orchestrator Agent + Specialized Tools) |
| LLM Provider | GitHub Models (GPT-4.1-mini free tier) |
| Database & Storage | Supabase PostgreSQL (relational data + jsonb + pgvector) and Supabase Storage (1MB max resume/logo uploads) |
| Real-time & Caching | SignalR (reasoning trace streaming, live status notifications) and Redis (rate-limiting queue and SignalR backplane) |
| Document Extraction | PdfPig (PDF text extraction) and DocumentFormat.OpenXml (DOCX text extraction) |
| Deployment | Render (Docker registry pattern via GHCR) |

The frontend is React + PrimeReact and consumes the backend exclusively through the REST/SignalR API surface described in [15-api-contracts-backend.md](15-api-contracts-backend.md). The backend never assumes anything about frontend rendering details.

## 3. In-Scope Backend Responsibilities

- Custom JWT auth (Access + Refresh), permanent role selection, Quick Demo Login seed accounts.
- Hard-wall onboarding enforcement, resume parsing (PdfPig/OpenXml + GPT-4.1-mini, 0-credit), profile management, recruiter verification.
- Jobs & Applications CRUD, dynamic `screening_questions`/`answers` jsonb persistence, duplicate-application prevention, withdrawal rules.
- ATS Match Scoring Engine (4-pillar weighted calculation with dynamic re-weighting).
- Agentic AI Orchestrator + 6 tools, credit ledger, Redis-backed platform rate limiting, pgvector-backed agent memory.
- Recruiter job posting lifecycle, applicant pipeline, ownership scoping, in-app notifications, dashboard aggregation.
- AI failure handling (no credit deduction on failure), secrets management, CORS configuration for the frontend origin.
- Deployment: Dockerized API, Render web service via GHCR image pull.

## 4. Explicitly Out of Scope

Per SRS §9 — do not implement any of the following:

- Third-party OAuth SSO (Google/LinkedIn).
- SMTP/Email infrastructure and password-reset flows.
- Multi-recruiter shared company accounts.
- Real payment gateway/subscription checkouts.
- Image-based OCR for scanned PDFs.
- Multi-provider LLM failovers.
- GDPR-style data export/account deletion flows.
- Admin analytics dashboard — admin scope is limited strictly to recruiter verification approve/reject.
- Automated test suite — manual testing during the build/polish phase is sufficient for this scope.
- Deep mobile-responsive polish beyond basic usability (this is primarily a frontend concern, noted here since it affects API design expectations — no special mobile API variants are needed).

See [08-out-of-scope.md](08-out-of-scope.md) for the full verification checklist.

## 5. Core Features & Priority (Backend-relevant)

| # | Feature | Priority |
|---|---|---|
| 1 | Guest browsing read endpoints (jobs list/detail) | P0 |
| 2 | JWT auth + hard-wall onboarding + resume parse | P0 |
| 3 | Dynamic jsonb screening questions (persistence + validation) | P0 |
| 4 | ATS Match Scoring Engine | P0 |
| 5 | Agentic AI Orchestrator (6 tools) + SignalR streaming | P0 |
| 6 | AI credit ledger + Redis platform rate limit | P0 |
| 7 | Recruiter job posting lifecycle + applicant pipeline | P0 |
| 8 | In-app notifications | P1 |
| 9 | Responsible-AI disclaimer metadata (flagging AI-generated content) | P1 |
| 10 | Deployment to Render via GHCR | P0 |

## 6. Assumptions & Constraints Relevant to Backend

- Solo developer/agent execution, 14-day timeline, manual testing only (no automated test suite in scope).
- LLM: GitHub Models GPT-4.1-mini (free tier), 150 req/day platform cap, 10 req/min via Redis token bucket.
- Uploads capped at 1MB; extracted resume text capped at 12,000 chars (~3,000 tokens).
- Chat context: sliding window of last 6 turns (~2,000 tokens), augmented by pgvector top-K memory retrieval.
- Credit quota: 500 credits / rolling 30 days per user, deducted only on success.
- DB: Supabase PostgreSQL with jsonb for `screening_questions`/`answers` and pgvector for embeddings.

## See Also

- [09-backend-folder-structure.md](09-backend-folder-structure.md) — where this all lives in code.
- [10-implementation-plan-backend-tracks.md](10-implementation-plan-backend-tracks.md) — day-by-day build plan.
- [INDEX.md](INDEX.md) — full file index.
