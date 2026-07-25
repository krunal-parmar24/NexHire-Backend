# Local Development Setup (Backend)

**Derived from:** Local Development Setup Guide §1 (Prerequisites), §2 (Repository Structure Recap), §3 (Local Services via Docker Compose), §4.1 (Backend Env Vars), §5 (Backend Local Setup), §7 (Seed Data), §8 (Running the Full Stack), §9 (Troubleshooting)

## 1. Prerequisites

| Tool | Version | Purpose |
|---|---|---|
| .NET SDK | 9.0.x | Backend build/run |
| Docker + Docker Compose | Latest | Local Postgres (pgvector) + Redis containers |
| Git | Latest | Version control |
| A GitHub account with GitHub Models access | — | LLM provider (GPT-4.1-mini free tier) |
| A Supabase account (or local Postgres+pgvector substitute) | — | Production DB target; local dev may use a containerized Postgres+pgvector image instead |

## 2. Repository Structure Recap

```
/
├── backend/        # ASP.NET Core solution (Domain/Application/Infrastructure/Api)
├── frontend/        # React + Vite + PrimeReact SPA
├── references/       # Source documentation (untouched)
├── .github/
│   └── workflows/   # CI/CD pipelines (GitHub Actions)
└── docker-compose.local.yml   # Local dev only, not used in production (Render deploys via GHCR images)
```

## 3. Local Services via Docker Compose

Create `docker-compose.local.yml` at the repo root:

```yaml
version: '3.9'

services:
  postgres:
    image: pgvector/pgvector:pg16
    container_name: jobportal-postgres-local
    restart: unless-stopped
    environment:
      POSTGRES_USER: jobportal
      POSTGRES_PASSWORD: localdevpassword
      POSTGRES_DB: jobportal_dev
    ports:
      - "5432:5432"
    volumes:
      - jobportal_pg_data:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U jobportal"]
      interval: 5s
      timeout: 5s
      retries: 5

  redis:
    image: redis:7-alpine
    container_name: jobportal-redis-local
    restart: unless-stopped
    ports:
      - "6379:6379"
    healthcheck:
      test: ["CMD", "redis-cli", "ping"]
      interval: 5s
      timeout: 5s
      retries: 5

volumes:
  jobportal_pg_data:
```

Start local services:

```bash
docker compose -f docker-compose.local.yml up -d
```

> The `pgvector/pgvector:pg16` image ships PostgreSQL with the `pgvector` extension pre-installed, matching the production Supabase pgvector requirement. Run `CREATE EXTENSION IF NOT EXISTS vector;` once via migration or manual psql command after first startup.

## 4. Environment Variables — `/backend/.env.example`

```env
# Database (local Docker Postgres by default; swap for Supabase connection string when testing against staging)
DATABASE_CONNECTION_STRING=Host=localhost;Port=5432;Database=jobportal_dev;Username=jobportal;Password=localdevpassword

# Redis (local Docker Redis by default)
REDIS_URL=localhost:6379

# JWT
JWT_SIGNING_SECRET=replace-with-a-long-random-local-only-secret
JWT_ACCESS_TOKEN_EXPIRY_MINUTES=15
JWT_REFRESH_TOKEN_EXPIRY_DAYS=7

# LLM Provider (GitHub Models GPT-4.1-mini)
LLM_API_KEY=replace-with-your-github-models-token
LLM_MODEL_NAME=gpt-4.1-mini
LLM_PLATFORM_DAILY_CAP=150
LLM_RATE_LIMIT_PER_MINUTE=10

# Supabase Storage (resume/logo uploads)
SUPABASE_STORAGE_URL=https://<your-project>.supabase.co/storage/v1
SUPABASE_STORAGE_KEY=replace-with-your-supabase-service-key
SUPABASE_STORAGE_BUCKET=job-portal-uploads

# Credit & Context Limits (values fixed by SRS Section 6.4 — do not change)
AI_CREDIT_QUOTA_PER_30_DAYS=500
RESUME_UPLOAD_MAX_BYTES=1048576
RESUME_TEXT_EXTRACTION_MAX_CHARS=12000
CHAT_SLIDING_WINDOW_TURNS=6

ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:8080
```

