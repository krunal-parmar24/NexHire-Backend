# Backend Folder Structure (ASP.NET Core)

**Derived from:** Architecture & Folder Structure Guide §1 (Architecture Recap), §2 (Backend Folder Structure), §4 (Cross-Cutting Conventions), §5 (Traceability Checklist)

## Architecture Recap

- **Backend:** ASP.NET Core 9 Web API, layered as `Domain → Application → Infrastructure → API`, internally following `Controllers → Services → Repositories (EF Core) → PostgreSQL`.
- **Agents module:** wraps Microsoft Agent Framework; each tool implements a common `IAgentTool` interface, registered via DI.

## Folder Tree

```
/backend
├── src/
│   ├── JobPortal.Domain/                     # Domain layer — entities, enums, no external deps
│   │   ├── Entities/
│   │   │   ├── User.cs
│   │   │   ├── Company.cs
│   │   │   ├── Job.cs
│   │   │   ├── Application.cs
│   │   │   ├── CreditLedger.cs
│   │   │   ├── ChatConversation.cs
│   │   │   ├── ChatMessage.cs
│   │   │   ├── ChatEmbedding.cs
│   │   │   ├── AgentMemory.cs
│   │   │   ├── SessionSummary.cs
│   │   │   ├── ToolCallLog.cs
│   │   │   ├── JobEmbedding.cs
│   │   │   ├── ProfileEmbedding.cs
│   │   │   └── Notification.cs
│   │   ├── Enums/
│   │   │   ├── UserRole.cs                   # JobSeeker, Recruiter
│   │   │   ├── JobStatus.cs                  # Draft, Active, Closed, Expired
│   │   │   ├── ApplicationStatus.cs          # Applied, Shortlisted, Interview, Rejected, Hired, Withdrawn
│   │   │   ├── VerificationStatus.cs         # Unverified, Pending Review, Verified
│   │   │   └── MemoryType.cs                 # FACT, PREF, GOAL
│   │   └── Interfaces/
│   │       └── IAgentTool.cs                 # Common tool interface
│   │
│   ├── JobPortal.Application/                # Application layer — services, DTOs, business rules
│   │   ├── Services/
│   │   │   ├── AuthService.cs
│   │   │   ├── OnboardingService.cs
│   │   │   ├── ProfileService.cs
│   │   │   ├── JobService.cs
│   │   │   ├── ApplicationService.cs
│   │   │   ├── AtsScoringService.cs          # 4-pillar weighted scoring
│   │   │   ├── ResumeParsingService.cs       # PdfPig/OpenXml + GPT-4.1-mini parse
│   │   │   ├── EmbeddingService.cs           # 1536-dim embeddings, pluggable provider
│   │   │   ├── CreditLedgerService.cs        # Deduct-on-success logic
│   │   │   ├── NotificationService.cs
│   │   │   └── DashboardService.cs
│   │   ├── Agents/
│   │   │   ├── OrchestratorAgent.cs          # Intent parsing + tool planning
│   │   │   └── Tools/
│   │   │       ├── JobSearchMatchTool.cs     # 5 credits
│   │   │       ├── ApplicationAutofillTool.cs # 10 credits, 4-phase loop
│   │   │       ├── ApplicationStatusTool.cs  # 2 credits
│   │   │       ├── BulkApplyTool.cs          # 10 credits/app
│   │   │       ├── JdGenerationTool.cs       # 15 credits
│   │   │       └── CandidateScreeningTool.cs # 5 credits/candidate
│   │   ├── BackgroundJobs/
│   │   │   ├── FactExtractionWorker.cs       # Async AgentMemories extraction
│   │   │   └── SessionSummaryWorker.cs       # Async episodic summarization
│   │   ├── DTOs/
│   │   │   ├── Auth/
│   │   │   ├── Onboarding/
│   │   │   ├── Jobs/
│   │   │   ├── Applications/
│   │   │   └── Agent/
│   │   └── Interfaces/
│   │       ├── IUserRepository.cs
│   │       ├── ICompanyRepository.cs
│   │       ├── IJobRepository.cs
│   │       ├── IApplicationRepository.cs
│   │       └── ... (one per repository)
│   │
│   ├── JobPortal.Infrastructure/             # Infrastructure layer — EF Core, external services
│   │   ├── Persistence/
│   │   │   ├── JobPortalDbContext.cs
│   │   │   ├── Migrations/
│   │   │   └── Repositories/
│   │   │       ├── UserRepository.cs
│   │   │       ├── CompanyRepository.cs
│   │   │       ├── JobRepository.cs
│   │   │       ├── ApplicationRepository.cs
│   │   │       ├── CreditLedgerRepository.cs
│   │   │       ├── ChatRepository.cs
│   │   │       ├── AgentMemoryRepository.cs  # pgvector cosine similarity queries
│   │   │       ├── JobEmbeddingRepository.cs
│   │   │       ├── ProfileEmbeddingRepository.cs
│   │   │       └── NotificationRepository.cs
│   │   ├── Auth/
│   │   │   ├── JwtTokenService.cs            # Access + refresh issuance
│   │   │   └── PasswordHasher.cs             # BCrypt
│   │   ├── DocumentExtraction/
│   │   │   ├── PdfTextExtractor.cs           # PdfPig
│   │   │   └── DocxTextExtractor.cs          # DocumentFormat.OpenXml
│   │   ├── Llm/
│   │   │   └── GitHubModelsClient.cs         # GPT-4.1-mini calls
│   │   ├── RateLimiting/
│   │   │   └── RedisTokenBucketLimiter.cs    # 10/min, 150/day global cap
│   │   ├── Realtime/
│   │   │   └── AgentHub.cs                   # SignalR hub, Redis backplane
│   │   └── Storage/
│   │       └── SupabaseStorageClient.cs      # Resume/logo uploads (1MB cap)
│   │
│   └── JobPortal.Api/                        # API layer — controllers, middleware, composition root
│       ├── Controllers/
│       │   ├── AuthController.cs             # /api/auth/register, /login, /refresh
│       │   ├── OnboardingController.cs       # /api/onboarding/jobseeker, /recruiter, /parse-resume
│       │   ├── ProfileController.cs          # /api/profile/jobseeker, /recruiter
│       │   ├── JobsController.cs             # /api/jobs, /api/jobs/{id}, /api/jobs/mine
│       │   ├── ApplicationsController.cs     # /api/applications, /mine, /{id}/withdraw
│       │   ├── ApplicantsController.cs       # /api/jobs/{id}/applicants, /api/applications/{id}/status
│       │   ├── DashboardController.cs        # /api/dashboard/recruiter
│       │   ├── AtsController.cs              # /api/jobs/{id}/match-score
│       │   ├── AgentController.cs            # /api/agent/chat, /autofill, /bulk-apply, /generate-jd, /screen-candidates
│       │   ├── CreditsController.cs          # /api/credits/balance
│       │   ├── NotificationsController.cs    # /api/notifications, /{id}/read
│       │   └── AdminController.cs            # /api/admin/companies/{id}/verify
│       ├── Middleware/
│       │   ├── JwtAuthMiddleware.cs
│       │   ├── OnboardingGuardMiddleware.cs  # Hard-wall onboarding enforcement
│       │   ├── GlobalExceptionMiddleware.cs
│       │   └── CreditDeductionInterceptor.cs # Deduct only on successful AI response
│       ├── Program.cs                        # DI registration, EF Core, SignalR, Redis, CORS
│       └── appsettings.json
│
├── tests/                                    # Manual test collections (no automated suite per scope)
│   └── JobPortal.Postman/
│       └── JobPortal.postman_collection.json
│
├── Dockerfile
└── JobPortal.sln
```

