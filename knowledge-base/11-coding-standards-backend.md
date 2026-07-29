# Coding Standards — C# / .NET (Backend)

**Derived from:** Coding Standards & Style Guide §1 (General Principles), §2 (C#/.NET Backend Standards), §4 (Naming Conventions — backend rows), §5 (Error Handling & Logging), §6 (Git & Commit Conventions — authoritative copy)

## 1. General Principles

- Follow the layering boundaries defined in the Architecture Guide strictly: Domain has zero outward dependencies; Application depends only on Domain; Infrastructure implements Application interfaces; Api composes everything at startup.
- Every new Agentic AI tool must be a discrete class implementing `IAgentTool`, registered via DI — never hardcoded into the Orchestrator.
- Prefer explicit, readable code over clever/compact code — this is a portfolio project reviewed by interviewers and recruiters, so clarity has interview value.
- Keep functions/methods short and single-purpose; extract helper methods rather than nesting deeply.

## 2. Project & File Organization

- One class per file; file name matches class name exactly (e.g., `JobSearchMatchTool.cs`).
- Namespace mirrors folder structure exactly as defined in [09-backend-folder-structure.md](09-backend-folder-structure.md) (e.g., `JobPortal.Application.Agents.Tools`).
- Controllers are thin: they validate input, call a Service, and map results to HTTP responses. No business logic in controllers.

## 3. Formatting

- 4-space indentation, no tabs.
- Opening braces on a new line (Allman style), consistent with default .NET/Visual Studio formatting.
- One statement per line; no inline multi-statement lines.
- Use `var` when the type is obvious from the right-hand side; use explicit types when it improves readability (e.g., interface return types).

## 4. Async & Nullability

- All I/O-bound methods (DB calls, HTTP calls, LLM calls) must be `async Task<T>` — no `.Result` or `.Wait()` blocking calls anywhere.
- Enable nullable reference types (`<Nullable>enable</Nullable>`) project-wide; annotate optional fields explicitly (e.g., `Certifications` list on Job Seeker profile).
- Use `CancellationToken` parameters on all async service/repository methods that call external systems (LLM, Redis, Supabase).

## 5. Dependency Injection

- Register services with the narrowest applicable lifetime: `Scoped` for anything touching `DbContext` or per-request state; `Singleton` for stateless services (e.g., `EmbeddingService` client wrapper); `Transient` for lightweight stateless helpers.
- Register all `IAgentTool` implementations via assembly scanning in `Program.cs` so adding a new tool requires no manual registration edit beyond the class itself.

## 6. EF Core & jsonb Conventions

- Map `Jobs.screening_questions` and `Applications.answers` as `jsonb` columns using EF Core's built-in JSON column mapping (or Npgsql's jsonb support) — never as a stringified/manually-serialized column.
- All pgvector embedding columns (`ChatEmbeddings`, `AgentMemories`, `SessionSummaries`, `JobEmbeddings`, `ProfileEmbeddings`) use a standardized `vector(1536)` type with an HNSW `vector_cosine_ops` index.
- Migrations are named descriptively: `<sequence>_<PascalCaseDescription>` (e.g., `005_AddScreeningQuestionsToJobs`).
- Never call `.SaveChanges()` inside a loop; batch changes and save once per logical operation.

## 7. Credit & Rate-Limit Interceptor Pattern

- Credit deduction must happen via a single cross-cutting interceptor/decorator wrapping AI tool execution — deduction fires only after a successful LLM response, never before or on failure.
- Rate-limit checks (Redis token bucket) must run **before** any LLM call is attempted, not after.

## 8. Naming Conventions (Backend Rows)

| Element | Convention | Example |
|---|---|---|
| C# class/interface | PascalCase | `ApplicationAutofillTool`, `IAgentTool` |
| C# method | PascalCase | `CalculateAtsScore()` |
| C# private field | `_camelCase` | `_creditLedgerRepository` |
| C# constant | PascalCase | `MaxResumeUploadSizeBytes` |
| EF Core migration | `<seq>_PascalCaseDescription` | `008_CreateAgentMemoriesTable` |
| API route | kebab-case | `/api/agent/bulk-apply` |

> Frontend naming conventions (React components, hooks, etc.) are documented in the Frontend knowledge base's `10-coding-standards-frontend.md` — not duplicated here.

## 9. Error Handling & Logging

- A single global MVC exception filter (`NexHire.Api.Filters.ApiExceptionFilter`, registered via `options.Filters.Add<ApiExceptionFilter>()` in `Program.cs`) catches unhandled exceptions thrown from any controller action and maps them to the standard `{ error: { code, message } }` envelope — controllers must **not** contain per-action `try/catch` blocks for these known exception types.
- Domain/application failures are represented by typed exceptions in `NexHire.Application.Exceptions`, each carrying its own machine-readable `Code`: `NotFoundException` (→ 404), `ConflictException` (→ 409), `AuthenticationException` (→ 401). Plain `ArgumentException` maps to 400 (`VALIDATION_ERROR`) and `UnauthorizedAccessException` maps to a 403 `ForbidResult` — both handled centrally by the same filter. Add new typed exceptions here (not raw BCL exceptions) when a new distinct error code is needed, and add the corresponding `case` to `ApiExceptionFilter`.
- AI/agent failures follow the stated rule exactly: no credit deduction on failure, and the API returns a clear error code (`AI_GENERATION_FAILED`) rather than a silent failure.
- Logging uses the built-in `Microsoft.Extensions.Logging` (`ILogger<T>` injected via constructor) — every class that can silently swallow an exception (document/text extraction, external HTTP/LLM calls) must inject `ILogger<T>` and log a `Warning` with the exception before falling back to a default value. Log at `Information` for normal request flow, `Warning` for handled/expected failures (rate limit hit, credit exhausted, extraction/parse fallback), `Error` for unhandled exceptions.
- Never log JWT tokens, LLM API keys, or full resume text content.

