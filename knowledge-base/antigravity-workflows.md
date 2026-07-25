# Antigravity Workflow Playbooks — Backend

Step-by-step autonomous playbooks for common backend development tasks on NexHire. Before running any playbook, read [`backend/knowledge-base/INDEX.md`](INDEX.md) and open the numbered KB file(s) relevant to your task.

---

## Playbook 1 — Scaffold a New `IAgentTool`

Use this when adding a new AI capability to the orchestrator (e.g., a seventh agent tool beyond the existing six).

### Step 1 — Read the orchestrator contract

Open [`05-agentic-ai-orchestrator-and-tools.md`](05-agentic-ai-orchestrator-and-tools.md). Understand:
- The `IAgentTool` interface (defined in `JobPortal.Domain/Interfaces/IAgentTool.cs`).
- Credit cost and function of the existing 6 tools — ensure the new tool does not duplicate an existing one.
- The HITL rule: if the new tool performs an irreversible action (submission, publish, bulk operation), its endpoint must **never auto-execute** from a chat response — only a separate explicit user confirmation may trigger the final action.
- The credit deduction rule: deduction fires **only after a successful LLM response** via `CreditDeductionInterceptor`.

### Step 2 — Check the implementation plan

Open [`10-implementation-plan-backend-tracks.md`](10-implementation-plan-backend-tracks.md). Find the Day and task that describes this tool. Note the exact task wording — you will check it `[x]` when done.

### Step 3 — Define the tool class

Create `backend/src/JobPortal.Application/Agents/Tools/<ToolName>Tool.cs`. Follow the naming convention from [`11-coding-standards-backend.md`](11-coding-standards-backend.md): PascalCase, one class per file, namespace mirrors folder path.

Starter shape:

```csharp
// JobPortal.Application/Agents/Tools/<ToolName>Tool.cs
using JobPortal.Domain.Interfaces;

namespace JobPortal.Application.Agents.Tools;

public sealed class <ToolName>Tool : IAgentTool
{
    // Inject only Application-layer services or Domain interfaces — never Infrastructure directly.
    private readonly I<Dependency>Repository _<dependency>Repository;

    public <ToolName>Tool(I<Dependency>Repository <dependency>Repository)
    {
        _<dependency>Repository = <dependency>Repository;
    }

    public string Name => "<ToolName>";           // Must be unique across all tools
    public int CreditCost => <N>;                  // Per the SRS — do not invent a cost

    public async Task<ToolResult> ExecuteAsync(ToolInput input, CancellationToken cancellationToken)
    {
        // 1. Validate input
        // 2. Perform work (call repository / service)
        // 3. Return result — do NOT deduct credits here; CreditDeductionInterceptor handles it
        throw new NotImplementedException();
    }
}
```

> **No credit deduction inside `ExecuteAsync`.** The `CreditDeductionInterceptor` middleware wraps every tool call and deducts credits only after `ExecuteAsync` returns successfully.

### Step 4 — Add a DTO (if the tool has a unique response shape)

Create the response DTO under `backend/src/JobPortal.Application/DTOs/Agent/<ToolName>Response.cs`. Mirror the shape specified in [`15-api-contracts-backend.md`](15-api-contracts-backend.md).

### Step 5 — Expose a controller action (if the tool has a dedicated HTTP endpoint)

Open `backend/src/JobPortal.Api/Controllers/AgentController.cs`. Add a new action method. Controllers are thin — validate input, call `OrchestratorAgent` or the tool directly via the service layer, map to HTTP response.

```csharp
[HttpPost("/<route>")]
[Authorize]
public async Task<IActionResult> <ActionName>([FromBody] <ToolName>Request request, CancellationToken ct)
{
    var result = await _orchestratorAgent.InvokeToolAsync("<ToolName>", request, ct);
    return Ok(result);
}
```

Confirm the route matches what [`15-api-contracts-backend.md`](15-api-contracts-backend.md) specifies. Do not invent new routes.

### Step 6 — Registration (automatic via assembly scanning)

Because all `IAgentTool` implementations are registered via assembly scanning in `Program.cs`, **no manual DI registration is needed** for the tool class itself. Verify that `Program.cs` already contains the scanning call (e.g., `services.AddAgentTools(Assembly.GetExecutingAssembly())`). If the scan targets the wrong assembly, add `JobPortal.Application` to the scan.

### Step 7 — Rate-limit and credit-guard checklist

