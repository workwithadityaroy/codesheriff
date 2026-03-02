# 📐 Architecture Decision Records (ADR)

> This document captures all major architectural decisions made during CodeSheriff development.
> Every decision includes context, options considered, and the final choice.

---

## ADR-001 — Clean Architecture

**Date:** Phase 0  
**Status:** Accepted

**Context:** We need an architecture that is testable, scalable, and easy to reason about.

**Decision:** Use Clean Architecture with 4 layers:
- Domain (zero deps) → Application → Infrastructure → API

**Why:** Dependency always points inward. Business logic never depends on frameworks or databases.

---

## ADR-002 — CQRS with MediatR

**Date:** Phase 0  
**Status:** Accepted

**Context:** Mixed read/write operations in controllers become hard to maintain.

**Decision:** Separate Commands (write) from Queries (read) using MediatR.

**Why:** Each operation is self-contained. Easy to add cross-cutting concerns (logging, validation) via pipeline behaviors.

---

## ADR-003 — Result Pattern over Exceptions

**Date:** Phase 0  
**Status:** Accepted

**Context:** Using exceptions for business logic (e.g., "user not found") is expensive and hides flow.

**Decision:** Use a `Result<T>` type for all business operations.

```csharp
// Instead of throwing
public Result<Review> GetReview(Guid id)
{
    if (review is null) return Result.Failure("Review not found");
    return Result.Success(review);
}
```

**Why:** Explicit error handling. No hidden exceptions. Better performance.

---

## ADR-004 — PostgreSQL via Supabase

**Date:** Phase 0  
**Status:** Accepted

**Context:** Need a reliable, scalable database with free tier for portfolio demo.

**Decision:** PostgreSQL hosted on Supabase.

**Why:** Free tier is generous. Real PostgreSQL (not a mock). Built-in auth, real-time, and storage if needed later.

---

## ADR-005 — Upstash Redis for Queue

**Date:** Phase 0  
**Status:** Accepted

**Context:** AI reviews are slow (3-10 seconds). Cannot block the webhook response.

**Decision:** Use Upstash Redis Queue + .NET BackgroundService worker.

**Why:** Serverless-friendly. Free tier. Webhook responds instantly, AI review happens async.

---

## ADR-006 — Clerk for Authentication

**Date:** Phase 0  
**Status:** Accepted

**Context:** Building auth from scratch is complex and risky for a portfolio project.

**Decision:** Use Clerk for auth, connected natively to Supabase.

**Why:** GitHub OAuth built-in. Organization management built-in. Free tier sufficient. Native Supabase integration (post-April 2025).

---

## ADR-007 — .NET 9 over .NET 8 LTS

**Date:** Phase 0  
**Status:** Accepted

**Context:** .NET 8 is LTS (until Nov 2026), .NET 9 is STS (18 months).

**Decision:** Use .NET 9 for this portfolio project.

**Why:** Portfolio projects benefit from showing latest tech. Interviewers notice. .NET 9 has improved Minimal APIs, better OpenAPI support, and performance gains.

---

## ADR-008 — Monorepo Structure

**Date:** Phase 0  
**Status:** Accepted

**Context:** Frontend and backend are separate deployments but closely related.

**Decision:** Single git repo with `/frontend` and `/backend` folders.

**Why:** Atomic commits across both. Single CI/CD config. Easier for portfolio review — one repo, full picture.

---

## ADR-009 — Vercel + Railway Hosting

**Date:** Phase 0  
**Status:** Accepted

**Context:** Need free hosting for portfolio demo.

**Decision:** Vercel for Next.js frontend, Railway for .NET 9 backend.

**Why:** Both have free tiers. Vercel has first-class Next.js support. Railway supports Docker and .NET natively.

---

## ADR-010 — Resend for Emails

**Date:** Phase 0  
**Status:** Accepted

**Context:** Need transactional email for weekly reports.

**Decision:** Use Resend API.

**Why:** 3,000 free emails/month. Developer-friendly API. React Email templates supported.
