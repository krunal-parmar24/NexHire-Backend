# Recruiter Job Posting & Applicant Management (Backend)

**Derived from:** SRS v3.1 §7 (Recruiter — Job Posting & Applicant Management)

## 1. Job Posting — Create, Edit, Publish

- Recruiter creates a job posting with: Title, Description, Requirements, Location, Job Type (Full-time/Part-time/Contract), Salary Range, Remote/Hybrid/Onsite.
- Optional AI assist: the JD Generation Agent (see [05-agentic-ai-orchestrator-and-tools.md](05-agentic-ai-orchestrator-and-tools.md)) drafts the description from a recruiter brief; the response must always include `requiresManualReview: true` — the backend must never auto-publish a JD-generated description.
- Job posting states: **Draft** (not visible to Job Seekers) → **Active** (visible, accepting applications) → **Closed** (manually closed by recruiter) → **Expired** (auto-closed after a fixed, platform-wide **30-day period** if not manually closed first). Implement the 30-day auto-expiry as a scheduled/background check (e.g., a hosted service or scheduled job comparing `created_at`/an `expires_at` column against current time).
- Recruiter can edit an Active posting; edits must **not** retroactively affect already-submitted applications — do not cascade job-field changes into `Applications.answers` or any submitted snapshot data.

## 2. Screening Question Builder (Backend Persistence)

See [03-screening-questions-and-applications-data-model.md](03-screening-questions-and-applications-data-model.md) for the full jsonb schema contract. The backend must:

- Accept and persist `screeningQuestions[]` as part of job creation/update, validating field types against the fixed 6-value set.
- Enforce ownership: only the recruiter who owns a job (via `Companies.recruiter_id`) can create/edit/publish it or view its screening questions in edit mode.

## 3. Applicant Management

- Recruiter views a list of applicants per job posting: submitted answers, resume, profile summary — scoped strictly to jobs the requesting recruiter owns.
- Applicant status pipeline: **Applied → Shortlisted → Interview → Rejected / Hired**, moved manually by the recruiter via `PATCH /api/applications/{id}/status`. A **"Withdrawn"** status can also appear, but it is set only by the Job Seeker (via `PATCH /api/applications/{id}/withdraw`) — the recruiter-facing status endpoint must reject any attempt to set "Withdrawn" directly.
- Status changes must be reflected back to the Job Seeker via the Application Status Tool — implement this as a **live lookup** against the current `Applications.status` value, never a cached/denormalized copy that could go stale.
- Optional AI assist: the Candidate Screening Agent ranks/summarizes applicants against the JD — this is **suggestion only**; the endpoint must never write to `Applications.status` itself. Only the recruiter's explicit status-change call can do that.

## 4. Ownership & Access

- One Recruiter account maps to its own company profile and its own job postings. **No multi-recruiter / shared-company / team access is in scope** — do not build any sharing/permission model beyond single-owner.
- Every recruiter-facing query (jobs, applicants, dashboard) must be scoped server-side to `recruiter_id` — enforce this at the repository/query level (e.g., `WHERE company.recruiter_id = @currentUserId`), not just via a client-side filter. A recruiter attempting to access another recruiter's job/applicant data must receive `403`.

## 5. Minimal Notifications

In-app notifications only (no email/SMS infrastructure) for:

- New applicant received (Recruiter)
- Application status changed (Job Seeker)
- Company verification approved (Recruiter)

Implement via a `NotificationService` triggered at the point each event occurs (new `Applications` row insert, status update, admin verification approval).

## 6. Recruiter Dashboard Overview

`GET /api/dashboard/recruiter` returns an aggregate query scoped to the current recruiter:

- Count of Active job postings.
- Total applicants across all postings.
- Applicants pending review (Applied status).
- Verification status (Verified/Unverified/Pending Review).

## Implementation Checklist (Backend)

- [ ] Implement job posting CRUD (`POST /api/jobs`, `PUT /api/jobs/{id}`, `PATCH /api/jobs/{id}/status`, `GET /api/jobs/mine`) with ownership checks
- [ ] Integrate JD Generation Agent output as always `requiresManualReview: true`; never persist/publish without an explicit subsequent save/publish call
- [ ] Implement job posting state machine: Draft → Active → Closed → Expired
- [ ] Implement 30-day platform-wide auto-expiry as a scheduled/background job
- [ ] Ensure edits to Active postings do not retroactively alter already-submitted `Applications` data
- [ ] Persist `screeningQuestions[]` with field-type validation (see data model doc)
- [ ] Implement `GET /api/jobs/{id}/applicants` scoped to the owning recruiter only
- [ ] Implement `PATCH /api/applications/{id}/status` (Applied→Shortlisted→Interview→Rejected/Hired only — reject "Withdrawn" from this endpoint)
- [ ] Implement `PATCH /api/applications/{id}/withdraw` as the only path to set "Withdrawn," with a pre-final-decision guard
- [ ] Wire the Application Status Tool as a live lookup against current `Applications.status`
- [ ] Implement Candidate Screening Agent as read-only/suggestion-only (no writes to `Applications.status`)
- [ ] Enforce recruiter data scoping (`recruiter_id`) at the repository/query level on every recruiter-facing endpoint; return `403` on cross-recruiter access attempts
- [ ] Implement `NotificationService` with triggers for: new applicant, status change, verification approved
- [ ] Implement `GET /api/dashboard/recruiter` aggregate query (active postings, total applicants, pending review, verification status)

## Integration Points (What the Frontend Consumes)

- `POST /api/jobs`, `PUT /api/jobs/{id}`, `PATCH /api/jobs/{id}/status`, `GET /api/jobs/mine`
- `GET /api/jobs/{id}/applicants`, `PATCH /api/applications/{id}/status`
- `GET /api/dashboard/recruiter`
- `GET /api/notifications`, `PATCH /api/notifications/{id}/read`
- `POST /api/agent/generate-jd`, `POST /api/agent/screen-candidates`

See [15-api-contracts-backend.md](15-api-contracts-backend.md) for exact shapes.