Confirm the new tool's execution path:
- [ ] `RedisTokenBucketLimiter` check runs **before** any LLM call — never after.
- [ ] `CreditDeductionInterceptor` deducts credits **after** a successful response — never on error or timeout.
- [ ] On LLM failure, the endpoint returns `AI_GENERATION_FAILED` (500 w/ code) with `creditsDeducted: 0`.
- [ ] On rate-limit hit, returns `AI_BUSY` (429) with `creditsDeducted: 0`.
- [ ] On credit exhaustion, returns `CREDIT_EXHAUSTED` (429) with `creditsDeducted: 0`.

### Step 8 — Update API contracts (if a new endpoint was added)

If you added a new controller action in Step 5, update [`15-api-contracts-backend.md`](15-api-contracts-backend.md) with the new endpoint's request and response shapes. Then sync [`../frontend/knowledge-base/14-api-contracts-frontend.md`](../frontend/knowledge-base/14-api-contracts-frontend.md) to match — both files must stay in sync per the project rule.

### Step 9 — Tick the checklist

Mark the corresponding item `[x]` in [`10-implementation-plan-backend-tracks.md`](10-implementation-plan-backend-tracks.md).

---

## Playbook 2 — Add a New Controller Endpoint

Use this when adding a new HTTP endpoint to an existing controller (not a new agent tool — for that, see Playbook 1).

### Step 1 — Confirm the endpoint is in the API contracts

Open [`15-api-contracts-backend.md`](15-api-contracts-backend.md). Find the endpoint. Confirm:
- HTTP method, route, auth requirement, request body shape, all possible response shapes and status codes.

> If the endpoint is not in the contracts file, do not invent it. Flag the gap and stop.

### Step 2 — Check the implementation plan

Open [`10-implementation-plan-backend-tracks.md`](10-implementation-plan-backend-tracks.md). Find the corresponding task. Note it — you will tick it `[x]` when done.

### Step 3 — Define request and response DTOs

Under `backend/src/JobPortal.Application/DTOs/<Domain>/`, create `<Action>Request.cs` and `<Action>Response.cs` matching the shapes in the contracts file exactly. Use data annotations for validation (`[Required]`, `[MaxLength]`, etc.).

```csharp
// Example
namespace JobPortal.Application.DTOs.Jobs;

public sealed record UpdateJobStatusRequest(
    [Required] string Status   // "Draft" | "Active" | "Closed" | "Expired"
);
```

### Step 4 — Add or extend the service method

Open the relevant service in `backend/src/JobPortal.Application/Services/<Domain>Service.cs`. Add a method that implements the business rule described in the KB. The service calls the repository interface (never EF Core directly):

```csharp
public async Task<UpdateJobStatusResponse> UpdateJobStatusAsync(
    Guid jobId, string status, Guid requestingUserId, CancellationToken ct)
{
    var job = await _jobRepository.GetByIdAsync(jobId, ct)
              ?? throw new NotFoundException(jobId);
    // ownership check, status transition rule, etc.
    await _jobRepository.UpdateAsync(job, ct);
    return new UpdateJobStatusResponse(job.Status.ToString());
}
```

### Step 5 — Add the controller action

Open the appropriate controller under `backend/src/JobPortal.Api/Controllers/`. Add the action. Keep it thin:

```csharp
[HttpPatch("{id}/status")]
[Authorize(Roles = "Recruiter")]
public async Task<IActionResult> UpdateStatus(
    Guid id, [FromBody] UpdateJobStatusRequest request, CancellationToken ct)
{
    var result = await _jobService.UpdateJobStatusAsync(id, request.Status, CurrentUserId, ct);
    return Ok(result);
}
```

Rules:
- Use `[Authorize(Roles = "...")]` — never bypass auth on a protected endpoint.
- Extract `CurrentUserId` from the JWT claims via a base-controller helper or `HttpContext.User`.
- No business logic in the controller — delegate entirely to the service.

### Step 6 — Verify the error envelope

The `GlobalExceptionMiddleware` handles unhandled exceptions. For **expected** errors (not found, forbidden, conflict), throw a domain exception that the middleware maps to the correct HTTP status and `{ error: { code, message } }` shape per [`15-api-contracts-backend.md`](15-api-contracts-backend.md) §2.

### Step 7 — Sync frontend API contracts (if this endpoint is consumed by the frontend)

Open [`../frontend/knowledge-base/14-api-contracts-frontend.md`](../frontend/knowledge-base/14-api-contracts-frontend.md) and add or update the consumer-facing description of the endpoint so the frontend KB stays in sync.

