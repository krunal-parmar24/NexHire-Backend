# Backend Knowledge Base — Index

This is the entry point for a Backend-only AI coding agent working on the Agentic AI Job Portal. Every file below is self-contained and derived exclusively from the source documents in `/references` (no invented requirements). Start with [01-product-overview.md](01-product-overview.md).

| File | Description | Derived From |
|---|---|---|
| [01-product-overview.md](01-product-overview.md) | Product purpose, backend tech stack, in/out-of-scope features, priorities | SRS §1, Implementation Plan §1 |
| [02-auth-and-onboarding-backend.md](02-auth-and-onboarding-backend.md) | JWT issuance, onboarding hard-wall enforcement, resume parsing service, verification logic | SRS §3 |
| [03-screening-questions-and-applications-data-model.md](03-screening-questions-and-applications-data-model.md) | `screening_questions`/`answers` jsonb schema — the authoritative source of truth | SRS §4 |
| [04-ats-scoring-engine.md](04-ats-scoring-engine.md) | Full weighted ATS scoring logic, certification matching, dynamic re-weighting | SRS §5 |
| [05-agentic-ai-orchestrator-and-tools.md](05-agentic-ai-orchestrator-and-tools.md) | Orchestrator, 6 agent tools, credit ledger, Redis rate limiting, pgvector agent memory | SRS §6 |
| [06-recruiter-job-posting-and-applicant-management-backend.md](06-recruiter-job-posting-and-applicant-management-backend.md) | Job posting lifecycle, ownership scoping, applicant pipeline, notifications, dashboard aggregation | SRS §7 |
| [07-data-model-and-security.md](07-data-model-and-security.md) | Core entities, AI failure handling, secrets management | SRS §8 |
| [08-out-of-scope.md](08-out-of-scope.md) | Explicitly deferred features — hard guardrails for scope control | SRS §9 |
| [09-backend-folder-structure.md](09-backend-folder-structure.md) | ASP.NET Core layered folder structure and cross-cutting backend conventions | Architecture Guide |
| [10-implementation-plan-backend-tracks.md](10-implementation-plan-backend-tracks.md) | Day 1–14 backend-only task checklist, keeping Day numbering aligned with frontend | Implementation Plan |
| [11-coding-standards-backend.md](11-coding-standards-backend.md) | C#/.NET coding standards, naming conventions, authoritative Git/commit conventions | Coding Standards |
| [12-local-dev-setup-backend.md](12-local-dev-setup-backend.md) | Docker Compose (Postgres/pgvector + Redis), env vars, seed data, full backend setup | Local Dev Setup |
| [13-nfr-backend.md](13-nfr-backend.md) | API response-time targets, scalability, availability, observability, security, data retention | NFR |
| [14-testing-strategy-backend.md](14-testing-strategy-backend.md) | Postman collection structure, backend/API-testable checklist by day, final regression checklist | Testing Strategy |
| [15-api-contracts-backend.md](15-api-contracts-backend.md) | Full API contract — implementation source of truth for all endpoints | API Contracts |
| [16-cicd-pipeline.md](16-cicd-pipeline.md) | Full GitHub Actions → GHCR → Render CI/CD pipeline | CI/CD Pipeline |

## Cross-Reference Note

Frontend implementation details (React component structure, PrimeReact theming, CSS conventions) are intentionally **not** duplicated here. Where the backend needs a frontend integration point (CORS origin, expected response shape a frontend consumes), it is included directly in the relevant file above.