## Cross-Cutting Conventions

- **Naming:** PascalCase for C# classes/files.
- **Layer boundaries:** Domain has zero outward dependencies; Application depends only on Domain; Infrastructure implements Application interfaces; Api composes Infrastructure + Application at startup — matching the Controllers → Services → Repositories → PostgreSQL flow.
- **Tool extensibility:** every new Agentic AI capability must be added as a new class under `Application/Agents/Tools/` implementing `IAgentTool`, registered via DI — no orchestrator logic should be hardcoded per tool.
- **Shared schema contract:** the `screening_questions` / `answers` jsonb field-type map must remain identical between backend DTOs and the frontend `DynamicFormRenderer` (frontend concern; backend must not drift this shape without updating the API Contracts doc).
- **Secrets:** LLM API keys and JWT signing secrets are read only from server-side configuration (`appsettings`/environment variables) in the Api/Infrastructure layers and must never be referenced anywhere that could leak to a client response.

## Traceability Checklist

- [ ] Backend layering matches Domain/Application/Infrastructure/API, Controllers→Services→Repositories→PostgreSQL
- [ ] All 7 core database entities (Users, Companies, Jobs, Applications, CreditLedger, ChatConversations, Notifications) have a corresponding Domain entity file
- [ ] All 7 vector-memory entities have corresponding Domain entities and repositories
- [ ] All 6 agent tools have a corresponding tool class file under `Application/Agents/Tools/`
- [ ] All API endpoints have a corresponding controller
- [ ] Background workers (fact extraction, session summarization) run as hosted services, not inline in the request path

## Implementation Checklist

- [ ] Create solution with four projects: Domain, Application, Infrastructure, Api
- [ ] Domain layer contains only entities, enums, and interfaces — no EF Core or external package references
- [ ] Application layer contains services, DTOs, and the Agents/Tools folder implementing `IAgentTool`
- [ ] Infrastructure layer contains EF Core `DbContext`, repositories, JWT/auth helpers, document extraction, LLM client, rate limiter, SignalR hub, and storage client
- [ ] Api layer contains only Controllers, Middleware, and composition-root `Program.cs`
- [ ] Each of the 6 agent tools lives as its own class file under `Application/Agents/Tools/`
- [ ] Each controller maps 1:1 to the endpoint groups defined in [10-implementation-plan-backend-tracks.md](10-implementation-plan-backend-tracks.md)
