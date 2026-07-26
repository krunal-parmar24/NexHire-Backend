# API Contracts — Backend Source of Truth

**Derived from:** API Contracts Reference (full document — this is the implementation source of truth for the backend)

> Every endpoint, field, and status code below maps to an entity or rule already defined in the SRS v3.1 or Implementation Plan v1.0. Field-level JSON shapes not explicitly dictated by the source docs are marked `[CONVENTION]` and may be refined without changing the underlying business rule. The Frontend knowledge base's `14-api-contracts-frontend.md` reframes this same contract as a consumer reference — keep both in sync if this contract changes.

## 1. Conventions

- All request/response bodies are JSON; `Content-Type: application/json`.
- All protected endpoints require `Authorization: Bearer <accessToken>`.
- Dates are ISO 8601 UTC strings (e.g., `2026-07-24T21:00:00Z`). `[CONVENTION]`
- IDs are GUID strings, matching the entity model. `[Source: Implementation Plan §2.5]`
- Pagination (where applicable) uses `page` and `pageSize` query params, returning `{ "items": [...], "totalCount": number, "page": number, "pageSize": number }`. `[CONVENTION]`

## 2. Error Envelope

`[CONVENTION — enforces the "clear inline error, no silent failure" rule from SRS §8.2]`

All non-2xx responses return a consistent shape, produced by `GlobalExceptionMiddleware`:

```json
{
  "error": {
    "code": "DUPLICATE_APPLICATION",
    "message": "You have already applied to this job.",
    "details": null
  }
}
```

