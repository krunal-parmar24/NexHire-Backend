# Auth & Onboarding (Backend)

**Derived from:** SRS v3.1 §3 (Authentication & User Onboarding)

## 1. Custom JWT Authentication

Built entirely on custom JWT token issuance (Access + Refresh tokens) managed in ASP.NET Core. **Third-party OAuth SSO (Google/LinkedIn) and SMTP-based password reset flows are strictly excluded** — do not implement either. Role selection (Job Seeker vs. Recruiter) occurs at registration and is **strictly permanent** — enforce this server-side on every protected endpoint, not just at registration time.

- A **"Quick Demo Login"** feature must be backed by pre-seeded test accounts for both roles, seeded via `DevSeeder`/production seed data (see [12-local-dev-setup-backend.md](12-local-dev-setup-backend.md)).
- Registration enforces: email format validation, email uniqueness, password strength, duplicate-account checks.
- Registration requires a mandatory Terms & Conditions / Privacy Policy consent flag (`acceptedTerms: true`) before account creation — reject registration if false/absent.
- Passwords are hashed with BCrypt before storage — plaintext passwords are never logged or persisted.
- JWT access tokens are short-lived; refresh tokens rotate on use (`JWT_ACCESS_TOKEN_EXPIRY_MINUTES`, `JWT_REFRESH_TOKEN_EXPIRY_DAYS` — see env vars in Local Dev Setup).

## 2. Mandatory Hard-Wall Onboarding

Zero platform access (browsing or dashboard) is allowed until onboarding is completed in a single sitting. Implement this as route-guard middleware (`OnboardingGuardMiddleware`) that blocks all protected routes until `Users.onboarding_completed` is true.

- **Job Seeker Fields (Mandatory):** Full Name, Email, Phone, Current Title, Total Experience (Years), Skills (min. 3 tags), Preferred Job Type, Preferred Location.
- **Job Seeker Fields (Optional):** Certifications, Portfolio/LinkedIn links, Expected Salary Range.
- **Free Resume Parsing:** Job Seekers can upload a resume (max 1MB, enforced server-side regardless of any client-side check). Text is extracted via PdfPig (PDF) / OpenXml (DOCX), capped at 12,000 characters, then parsed via GPT-4.1-mini to auto-fill onboarding fields at **0 AI credit cost** — this call must bypass the CreditLedger deduction path entirely.
- **Recruiter Fields & Verification:** Company Name, Industry, Size, Designation. New Recruiter accounts receive an "Unverified" badge but gain immediate access to post jobs (do not block job posting on verification). Admin approval updates status to "Verified" via `PATCH /api/admin/companies/{id}/verify`.

## 3. Profile Management (Post-Onboarding)

- Once onboarding is complete, both roles can edit their profile at any time — implement as a distinct edit endpoint, not a re-run of the onboarding endpoint, since onboarding is a one-time gate but editing is unlimited.
- Job Seeker can update: resume (re-upload triggers a free AI re-parse under the same 0-credit rule as onboarding), skills, experience, job preferences, and optional fields.
- Recruiter can update: company details, logo, description, designation. **Material changes to company identity (e.g., company name) must reset `verification_status` to "Pending Review"** and re-apply the "Unverified" badge until re-approved — implement this as a service-layer check comparing the incoming company name to the stored value, not a database trigger.
- Mandatory fields remain mandatory on edit — reuse the same validation rules as onboarding (do not fork a separate, looser validation path for edits).

## Implementation Checklist (Backend)

- [ ] Implement custom JWT issuance (Access + Refresh tokens) in ASP.NET Core
- [ ] Implement BCrypt password hashing before any persistence
- [ ] Enforce role-permanence server-side on every protected endpoint (not just at registration)
- [ ] Seed Quick Demo Login accounts (Job Seeker + Recruiter)
- [ ] Implement `POST /api/auth/register` with role-permanent selection, T&C flag validation, email uniqueness/format, password strength, duplicate-account checks
- [ ] Implement `POST /api/auth/login`, `POST /api/auth/refresh` (rotate refresh token on use)
- [ ] Implement `OnboardingGuardMiddleware` blocking all protected routes until onboarding is complete
- [ ] Implement `POST /api/onboarding/jobseeker`, `POST /api/onboarding/recruiter`
- [ ] Implement resume upload (1MB server-side cap) + PdfPig/OpenXml extraction (12k char cap)
- [ ] Implement GPT-4.1-mini resume-to-profile parse at 0 credit cost (bypasses `CreditLedgerService` deduction)
- [ ] Implement Recruiter "Unverified"/"Verified"/"Pending Review" status logic with immediate job-posting access pre-verification
- [ ] Implement Admin approval endpoint updating Recruiter verification status
- [ ] Implement `PUT /api/profile/jobseeker`, `PUT /api/profile/recruiter` reusing onboarding validation rules
- [ ] Implement free AI re-parse on resume re-upload (0-credit rule, same code path as onboarding parse)
- [ ] Implement verification-status reset to "Pending Review" on material company-identity change

## Integration Points (What the Frontend Needs)

- Standard JWT bearer auth; `401` on missing/expired token, frontend attempts refresh.
- `onboardingCompleted` boolean returned from login/register/onboarding responses — the frontend route guard depends on this exact field.
- Resume parse and re-parse responses always include `creditsDeducted: 0` — the frontend credit meter should not react to this value changing.

See [15-api-contracts-backend.md](15-api-contracts-backend.md) for exact request/response shapes.