> Copy to `.env` locally and fill in real values. `.env` must be git-ignored; only `.env.example` is committed. `LLM_API_KEY` and `JWT_SIGNING_SECRET` must never leave this backend service configuration.

## 5. Backend Local Setup

```bash
cd backend

# Restore dependencies
dotnet restore JobPortal.sln

# Apply EF Core migrations against local Postgres
dotnet ef database update --project src/JobPortal.Infrastructure --startup-project src/JobPortal.Api

# Run the API
dotnet run --project src/JobPortal.Api
```

- API is available at `http://localhost:8080` (matches the Dockerfile `EXPOSE 8080`).
- Swagger/OpenAPI UI (if enabled) available at `http://localhost:8080/swagger`.
- Ensure CORS is configured to allow the frontend origin (`http://localhost:5173` locally).

## 6. Seed Data

A seed script/endpoint (run once after first migration) must populate:

- **Quick Demo Login accounts** — one Job Seeker, one Recruiter, pre-verified where applicable.
- **Sample job postings** covering all `JobStatus` values (Draft, Active, Closed, Expired).
- **Sample screening questions** covering all 6 field types (text, single-select, multi-select, file upload, yes/no, numeric).
- **A sample resume file** (PDF and DOCX) under 1MB for testing the resume-parsing flow.

Recommended location: `/backend/src/JobPortal.Infrastructure/Persistence/SeedData/` with a `DevSeeder.cs` invoked conditionally when `ASPNETCORE_ENVIRONMENT=Development`.

## 7. Running the Full Stack (Backend's Place in the Order)

1. `docker compose -f docker-compose.local.yml up -d` (Postgres + Redis)
2. `cd backend && dotnet run --project src/JobPortal.Api` (API + SignalR hub + background workers)
3. Frontend runs separately (`cd frontend && npm run dev`) — see Frontend KB's local setup file.
4. Open `http://localhost:5173`, log in via Quick Demo Login, verify guest browsing, onboarding, and chat all reach this backend.

## 8. Troubleshooting

| Symptom | Likely Cause | Fix |
|---|---|---|
| API fails to start: connection refused to Postgres | Docker Compose services not running or not healthy yet | Run `docker compose ps`, wait for healthcheck to pass, retry |
| SignalR client can't connect (reported from frontend) | CORS not configured for `localhost:5173`, or hub route mismatch | Verify backend CORS policy includes the frontend origin and hub route matches `VITE_SIGNALR_HUB_URL` |
| Resume parse returns empty fields | `LLM_API_KEY` missing/invalid, or file exceeds 1MB cap | Check `.env`, confirm file size under `RESUME_UPLOAD_MAX_BYTES` |
| "AI Busy" message immediately on first chat message | Redis token bucket not reset from a previous session | Flush local Redis: `docker exec jobportal-redis-local redis-cli FLUSHALL` (local only, never in production) |
| pgvector extension error on migration | Extension not created in the local Postgres instance | Connect via psql and run `CREATE EXTENSION IF NOT EXISTS vector;` |

## Implementation Checklist

- [ ] Create `docker-compose.local.yml` with pgvector-enabled Postgres + Redis services
- [ ] Create `/backend/.env.example` with all variables listed in Section 4
- [ ] Add `.env` to `.gitignore` in `/backend`
- [ ] Enable `CREATE EXTENSION IF NOT EXISTS vector;` in the first relevant migration
- [ ] Build `DevSeeder.cs` covering Quick Demo Login accounts, sample jobs (all statuses), sample screening questions (all 6 types), sample resumes
- [ ] Configure CORS to allow the frontend's local origin (`http://localhost:5173`)
- [ ] Verify full local stack boots in the order specified in Section 7
