# Data Model & Security Considerations (Backend)

**Derived from:** SRS v3.1 §8 (Data Model & Security Considerations)

## 1. Core Entities (Conceptual)

The following is a conceptual list to guide EF Core model/schema design, not a final ERD:

| Entity | Key Fields |
|---|---|
| Users | id, role, auth info, profile fields, credit_balance, credit_reset_date |
| Companies | id, recruiter_id, name, verification_status, details |
| Jobs | id, company_id, title, description, screening_questions[], status, created_at |
| Applications | id, job_id, user_id, answers[], status, submitted_at |
| SavedJobs | user_id, job_id, saved_at |
| CreditLedger | id, user_id, action_type, credits_deducted, timestamp — supports the credit meter and enables debugging/refunds |
| ChatConversations | id, user_id, messages[], created_at — supports chatbot context |
| PlatformAIUsage | date, request_count (in **Redis**, not PostgreSQL) — tracks the daily platform-wide GitHub Models cap, distinct from CreditLedger |

Additional vector-memory entities (ChatMessages, ChatEmbeddings, AgentMemories, SessionSummaries, ToolCallLogs, JobEmbeddings, ProfileEmbeddings) are detailed in [05-agentic-ai-orchestrator-and-tools.md](05-agentic-ai-orchestrator-and-tools.md) §7.

Full entity/enum breakdown by Domain-layer file lives in [09-backend-folder-structure.md](09-backend-folder-structure.md).

## 2. AI Failure Handling

- If an AI agent call fails or times out before producing a usable result (e.g., LLM API error, timeout, or GitHub Models rate limit hit), the credit for that action is **not deducted** — deduction happens only after a successful response is returned to the user.
- On failure, return a clear error (`AI_GENERATION_FAILED`) rather than a silent failure or generic crash, so the frontend can render an inline error state.

## 3. Secrets & Deployment Security

- LLM API keys and JWT signing secrets are kept **server-side only** and are never exposed to the React frontend.
- Enforced via environment variables in Render (production) and `.env` (local, git-ignored) — never hardcoded, never logged, never returned in any API response body.
- Recruiter data scoping (a recruiter sees only their own jobs/applicants) is enforced at the repository/query level, not just filtered client-side (see [06-recruiter-job-posting-and-applicant-management-backend.md](06-recruiter-job-posting-and-applicant-management-backend.md)).
- Role permanence (Job Seeker vs. Recruiter) is enforced server-side on every protected endpoint, not just hidden in the UI.

## Implementation Checklist (Backend)

- [ ] Design EF Core models for Users, Companies, Jobs, Applications, CreditLedger, ChatConversations
- [ ] Implement Redis-backed `PlatformAIUsage` tracking (date, request_count) distinct from `CreditLedger`
- [ ] Implement credit non-deduction on AI agent failure/timeout
- [ ] Build clear inline error messaging contract (`AI_GENERATION_FAILED`) for failed AI generations (no silent failure/crash)
- [ ] Store LLM API keys and JWT signing secrets server-side only; audit for any accidental frontend exposure or response-body leakage
- [ ] Confirm role-permanence and recruiter-scoping checks exist at the repository/query level, not only in controllers

## Integration Points (What the Frontend Needs)

- The frontend never receives raw secrets or internal entity IDs beyond what's already in the documented API contracts (see [15-api-contracts-backend.md](15-api-contracts-backend.md)). No additional data-model exposure is required.
