# Screening Questions & Applications Data Model (Backend)

**Derived from:** SRS v3.1 §4 (Dynamic Screening Form Architecture — jsonb)

## 1. Why jsonb

Recruiters can attach dynamic screening questions to job postings. To optimize database queries and flex with dynamic field creation, question definitions and submitted answers are stored using PostgreSQL `jsonb` columns — **this is the authoritative source of truth for the schema**; the frontend only ever consumes/produces this same shape (see the Frontend KB's `04-dynamic-screening-form.md` for the consumer-side view).

## 2. Database Schema Definition (jsonb)

### `Jobs.screening_questions` (jsonb)

Stores an array of question field objects:

```json
{
  "id": "q1_experience",
  "label": "Years of experience with ASP.NET Core?",
  "type": "numeric",
  "required": true
}
```

### `Applications.answers` (jsonb)

Stores candidate responses mapped to question IDs:

```json
{
  "question_id": "q1_experience",
  "value": "4"
}
```

## 3. Supported Field Types

`text | single-select | multi-select | file upload | yes/no | numeric` — this set is fixed by the SRS and must match exactly what the recruiter-side builder and seeker-side form both render. The backend does not need to validate rendering, but it must validate:

- Each question object has a valid `type` from this fixed set.
- Each mandatory (`required: true`) question has a corresponding non-empty answer in `Applications.answers` at submission time.

## 4. EF Core & Persistence Rules

- Map `Jobs.screening_questions` and `Applications.answers` as `jsonb` columns using EF Core's built-in JSON column mapping (or Npgsql's jsonb support) — **never** as a stringified/manually-serialized column.
- These columns are read/written as structured JSON objects in C# (e.g., a `List<ScreeningQuestion>` / `List<Answer>` model), not raw strings, at the Application layer boundary.

## 5. Related Backend Logic (Screening Question Builder — Server Side)

While the UI for the builder is frontend-owned, the backend must support:

- `POST /api/jobs` / `PUT /api/jobs/{id}` accepting a full `screeningQuestions[]` array as part of the job payload, validated against the fixed type set.
- A small library of **preset questions** (e.g., "Years of relevant experience," "Work authorization status," "Notice period") — these can be represented as static seed/constant data the backend exposes or simply documents for the frontend to hardcode; no dedicated preset-questions table/endpoint is required by the SRS beyond what's already implied.
- Recruiters can preview the screening form exactly as a Job Seeker would see it — this is purely a frontend rendering concern; the backend just needs to serve the same `screeningQuestions[]` shape consistently whether requested for builder, preview, or live form contexts.

## Implementation Checklist (Backend)

- [ ] Define `Jobs.screening_questions` as a PostgreSQL `jsonb` column storing an array of question objects (`id`, `label`, `type`, `required`)
- [ ] Define `Applications.answers` as a PostgreSQL `jsonb` column storing `question_id` → `value` mappings
- [ ] Ensure EF Core model mapping supports jsonb read/write for both columns (never stringified JSON)
- [ ] Validate `type` against the fixed 6-value set on job creation/update
- [ ] Validate all mandatory questions have a corresponding answer at application submission time; reject with `400` otherwise
- [ ] Ensure `GET /api/jobs/{id}` and `POST /api/applications` use byte-for-byte identical field-shape conventions for `screeningQuestions`/`answers`

## Integration Points (What the Frontend Needs)

- `GET /api/jobs/{id}` → `screeningQuestions[]` (exact shape above, camelCase in JSON responses).
- `POST /api/jobs`, `PUT /api/jobs/{id}` (Recruiter) → same shape, editable.
- `POST /api/applications` → `answers[]` keyed by `questionId`.

See [15-api-contracts-backend.md](15-api-contracts-backend.md) for full request/response examples.
