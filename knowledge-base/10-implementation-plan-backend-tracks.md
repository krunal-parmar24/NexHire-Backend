# Implementation Plan — Backend Track (Day 1–14)

**Derived from:** Implementation Plan v1.0 §3 (Development Plan), §4.1–§4.6 (Task Breakdown), §5 (Development Order), §6 (Milestones)

> Day numbering is identical to the Frontend track (see Frontend KB's `09-implementation-plan-frontend-tracks.md`) for cross-team traceability. Each day lists only the Backend-owned work; Frontend tasks for the same day live in the Frontend knowledge base.

## Day 1 — Project Scaffolding & Auth Foundations
- [x] Init ASP.NET Core 9 solution (API/Application/Domain/Infrastructure).
- [x] Configure EF Core + Supabase Postgres connection.
- [x] Create Users, Companies tables/migrations.
- [x] Implement JWT access+refresh issuance; password hashing (BCrypt).
- [x] API endpoints: `POST /api/auth/register`, `POST /api/auth/login`, `POST /api/auth/refresh`.
- [x] Testing: manual Postman collection for auth endpoints.
- **Dependencies:** Supabase project provisioned first.
- **DoD:** user can register (role-permanent), log in, refresh token.

## Day 2 — Onboarding Hard-Wall & Resume Parsing
- [x] Onboarding service; PdfPig/OpenXml text extraction (cap 12k chars); GitHub Models call to parse resume into profile fields; enforce "no platform access until complete" via middleware/route guard.
- [x] DB changes: extend Users with `profile` jsonb, `onboarding_completed` flag; add `credit_balance`, `credit_reset_date`.
- [x] API endpoints: `POST /api/onboarding/jobseeker`, `POST /api/onboarding/recruiter`, `POST /api/onboarding/parse-resume`.
- [x] AI/Agent: resume parse via GPT-4.1-mini (0-credit).
- [x] Testing: manual test with sample PDF/DOCX resumes; validate 1MB/12k-char caps.
- **Dependencies:** Day 1 auth.
- **DoD:** onboarding blocks all navigation until submitted; resume parse populates ≥5 fields correctly on sample resume.

## Day 3 — Guest Browsing, Search & Job Listings
- [x] Jobs read endpoints with filter/pagination (keyword, location, type, remote/hybrid/onsite); seed sample jobs.
- [x] DB changes: create Jobs table (status enum: Draft/Active/Closed/Expired).
- [x] API endpoints: `GET /api/jobs`, `GET /api/jobs/{id}`.
- [x] Testing: verify guest can browse without auth.
- **Dependencies:** Day 1 auth (for gating), Jobs table.
- **DoD:** guest sees jobs, filters work.

## Day 4 — Recruiter Job Posting & Screening Question Builder
- [x] Jobs CRUD (Draft/Active/Closed/Expired transitions); `screening_questions` jsonb persistence; ownership checks (recruiter_id scoping).
- [x] DB changes: add `screening_questions` jsonb column to Jobs.
- [x] API endpoints: `POST /api/jobs`, `PUT /api/jobs/{id}`, `PATCH /api/jobs/{id}/status`, `GET /api/jobs/mine`.
- [x] Testing: create job with mixed question types; verify state transitions.
- **Dependencies:** Day 1 onboarding (recruiter fields), Day 3 Jobs table.
- **DoD:** job persists with jsonb schema.

## Day 5 — Dynamic Application Form & Submission Rules
- Applications CRUD; duplicate-application prevention; withdraw logic (status=Withdrawn, visible to recruiter); `answers` jsonb persistence.
- DB changes: create Applications table (job_id, user_id, answers jsonb, status, submitted_at); unique constraint (job_id, user_id).
- API endpoints: `POST /api/applications`, `GET /api/applications/mine`, `PATCH /api/applications/{id}/withdraw`.
- Testing: duplicate apply blocked; withdraw before Hired/Rejected works, blocked after.
- **Dependencies:** Day 4 dynamic schema, Day 2 profile data.
- **DoD:** seeker can apply, view status, withdraw; recruiter sees Withdrawn in pipeline.

## Day 6 — Applicant Management & Recruiter Dashboard
- Applicant list per job (answers, resume, profile summary); status transition endpoint (Applied→Shortlisted→Interview→Rejected/Hired); dashboard aggregate query.
- DB changes: none (reuse Applications/Jobs); add indices on job_id/status.
- API endpoints: `GET /api/jobs/{id}/applicants`, `PATCH /api/applications/{id}/status`, `GET /api/dashboard/recruiter`.
- Testing: status transitions reflected instantly; scoping verified (recruiter sees only own jobs).
- **Dependencies:** Day 5 Applications table.
- **DoD:** all pipeline states reachable; dashboard counts match DB truth.

## Day 7 — ATS Match Scoring Engine
- `AtsScoringService`: Skills Coverage, Experience Fit, Certification Match, Domain/Title Match with dynamic re-weighting; semantic title match via lightweight LLM call or embedding similarity.
- DB changes: none (computed on-demand; optionally cache score on Applications).
- API endpoints: `GET /api/jobs/{id}/match-score`, internal service reused by Job Search & Match tool.
- Testing: unit tests for weight redistribution logic (cert vs. no-cert JD scenarios).
- **Dependencies:** Day 2 profile, Day 4 job requirements.
- **DoD:** score matches SRS weight table on 3+ manual test cases including no-cert redistribution.

## Day 8 — Agent Framework, Vector Memory Setup + Job Search/Match & Autofill Tools
- Register Microsoft Agent Framework; `IAgentTool` interface; enable pgvector extension on Supabase; create ChatMessages, ChatEmbeddings, AgentMemories, SessionSummaries, ToolCallLogs, JobEmbeddings, ProfileEmbeddings (HNSW cosine index, dim 1536); implement embedding service + async fact-extraction worker; implement Job Search & Match Tool (5 credits) and Application Autofill Tool (10 credits, 4-phase loop); SignalR AgentHub for streaming; Redis token-bucket rate limiter (10/min, 150/day).
- DB changes: create ChatMessages, ChatEmbeddings, AgentMemories, SessionSummaries, ToolCallLogs, JobEmbeddings, ProfileEmbeddings, and CreditLedger tables; backfill embeddings for seed data.
- API endpoints: `POST /api/agent/chat` (SignalR-invoked), `GET /api/credits/balance`.
- Testing: manual chat "find me jobs matching my profile" returns ranked results with streamed trace; verify returning-user session recalls prior stated preference via memory retrieval.
- **Dependencies:** Day 7 ATS engine, Day 1 auth (session), Redis provisioned, pgvector extension enabled.
- **DoD:** single in-flight request enforced; rate limit and credit deduction verified; failed calls don't deduct credits; a memory stated in session 1 is correctly retrieved in session 2.

## Day 9 — Autofill Review Loop + Bulk Apply + JD Generation Tools
- Interactive Pause-and-Prompt phase (client prompt when required fields unmapped); Review & Inline Edit persistence (save-to-profile option); Bulk Apply Tool (10 credits/app, ATS≥80% filter, batch confirm); JD Generation Tool (15 credits).
- DB changes: none new; ensure `Applications.answers` write-back path to `Users.profile`.
- API endpoints: `POST /api/agent/autofill`, `POST /api/agent/bulk-apply`, `POST /api/agent/generate-jd`.
- Testing: trigger autofill on job with unmapped required question → prompt appears; bulk apply respects 80% threshold.
- **Dependencies:** Day 8 orchestrator, Day 5 application flow, Day 7 ATS score.
- **DoD:** no auto-submission without explicit confirm click at every irreversible step.

## Day 10 — Status Tool, Candidate Screening Tool & Notifications
- Application Status Tool (2 credits, live DB lookup); Candidate Screening Agent (5 credits/candidate, suggestion only); Notifications service (new applicant, status changed, verification approved).
- DB changes: create Notifications table.
- API endpoints: `GET /api/notifications`, `PATCH /api/notifications/{id}/read`, `POST /api/agent/screen-candidates`.
- Testing: trigger each notification event; verify screening suggestions don't auto-change status.
- **Dependencies:** Day 6 applicant management, Day 8 orchestrator.
- **DoD:** all 6 tools invokable from chat with correct credit costs and streamed traces.

## Day 11 — Responsible AI Disclaimer Support, Admin Verification, Profile Editing
- Recruiter verification approve/reject endpoint (minimal admin); profile edit endpoints (re-parse resume on re-upload, 0-credit); material-change re-verification trigger.
- DB changes: none (reuse `verification_status`).
- API endpoints: `PUT /api/profile/jobseeker`, `PUT /api/profile/recruiter`, `PATCH /api/admin/companies/{id}/verify`.
- Testing: verify re-upload triggers re-parse; verification reset on company-name change.
- **Dependencies:** Day 2 onboarding, Day 9 JD generation.
- **DoD:** every AI-generating endpoint's response is distinguishable so the disclaimer can render on all AI outputs (backend contract requirement, not backend rendering).

## Day 12 — Hardening: Rate Limits, Credit Guardrails, Error Handling
- Verify 500-credit/30-day rolling window reset logic; zero-balance blocks AI but not manual flows; platform 150/day cap surfaces `AI_BUSY` without credit deduction; global exception middleware; secrets audit (no keys in frontend bundle or API responses).
- DB changes: none; add indices for CreditLedger queries.
- API endpoints: no new; harden existing with validation/error contracts.
- Testing: manual regression across all flows: guest, seeker, recruiter, AI tools, edge cases (duplicate apply, expired job, withdrawn app).
- **Dependencies:** Days 1–11 complete.
- **DoD:** zero critical bugs in manual regression checklist; secrets confirmed server-side only.

## Day 13 — Deployment & End-to-End Integration
- Dockerize API; configure Render web service; environment variables (LLM key, JWT secret, DB conn string, Redis URL) via Render secrets; enable CORS for frontend domain.
- DB changes: run all migrations against production Supabase instance; seed demo accounts (Quick Demo Login for both roles).
- API endpoints: smoke-test all endpoints against production.
- Testing: full E2E manual walkthrough (backend side): guest→apply→login→onboarding→AI autofill→submit; recruiter post→screen→hire.
- **Dependencies:** Day 12 hardening complete.
- **DoD:** public URL live; Quick Demo Login works for both roles; no console errors.

## Day 14 — Final Polish, Documentation & Demo Readiness
- Final log review; performance spot-check on ATS scoring and chat streaming latency.
- DB changes: none; verify seed data covers all screening question types.
- API endpoints: final Postman collection export for documentation.
- AI/Agent: record/verify a full demo script covering all 6 tools + HITL confirmations + disclaimer visibility.
- Testing: final regression pass against Acceptance Criteria.
- **Dependencies:** Day 13 deployment.
- **DoD:** all acceptance criteria pass; demo script runs without failure.

## Development Order Notes (Backend-Relevant)

1. Auth + DB provisioning (Day 1) — blocks everything downstream.
2. Onboarding + resume parse (Day 2) — blocks profile-dependent features.
3. Jobs read path (Day 3), then write path + screening builder (Day 4).
4. Applications + dynamic form schema (Day 5) — shared schema contract with frontend renderer.
5. Applicant management + recruiter dashboard (Day 6) — pure consumption of Day 5 data.
6. ATS scoring engine (Day 7) — standalone service, backend-only.
7. Agent framework + first 2 tools (Day 8) — depends on ATS engine (7) and auth (1).
8. Remaining 4 AI tools (Days 9–10) — parallelizable across tools since each is an independent `IAgentTool` implementation.
9. Compliance/profile polish (Day 11), hardening (Day 12), deployment (Day 13), final QA (Day 14).

**Key parallelization rule:** once a backend entity + DTO contract is defined (even before full implementation), the frontend can build against mocked responses matching that contract.

## Milestones (Backend Scope)

- **Week 1 (Days 1–7):** Auth, onboarding, resume parsing; guest job browsing/search backend; recruiter job posting + dynamic screening persistence; job seeker application flow (manual, no AI) backend; applicant management + dashboard backend; ATS Match Scoring Engine complete and unit-tested.
- **Week 2 (Days 8–14):** All 6 Agentic AI tools operational with streamed reasoning traces; HITL guardrails enforced server-side; credit system, Redis rate limiting, disclaimer-support metadata live; notifications and profile management complete; hardened error/rate-limit/credit-exhaustion handling; production deployment on Render with seeded demo accounts.
