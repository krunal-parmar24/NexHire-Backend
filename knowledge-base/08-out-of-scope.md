# Out of Scope (Backend Guardrails)

**Derived from:** SRS v3.1 §9 (Out of Scope — Explicitly Deferred)

> These items must **not** be implemented anywhere in the backend, even if convenient or "easy to add while you're in there." Treat this as a hard boundary for scope control during the 14-day build.

## Explicitly Deferred

- Third-party OAuth SSO (Google/LinkedIn).
- SMTP/Email infrastructure and Password Reset flows.
- Multi-recruiter shared company accounts.
- Real payment gateway/subscription checkouts.
- Image-based OCR for scanned PDFs.
- Multi-provider LLM failovers.
- GDPR-style data export / account deletion flows.
- Admin analytics dashboard — admin scope is limited to recruiter verification approve/reject only.
- Automated test suite — manual testing during the build/polish phase is sufficient for this scope.
- Mobile-responsive polish beyond basic usability (primarily a frontend concern, noted here since it means no special mobile-optimized API variants are needed).

## Out-of-Scope Verification Checklist

- [ ] Confirm no third-party OAuth SSO is implemented
- [ ] Confirm no SMTP/email infrastructure or password-reset flow is implemented
- [ ] Confirm no multi-recruiter shared company account support is implemented
- [ ] Confirm no real payment gateway/subscription checkout is implemented
- [ ] Confirm no OCR support for scanned/image-based PDFs is implemented
- [ ] Confirm no multi-provider LLM failover logic is implemented
- [ ] Confirm no GDPR-style data export/account deletion flows are implemented
- [ ] Confirm admin scope is limited strictly to recruiter verification approve/reject
- [ ] Confirm reliance on manual testing only (no automated test suite required)
- [ ] Confirm mobile responsiveness is basic-usability only, not a polish target requiring dedicated backend support

Run this checklist as part of Day 12 hardening (see [10-implementation-plan-backend-tracks.md](10-implementation-plan-backend-tracks.md)) and again during the Day 14 final regression pass (see [14-testing-strategy-backend.md](14-testing-strategy-backend.md)).