| HTTP Status | Meaning |
|---|---|
| 400 | Validation error (e.g., missing required onboarding field) |
| 401 | Missing/invalid/expired JWT |
| 403 | Authenticated but role/ownership check failed (e.g., recruiter viewing another recruiter's applicants) |
| 404 | Resource not found |
| 409 | Conflict (e.g., duplicate application, duplicate email at registration) |
| 429 | AI rate limit or credit exhaustion (`AI_BUSY` or `CREDIT_EXHAUSTED` error codes) |
| 500 | Unhandled server error (caught by global exception middleware) |

AI-specific error codes: `AI_BUSY` (platform 150/day or 10/min cap hit — no credit deducted), `CREDIT_EXHAUSTED` (user's 500/30-day quota hit), `AI_GENERATION_FAILED` (LLM error/timeout — no credit deducted). `[Source: SRS §6.4, §8.2]`

## 3. Auth & Onboarding Endpoints

### `POST /api/auth/register`
```json
// Request
{ "email": "jane@example.com", "password": "SecurePass123!", "role": "JobSeeker", "acceptedTerms": true }
```
```json
// 201 Response
{ "userId": "b3f1...", "role": "JobSeeker", "onboardingCompleted": false }
```

### `POST /api/auth/login`
```json
// Request
{ "email": "jane@example.com", "password": "SecurePass123!" }
```
```json
// 200 Response
{ "accessToken": "eyJ...", "refreshToken": "eyJ...", "role": "JobSeeker", "onboardingCompleted": false }
```

### `POST /api/auth/refresh`
```json
// Request: { "refreshToken": "eyJ..." }
// 200 Response: { "accessToken": "eyJ...", "refreshToken": "eyJ..." }
```

### `POST /api/onboarding/jobseeker`
```json
// Request — fields per SRS §3.2
{
  "fullName": "Jane Doe", "phone": "+91-9999999999", "currentTitle": "Backend Engineer",
  "totalExperienceYears": 5, "skills": ["C#", "ASP.NET Core", "SQL"],
  "preferredJobType": "Full-time", "preferredLocation": "Remote",
  "certifications": [], "portfolioLinks": [], "expectedSalaryRange": null
}
```
```json
// 200 Response: { "onboardingCompleted": true }
```

### `POST /api/onboarding/recruiter`
```json
// Request — fields per SRS §3.2
{ "companyName": "Acme Corp", "industry": "Software", "size": "51-200", "designation": "HR Manager" }
```
```json
// 200 Response: { "onboardingCompleted": true, "verificationStatus": "Unverified" }
```

### `POST /api/onboarding/parse-resume`
```json
// multipart/form-data: file (max 1MB, PDF/DOCX per SRS §3.2)
```
```json
// 200 Response — auto-filled fields, 0 credit cost
{
  "parsedFields": { "fullName": "Jane Doe", "currentTitle": "Backend Engineer", "totalExperienceYears": 5, "skills": ["C#", "ASP.NET Core"] },
  "creditsDeducted": 0
}
```

## 4. Jobs & Applications Endpoints

### `GET /api/jobs`
```
Query: ?keyword=&location=&jobType=&remoteType=&page=1&pageSize=20
```
```json
// 200 Response
{
  "items": [
    { "id": "j1...", "title": "Backend Engineer", "companyName": "Acme Corp", "location": "Remote", "jobType": "Full-time", "remoteType": "Remote", "status": "Active", "createdAt": "2026-07-20T10:00:00Z" }
  ],
  "totalCount": 42, "page": 1, "pageSize": 20
}
```

### `GET /api/jobs/{id}`
```json
// 200 Response — includes screening_questions jsonb per SRS §4.1
{
  "id": "j1...", "title": "Backend Engineer", "description": "...", "requirements": "...",
  "location": "Remote", "jobType": "Full-time", "salaryRange": "10-15 LPA", "remoteType": "Remote", "status": "Active",
  "screeningQuestions": [ { "id": "q1_experience", "label": "Years of experience with ASP.NET Core?", "type": "numeric", "required": true } ]
}
```

### `GET /api/jobs/saved`
```json
// 200 Response — returns list of job IDs the authenticated user has saved
[ "j1...", "j2..." ]
```

### `POST /api/jobs/{id}/save`
```json
// Request: {}
// 200 Response — toggles the saved state
{ "isSaved": true }
```

### `POST /api/jobs` (Recruiter)
```json
// Request — fields per SRS §7.1
{
  "title": "Backend Engineer", "description": "...", "requirements": "...", "location": "Remote",
  "jobType": "Full-time", "salaryRange": "10-15 LPA", "remoteType": "Remote",
  "screeningQuestions": [ { "id": "q1_experience", "label": "Years of experience with ASP.NET Core?", "type": "numeric", "required": true } ]
}
```
```json
// 201 Response: { "id": "j1...", "status": "Draft" }
```

### `PATCH /api/jobs/{id}/status`
```json
// Request: { "status": "Active" }   // Draft | Active | Closed | Expired, per SRS §7.1
```

### `POST /api/applications`
```json
// Request — answers jsonb mapped to question IDs, per SRS §4.1
{ "jobId": "j1...", "answers": [ { "questionId": "q1_experience", "value": "4" } ] }
```
```json
// 201 Response: { "applicationId": "a1...", "status": "Applied" }
// 409 Response — duplicate application, per SRS §2.4
{ "error": { "code": "DUPLICATE_APPLICATION", "message": "You have already applied to this job." } }
```

### `PATCH /api/applications/{id}/withdraw`
```json
// 200 Response: { "status": "Withdrawn" }
// 409 Response — after final decision, per SRS §2.4
{ "error": { "code": "WITHDRAWAL_NOT_ALLOWED", "message": "Cannot withdraw after a final decision." } }
```

### `GET /api/jobs/{id}/applicants` (Recruiter)
```json
// 200 Response
{
  "items": [
    { "applicationId": "a1...", "applicantName": "Jane Doe", "status": "Applied",
      "answers": [ { "questionId": "q1_experience", "value": "4" } ],
      "resumeUrl": "https://.../resume.pdf", "profileSummary": "5 yrs backend engineer..." }
  ]
}
```

### `PATCH /api/applications/{id}/status` (Recruiter)
```json
// Request: { "status": "Shortlisted" }   // Applied|Shortlisted|Interview|Rejected|Hired, per SRS §7.3
```

### `GET /api/dashboard/recruiter`
```json
// 200 Response — per SRS §7.6
{ "activeJobPostings": 5, "totalApplicants": 34, "pendingReview": 12, "verificationStatus": "Verified" }
```

## 5. ATS Scoring Endpoint

### `GET /api/jobs/{id}/match-score`
```json
// 200 Response — per SRS §5.1
{
  "jobId": "j1...", "overallScore": 78,
  "breakdown": {
    "skillsCoverage": { "weight": 40, "score": 85 },
    "experienceFit": { "weight": 25, "score": 90 },
    "certificationMatch": { "weight": 20, "score": 0 },
    "domainTitleMatch": { "weight": 15, "score": 70 }
  },
  "certificationWeightRedistributed": false
}
```

## 6. Agent / AI Endpoints

### `POST /api/agent/chat` (invoked via SignalR, HTTP fallback shape shown)
```json
// Request: { "conversationId": "c1...", "message": "find me jobs matching my profile" }
```
```json
// Streamed response chunks via SignalR (per Implementation Plan §2.7):
{ "type": "reasoning", "content": "Searching job listings using your profile skills..." }
{ "type": "tool_call", "toolName": "JobSearchMatchTool", "creditsDeducted": 5 }
{ "type": "final", "content": "Here are 3 matching jobs...", "data": [ /* job + score list */ ] }
```

### `POST /api/agent/autofill`
```json
// Request: { "applicationDraftId": "d1...", "jobId": "j1..." }
```
```json
// 200 Response — 4-phase loop output, per SRS §6.3
{
  "phase": "ReviewAndInlineEdit",
  "draftAnswers": [ { "questionId": "q1_experience", "value": "4", "source": "AutoResolved", "editable": true } ],
  "unresolvedRequiredQuestions": [],
  "creditsDeducted": 10
}
```

### `POST /api/agent/bulk-apply`
```json
// Request: { "minAtsScore": 80 }
```
```json
// 200 Response — per SRS §6.1
{ "eligibleJobs": [ { "jobId": "j2...", "atsScore": 84 } ], "requiresBatchConfirmation": true, "creditsPerApplication": 10 }
```

### `POST /api/agent/generate-jd` (Recruiter)
```json
// Request: { "brief": "Looking for a senior backend engineer with .NET and Azure experience" }
```
```json
// 200 Response — draft only, never auto-published, per SRS §7.1
{ "draftDescription": "We are seeking...", "creditsDeducted": 15, "requiresManualReview": true }
```

### `POST /api/agent/screen-candidates` (Recruiter)
```json
// Request: { "jobId": "j1..." }
```
```json
// 200 Response — suggestion only, per SRS §7.3
{
  "suggestions": [ { "applicationId": "a1...", "rank": 1, "summary": "Strong ASP.NET Core match, 5 yrs exp" } ],
  "creditsDeducted": 5,
  "note": "Suggestion only — no status changes applied"
}
```

## 7. Credits, Notifications & Admin Endpoints

### `GET /api/credits/balance`
```json
// 200 Response — per SRS §6.4
{ "creditBalance": 350, "quota": 500, "resetDate": "2026-08-15T00:00:00Z" }
```

### `GET /api/notifications`
```json
// 200 Response
{ "items": [ { "id": "n1...", "type": "ApplicationStatusChanged", "message": "Your application status changed to Shortlisted", "isRead": false, "createdAt": "2026-07-24T18:00:00Z" } ] }
```

### `PATCH /api/notifications/{id}/read`
```json
// 200 Response: { "isRead": true }
```

### `PATCH /api/admin/companies/{id}/verify`
```json
// Request: { "approve": true }
// 200 Response: { "verificationStatus": "Verified" }
```

## Implementation Checklist

- [ ] Implement the shared error envelope across all controllers via `GlobalExceptionMiddleware`
- [ ] Ensure every AI endpoint response includes `creditsDeducted` (0 if the action failed) per SRS §8.2
- [ ] Ensure `screeningQuestions` / `answers` shapes match exactly between `GET /api/jobs/{id}` and `POST /api/applications`
- [ ] Ensure `PATCH /api/applications/{id}/withdraw` enforces the pre-final-decision guard with a 409 response
- [ ] Ensure `POST /api/applications` returns 409 with `DUPLICATE_APPLICATION` on duplicate submission
- [ ] Validate all AI rate-limit/credit-exhaustion responses use the `AI_BUSY` / `CREDIT_EXHAUSTED` error codes consistently
- [ ] Confirm `GET /api/jobs/{id}/match-score` breakdown reflects dynamic re-weighting when `certificationWeightRedistributed` is true
- [ ] Keep this file in sync with the Frontend knowledge base's `14-api-contracts-frontend.md` if any shape changes
