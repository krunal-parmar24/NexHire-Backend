# Manual Testing Strategy (Backend)

**Derived from:** Manual Testing Strategy §1 (Testing Approach), §2 (Postman Collection Structure), §3 (Manual Test Case Checklist — backend/API-testable items, by Day), §4 (Bug Severity & Triage Convention), §5 (Final Regression Checklist — backend phrasing)

## 1. Testing Approach

- **No automated test suite is in scope** — manual testing during the build/polish phase is explicitly sufficient.
- Manual testing happens **daily**, at the end of each Implementation Plan day, against that day's own Testing/DoD lines.
- A final full regression pass occurs on Day 14 against the complete Acceptance Criteria table.
- **Postman is the designated tool** for API-level manual verification throughout.

## 2. Postman Collection Structure

Maintain a single growing Postman collection, `JobPortal.postman_collection.json`, organized into folders matching backend subsystems:

```
JobPortal (collection)
├── Auth                     # register, login, refresh
├── Onboarding               # jobseeker, recruiter, parse-resume
├── Profile                  # jobseeker, recruiter edit
├── Jobs (Guest)             # GET /api/jobs, GET /api/jobs/{id}
├── Jobs (Recruiter)         # POST/PUT/PATCH /api/jobs, mine
├── Applications             # POST, mine, withdraw
├── Applicant Management     # applicants list, status update
├── Dashboard                # recruiter aggregate
├── ATS                      # match-score
├── Agent                    # chat, autofill, bulk-apply, generate-jd, screen-candidates
├── Credits                  # balance
├── Notifications            # list, read
└── Admin                    # company verify
```

- Environment variables: `{{baseUrl}}`, `{{accessToken}}`, `{{refreshToken}}`, `{{demoSeekerEmail}}`, `{{demoRecruiterEmail}}`.
- Export an updated collection at the end of each day that adds new endpoints, and a final export as part of Day 14's deliverable.

## 3. Backend/API-Testable Checklist (By Day)

### Day 1 — Auth Foundations
- [ ] `POST /api/auth/register` — Job Seeker; confirm role is permanently set
- [ ] `POST /api/auth/register` — Recruiter; confirm role is permanently set
- [ ] `POST /api/auth/login` returns JWT (access + refresh)
- [ ] `POST /api/auth/refresh` returns a new valid access token

### Day 2 — Onboarding & Resume Parsing
- [ ] New user is blocked (via middleware) from all protected endpoints until onboarding is submitted
- [ ] Upload a sample PDF resume under 1MB via `POST /api/onboarding/parse-resume`; confirm ≥5 profile fields auto-fill correctly
- [ ] Upload a sample DOCX resume under 1MB; confirm extraction works
- [ ] Attempt upload over 1MB; confirm server-side rejection (400)
- [ ] Confirm resume parse response always shows `creditsDeducted: 0`

### Day 3 — Guest Browsing & Search
- [ ] `GET /api/jobs` and `GET /api/jobs/{id}` work without an auth header
- [ ] Keyword, location, job type, and remote/hybrid/onsite filters all return correct results

### Day 4 — Recruiter Job Posting & Screening Builder
- [ ] `POST /api/jobs` persists all 6 screening-question field types correctly
- [ ] Job posting state transitions (Draft→Active→Closed/Expired) enforced via `PATCH /api/jobs/{id}/status`
- [ ] `GET /api/jobs/mine` returns only the requesting recruiter's own postings

### Day 5 — Dynamic Application Form & Submission Rules
- [ ] `POST /api/applications` succeeds for a valid dynamic form submission
- [ ] Duplicate application to the same job by the same user returns `409 DUPLICATE_APPLICATION`
- [ ] `PATCH /api/applications/{id}/withdraw` succeeds pre-final-decision; returns `409 WITHDRAWAL_NOT_ALLOWED` after Hired/Rejected
- [ ] Withdrawn application remains queryable via `GET /api/jobs/{id}/applicants` (not deleted/hidden)

### Day 6 — Applicant Management & Recruiter Dashboard
- [ ] `GET /api/jobs/{id}/applicants` returns answers, resume, and profile summary
- [ ] `PATCH /api/applications/{id}/status` supports Applied→Shortlisted→Interview→Rejected/Hired and persists
- [ ] `GET /api/dashboard/recruiter` counts (active postings, total applicants, pending review) match DB truth
- [ ] Cross-recruiter access to another recruiter's applicants returns `403`

### Day 7 — ATS Match Scoring Engine
- [ ] Score matches the SRS weight table for a JD that includes certification requirements
- [ ] Score correctly redistributes weight (+15% Skills, +5% Experience) for a JD with no certification requirements, with `certificationWeightRedistributed: true`
- [ ] Optional fields (e.g., Expected Salary) are excluded from scoring unless configured as a mandatory screening question
- [ ] Test at least 3 manual (user, job) pairs including one no-cert redistribution case

### Day 8 — Agent Framework, Vector Memory & First 2 Tools
- [ ] Chat prompt "find me jobs matching my profile" returns ranked results with a streamed reasoning trace via SignalR
- [ ] A single in-flight request is enforced server-side — sending a second message while one is processing is rejected
- [ ] Rate limit (10/min, 150/day) triggers `AI_BUSY` (429) without credit deduction when exceeded
- [ ] Failed AI calls do not deduct credits (`creditsDeducted: 0` or omitted, no CreditLedger row written)
- [ ] A fact/preference stated in one session is correctly recalled in a later session via cosine-similarity retrieval