### Step 8 — Tick the checklist

Mark the corresponding item `[x]` in [`10-implementation-plan-backend-tracks.md`](10-implementation-plan-backend-tracks.md).

---

## Playbook 3 — Add an EF Core Migration (jsonb / pgvector Columns)

Use this when adding a new table, column, or index — including `jsonb` columns (screening questions, answers) and `vector(1536)` embedding columns.

> **PAUSE BEFORE RUNNING THIS PLAYBOOK AGAINST A LIVE DATABASE.** Running `dotnet ef database update` against a production or staging Supabase instance is irreversible. Confirm with the user before executing any `database update` command.

### Step 1 — Read the data model

Open [`07-data-model-and-security.md`](07-data-model-and-security.md) and the relevant entity file(s) under `JobPortal.Domain/Entities/`. Understand:
- The entity's properties and their types.
- Whether any column is `jsonb` (used for `screening_questions`, `answers`) or `vector(1536)` (used for all embedding tables).
- Foreign key constraints and ownership rules.

### Step 2 — Update the Domain entity

Open or create `backend/src/JobPortal.Domain/Entities/<Entity>.cs`. Add the new property. Use nullable reference types where the column is optional:

```csharp
// For a jsonb column (EF Core maps this via Npgsql JSON support):
public IList<ScreeningQuestion> ScreeningQuestions { get; set; } = [];

// For a pgvector column:
public Vector? Embedding { get; set; }   // Pgvector.EntityFrameworkCore.Vector
```

> The Domain layer must have **zero** EF Core dependencies. Import only `Pgvector` (the pure .NET type library) if needed for the vector property type — not `Npgsql.EntityFrameworkCore.PostgreSQL`.

### Step 3 — Configure EF Core mapping in `JobPortalDbContext`

Open `backend/src/JobPortal.Infrastructure/Persistence/JobPortalDbContext.cs`. In `OnModelCreating`, add the column configuration:

**jsonb column:**
```csharp
modelBuilder.Entity<Job>()
    .Property(j => j.ScreeningQuestions)
    .HasColumnType("jsonb")
    .HasConversion<string>();  // or use EF Core 8 JSON column mapping
```

**pgvector column (with HNSW index):**
```csharp
modelBuilder.Entity<AgentMemory>()
    .Property(m => m.Embedding)
    .HasColumnType("vector(1536)");

modelBuilder.Entity<AgentMemory>()
    .HasIndex(m => m.Embedding)
    .HasMethod("hnsw")
    .HasOperators("vector_cosine_ops");
```

All embedding columns use dimension `1536` and `vector_cosine_ops` — do not use a different dimension or operator class without explicit user confirmation.

### Step 4 — Generate the migration

Run from the `backend/` directory (confirm no live DB connection is active):

```powershell
dotnet ef migrations add <seq>_<PascalCaseDescription> `
  --project src/JobPortal.Infrastructure `
  --startup-project src/JobPortal.Api
```

Name the migration descriptively, e.g., `008_CreateAgentMemoriesTable`, `005_AddScreeningQuestionsToJobs`.

### Step 5 — Review the generated migration file

Open `backend/src/JobPortal.Infrastructure/Persistence/Migrations/<timestamp>_<Name>.cs`. Verify:
- `jsonb` columns are typed `jsonb`, not `text` or `json`.
- `vector` columns are typed `vector(1536)`.
- HNSW indexes appear in `Up()` with the correct operator class.
- The `Down()` method correctly reverses all changes.

If EF Core generated `text` instead of `jsonb`, manually edit the `HasColumnType` call and re-scaffold. Never leave a `jsonb` column mapped as `text` — the ATS scoring engine and screening form contract depend on PostgreSQL-level jsonb operators.

### Step 6 — Apply the migration (local dev only — confirm before any other environment)

```powershell
dotnet ef database update `
  --project src/JobPortal.Infrastructure `
  --startup-project src/JobPortal.Api
```

> **Do not run against staging or production without explicit user approval.** State what will change and why it cannot be reversed before asking.

### Step 7 — Verify the schema in Supabase (local)

Open the Supabase Table Editor or run `\d <table_name>` in psql. Confirm column types and that the HNSW index appears.

### Step 8 — Tick the checklist

Mark the corresponding item `[x]` in [`10-implementation-plan-backend-tracks.md`](10-implementation-plan-backend-tracks.md).
