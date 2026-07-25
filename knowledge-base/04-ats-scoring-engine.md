# ATS Match Scoring Engine (Backend)

**Derived from:** SRS v3.1 §5 (ATS Match Scoring Engine)

## 1. Purpose

The ATS Scoring Engine calculates a fit percentage by evaluating candidate profile data and extracted resume text against the Job Description (JD) and requirements. This is a **pure backend computation** — implement it as `AtsScoringService`, computed on-demand (optionally cacheable on `Applications`, but not required).

## 2. Weight Distribution

| Evaluation Pillar | Standard Weight (JD Has Certs) | Adjusted Weight (JD Has NO Certs) | Evaluation Criteria |
|---|---|---|---|
| Skills Coverage | 40% | 55% | Percentage of required/preferred JD skills present in candidate profile & resume |
| Experience Fit | 25% | 30% | Total years of experience vs. minimum experience requirement in JD |
| Certification Match | 20% | 0% (Excluded) | Explicit check: if JD mentions a certification (e.g., AWS, PMP, CKAD) and candidate profile contains the exact/equivalent certification |
| Domain/Title Match | 15% | 15% | Semantic alignment between candidate's recent job title/industry and JD title |

## 3. Certification Matching Rules

- **Explicit JD Mention:** If a certification is listed in the JD requirements, check the Job Seeker's Certifications profile list and extracted resume text.
- **Positive Match:** If a matching certificate is found, Certification Match score = 100%.
- **Missing Certificate:** If the JD lists required certifications but the user has not added them, Certification Match score = 0%, impacting the overall ATS score.
- **Dynamic Re-weighting:** If the JD contains no certification requirements, certification weight drops to 0%, and its weight is dynamically redistributed to Skills Coverage (+15%) and Experience (+5%), ensuring candidates are not penalized for unrequested certifications. Return `certificationWeightRedistributed: true` in this case so the frontend can reflect it.
- **Optional Profile Fields:** Optional fields like Expected Salary are excluded from ATS calculations unless configured as a mandatory screening question in `screening_questions`.

## 4. Domain/Title Match

Semantic alignment between the candidate's recent job title/industry and the JD title — implement via a lightweight LLM call or embedding similarity (the same embedding infrastructure used for `JobEmbeddings`/`ProfileEmbeddings`, see [05-agentic-ai-orchestrator-and-tools.md](05-agentic-ai-orchestrator-and-tools.md), can be reused here rather than building a separate similarity mechanism).

## 5. Response Shape

`GET /api/jobs/{id}/match-score` (Job Seeker, authenticated):

```json
{
  "jobId": "j1...",
  "overallScore": 78,
  "breakdown": {
    "skillsCoverage": { "weight": 40, "score": 85 },
    "experienceFit": { "weight": 25, "score": 90 },
    "certificationMatch": { "weight": 20, "score": 0 },
    "domainTitleMatch": { "weight": 15, "score": 70 }
  },
  "certificationWeightRedistributed": false
}
```

This same scoring service is reused internally by the Job Search & Match Agent (tool) and the Bulk Apply Agent's ATS ≥ 80% filter — do not duplicate the scoring logic per caller.

## Implementation Checklist (Backend)

- [ ] Implement `AtsScoringService` combining candidate profile + resume text vs. JD requirements
- [ ] Implement standard weight distribution (Skills 40% / Experience 25% / Certification 20% / Domain-Title 15%)
- [ ] Implement adjusted weight distribution when JD has no certification requirements (Skills 55% / Experience 30% / Certification 0% / Domain-Title 15%)
- [ ] Implement Skills Coverage evaluation against JD required/preferred skills
- [ ] Implement Experience Fit evaluation (candidate years vs. JD minimum)
- [ ] Implement Certification Match logic (explicit JD mention check, profile + resume text scan, 100%/0% outcomes)
- [ ] Implement dynamic re-weighting logic when JD lacks certification requirements, returning `certificationWeightRedistributed: true`
- [ ] Implement Domain/Title Match semantic alignment check (embedding similarity or lightweight LLM call)
- [ ] Exclude optional profile fields (e.g., Expected Salary) from ATS scoring unless present as a mandatory screening question
- [ ] Expose `GET /api/jobs/{id}/match-score`; ensure the Job Search & Match Agent and Bulk Apply Agent both call the same `AtsScoringService` instance/method rather than reimplementing scoring
- [ ] Unit test: cert-present vs. cert-absent weight redistribution across 3+ (user, job) pairs

## Integration Points (What the Frontend Consumes)

- `GET /api/jobs/{id}/match-score` — see [15-api-contracts-backend.md](15-api-contracts-backend.md) for the exact response shape the frontend renders as a badge + breakdown tooltip.