## 10. Git & Commit Conventions (Authoritative — Shared with Frontend)

This is the authoritative copy of the Git/commit convention; the Frontend knowledge base cross-references this file rather than duplicating it.

- Conventional Commits format: `<type>(<scope>): <description>`
  - Types: `feat`, `fix`, `refactor`, `test`, `docs`, `chore`, `perf`, `style`
  - Scope: matches the Implementation Plan's task area (e.g., `feat(onboarding): add resume parse endpoint`)
- Reference the Implementation Plan Day number in the PR description (e.g., "Implements Day 5 — Dynamic Application Form & Submission Rules").
- Small, single-purpose commits/PRs preferred over large multi-feature commits, to keep traceability to individual Implementation Plan tasks intact.

## 11. Refactor-Established Conventions (Authoritative)

The following conventions were established during the July 2026 backend quality-audit refactor and **must be followed by default for all new backend code** in this project.

### 11.1 Folder/Layer Structure

- Controller → Service (interface, in `Application/Interfaces`) → Repository (interface, in `Application/Interfaces`) → `DbContext`. Controllers must never inject `DbContext` or a repository directly — always go through a Service interface.
- All public service/repository **interfaces** live in `NexHire.Application/Interfaces/`, regardless of which layer implements them. Concrete implementations that require an Infrastructure-layer dependency (EF Core, JWT libraries, external HTTP clients) live in `NexHire.Infrastructure/Services|Persistence|Llm|DocumentExtraction/`; pure-application-logic implementations live in `NexHire.Application/Services/`.
- Auth is a first-class vertical slice like Jobs/Applications/Onboarding: `IAuthService`/`AuthService` in Application, `AuthController` in Api depends only on `IAuthService` — never on `DbContext`, `IPasswordHasher`, or `IJwtTokenService` directly.

### 11.2 Constants & Magic Values

- Centralize magic strings/numbers into a `Common/Constants/` folder **scoped to the layer that owns the concern**:
  - `NexHire.Application/Common/Constants/` — business-rule constants shared across layers (e.g., `FileUploadConstants.MaxResumeSizeBytes`).
  - `NexHire.Api/Common/Constants/` — API-host concerns only (e.g., `CorsConstants`, `ApiRouteConstants`).
  - Provider-specific literals (endpoint URLs, model names, prompts) live next to their client, e.g. `NexHire.Infrastructure/Llm/LlmConstants.cs`.
- Never inline a repeated literal (URL, size limit, route prefix, policy name) directly in a controller/service/middleware body — add it to the appropriate constants class first.

### 11.3 Shared Cross-Cutting Helpers

- User-id extraction from `ClaimsPrincipal` must use `NexHire.Api.Extensions.ClaimsPrincipalExtensions`: `User.GetUserId()` (throws if missing/invalid — for actions where the `[Authorize]` attribute guarantees a valid claim) or `User.TryGetUserId(out var id)` (for actions that need to return `Unauthorized()` gracefully). Do not re-inline `Guid.Parse`/`Guid.TryParse` claim-extraction logic in a controller.
- HTTP error responses for known exception types must **not** be hand-built in controllers — see §9 (`ApiExceptionFilter`).

### 11.4 DI & Interface Pattern for New Services/Repositories

1. Define the interface in `NexHire.Application/Interfaces/I<Name>.cs` with XML `<summary>` docs on every member.
2. Implement it in the layer that owns the dependency (`Application/Services` for pure orchestration, `Infrastructure/*` for anything touching EF Core, HTTP, or a third-party SDK).
3. Add class-level `/// <inheritdoc cref="I<Name>"/>` on the implementation.
4. Register in `Program.cs` with the narrowest applicable lifetime (see §5).
5. If the implementation needs a NuGet package not already referenced by that project, do not add it without explicit approval — prefer exposing a primitive-typed method on the interface (e.g., `bool TryReadAccessToken(string token, out Guid userId, out string role)`) so the dependent layer doesn't need the package.

### 11.5 Exception Handling & Logging Pattern

- See §9 for the authoritative `ApiExceptionFilter` + typed-exception pattern.
- Any class that can encounter a recoverable failure (parsing, external API calls, file extraction) must inject `ILogger<T>` via constructor and log a `Warning` with the exception before returning a fallback value — never swallow silently with a bare `catch { }`.

## Implementation Checklist

- [ ] Enable nullable reference types across all backend projects
- [ ] Configure `.editorconfig` enforcing 4-space indentation and Allman braces
- [ ] Register all `IAgentTool` implementations via assembly scanning
- [ ] Configure EF Core jsonb mapping for `screening_questions` and `answers`
- [ ] Configure pgvector HNSW indexes on all 5 embedding tables
- [ ] Implement credit-deduction interceptor as a single cross-cutting decorator
- [ ] Configure `ILogger<T>` log levels (Information/Warning/Error) per this document's guidance
- [ ] Adopt Conventional Commits format for all commits/PRs