### Day 9 — Autofill Loop, Bulk Apply, JD Generation
- [ ] Autofill on a job with an unmapped required question returns `unresolvedRequiredQuestions` (non-empty)
- [ ] "Save to profile" correctly persists an edited value back to `Users.profile`
- [ ] No application is created without an explicit subsequent `POST /api/applications` call (autofill never auto-submits)
- [ ] `POST /api/agent/bulk-apply` only includes jobs with ATS score ≥ 80%
- [ ] `POST /api/agent/generate-jd` always returns `requiresManualReview: true`

### Day 10 — Status Tool, Candidate Screening, Notifications
- [ ] Chat query "what's my application status" returns a correct, live pipeline stage (not cached/stale)
- [ ] `POST /api/agent/screen-candidates` never writes to `Applications.status`
- [ ] Notification triggers fire correctly for: new applicant received, application status changed, verification approved
- [ ] All 6 agent tools are invokable with the correct credit cost deducted per call

### Day 11 — AI Disclaimer Support, Admin Verification, Profile Editing
- [ ] Every AI-generating endpoint's response is distinguishable enough for the frontend to attach a disclaimer (chat, autofill, JD generation, match explanations)
- [ ] Resume re-upload on profile edit triggers a free re-parse (0-credit)
- [ ] Changing a company's name resets `verification_status` to "Pending Review"
- [ ] `PATCH /api/admin/companies/{id}/verify` approves/rejects correctly

### Day 12 — Hardening
- [ ] Zero AI credit balance returns `CREDIT_EXHAUSTED` (429) for AI endpoints only; manual search/apply endpoints remain fully functional
- [ ] Platform 150/day cap exhaustion returns `AI_BUSY` (429) without deducting user credits
- [ ] No unhandled exception appears anywhere in a full manual Postman walkthrough of guest, seeker, recruiter, and AI flows
- [ ] Edge cases pass: duplicate apply, expired job listing, withdrawn application
- [ ] Confirm no secrets (LLM key, JWT secret) appear in any API response body or log output

### Day 13 — Deployment & E2E Integration
- [ ] Full guest→apply→login→onboarding→AI autofill→submit journey works against the production Render URL
- [ ] Full recruiter post→screen(AI)→manage→hire journey works against the production Render URL
- [ ] Quick Demo Login works for both roles in production
- [ ] CORS allows the production frontend origin

### Day 14 — Final Polish & Demo Readiness
- [ ] Full regression pass against every backend-relevant row in Section 5 below
- [ ] Final Postman collection exported and matches all live endpoints

## 4. Bug Severity & Triage Convention

| Severity | Definition | Action |
|---|---|---|
| Blocker | Breaks a P0 feature or the day's DoD; blocks demo | Fix same day before moving to next day's tasks |
| Major | Feature works but violates a stated acceptance criterion | Fix before Day 12 hardening begins |
| Minor | Cosmetic/UX issue not affecting stated requirements | Fix during Day 14 polish pass if time permits |
| Deferred | Out-of-scope per SRS §9 | Log and explicitly do not fix |

## 5. Final Regression Checklist (Backend Phrasing)

- [ ] Guest Browsing — `GET /api/jobs`/`GET /api/jobs/{id}` work unauthenticated with correct filter behavior
- [ ] Auth & Onboarding — validations enforced server-side, permanent role, hard-wall gate middleware, ≥5 auto-filled fields at 0 credit cost
- [ ] Apply Gate — duplicate applications rejected with correct error code
- [ ] Withdraw — allowed before Rejected/Hired at the API level, rejected after with correct error code
- [ ] Dynamic Screening Forms — `screeningQuestions`/`answers` shapes identical across builder/preview/seeker consumption paths
- [ ] ATS Scoring — matches weight table, correct no-cert redistribution, optional fields excluded unless mandatory
- [ ] Job Posting Lifecycle — Draft→Active→Closed/Expired enforced server-side, 30-day auto-expiry job runs, edits don't retroactively affect submitted applications
- [ ] Applicant Management — recruiter scoping enforced server-side (403 on violation), status pipeline enforced, status visible via live Status Tool lookup
- [ ] Orchestrator & Tools — all 6 tools invokable, correct credit costs, streamed traces, single in-flight enforcement server-side
- [ ] Autofill Loop — unresolved required questions surfaced correctly, save-to-profile persists, no auto-submission
- [ ] Bulk Apply — ATS≥80% filter enforced server-side, batch confirmation required before any application is created
- [ ] Credits & Rate Limits — 500/30-day quota enforced, zero-balance blocks AI only, 150/day cap returns `AI_BUSY` without deduction, failed calls never deduct
- [ ] Notifications — new applicant, status change, verification approved all trigger correctly
- [ ] Vector Memory & Intent Continuity — returning-user prompt correctly retrieves prior facts/summary via cosine similarity, async without added latency
- [ ] Security — JWT secret and LLM API key never present in any API response, log, or source-controlled file
- [ ] Deployment — public Render URL live, Quick Demo Login works for both roles, no unhandled exceptions in full E2E walkthrough

## Implementation Checklist

- [ ] Create and maintain `JobPortal.postman_collection.json` with the folder structure in Section 2
- [ ] Run the Day-N backend checklist (Section 3) at the end of each of the 14 build days before proceeding
- [ ] Log any failing case using the severity convention in Section 4
- [ ] Run the full Section 5 regression checklist on Day 14 before considering the backend demo-ready
- [ ] Export final Postman collection as part of Day 14 documentation deliverable
