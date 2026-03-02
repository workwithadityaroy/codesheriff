# 🔍 CodeSheriff

> AI-Powered Code Review & Tech Debt Analyzer for .NET Projects

[![CI Dev](https://github.com/YOUR_USERNAME/codesheriff/actions/workflows/ci-dev.yml/badge.svg)](https://github.com/YOUR_USERNAME/codesheriff/actions/workflows/ci-dev.yml)
[![CI Prod](https://github.com/YOUR_USERNAME/codesheriff/actions/workflows/ci-prod.yml/badge.svg)](https://github.com/YOUR_USERNAME/codesheriff/actions/workflows/ci-prod.yml)

---

## 📌 What is CodeSheriff?

CodeSheriff automatically reviews your code when a Pull Request is opened. It flags security issues, performance bottlenecks, code smells, and gives a **Tech Debt Score** per review. Team leads get a **weekly report** with trends over time.

**Real problem solved:** Companies lose millions due to unreviewed code, hidden tech debt, and security vulnerabilities sitting in repos. Junior devs get no mentorship. Senior devs waste time on manual reviews.

---

## 🏗️ Architecture

```
┌─────────────────────────────────────────────┐
│         Next.js 16 Frontend (Vercel)        │
└─────────────────┬───────────────────────────┘
                  │ HTTPS/REST
┌─────────────────▼───────────────────────────┐
│      ASP.NET Core 9 API (Railway)           │
│   Clean Architecture + CQRS + MediatR       │
└─────────────────┬───────────────────────────┘
                  │
       ┌──────────┴──────────┐
       ▼                     ▼
┌─────────────┐     ┌─────────────────┐
│  Supabase   │     │  Upstash Redis  │
│ (PostgreSQL)│     │    (Queue)      │
└─────────────┘     └────────┬────────┘
                             │
                    ┌────────▼────────┐
                    │  Queue Worker   │
                    │ (Background Svc)│
                    └────────┬────────┘
                             │
                    ┌────────▼────────┐
                    │   Claude AI     │
                    │  (Anthropic)    │
                    └─────────────────┘
```

---

## 🛠️ Tech Stack

| Layer | Technology | Version |
|---|---|---|
| Frontend | Next.js | 16.1 |
| Language | TypeScript | 5.x |
| Styling | Tailwind CSS | v4 |
| UI Components | shadcn/ui | Latest |
| Auth | Clerk | Latest |
| Backend | ASP.NET Core | .NET 9 |
| ORM | Entity Framework Core | 9.x |
| CQRS | MediatR | 12.x |
| Validation | FluentValidation | Latest |
| Logging | Serilog | Latest |
| Database | PostgreSQL (Supabase) | 15+ |
| Queue | Upstash Redis | Latest |
| AI Engine | Claude API (Anthropic) | Sonnet |
| Email | Resend | Latest |
| Monitoring | Sentry | Latest |
| Frontend Hosting | Vercel | - |
| Backend Hosting | Railway | - |
| CI/CD | GitHub Actions | - |

---

## 🌿 Git Branch Strategy

```
main (PROD)   ──────────────────────────► Stable. Only merged from dev via PR.
     │
     └── dev  ──────────────────────────► All development. Feature branches merge here.
                    │
                    ├── feature/auth-setup
                    ├── feature/github-webhook
                    ├── feature/ai-review-engine
                    ├── feature/dashboard-ui
                    └── feature/email-reports
```

### Commit Message Convention
```
feat:     New feature
fix:      Bug fix
chore:    Config, tooling, dependencies
docs:     Documentation only
refactor: Code change, no feature/fix
test:     Adding/updating tests
ci:       CI/CD changes
```

---

## 🗂️ Project Structure

```
codesheriff/
├── .github/
│   └── workflows/
│       ├── ci-dev.yml          ← Runs on every push to dev
│       └── ci-prod.yml         ← Runs on PR to main
├── frontend/                   ← Next.js 16 App
│   ├── app/
│   │   ├── auth/
│   │   │   ├── sign-in/
│   │   │   └── sign-up/
│   │   ├── dashboard/
│   │   ├── repos/
│   │   ├── reviews/
│   │   └── reports/
│   ├── components/
│   │   ├── ui/                 ← shadcn components
│   │   └── custom/             ← our components
│   ├── hooks/
│   ├── lib/
│   └── types/
├── backend/
│   ├── CodeSheriff.API/        ← Entry point, controllers, middleware
│   ├── CodeSheriff.Application/← CQRS handlers, commands, queries
│   ├── CodeSheriff.Domain/     ← Entities, interfaces (zero deps)
│   ├── CodeSheriff.Infrastructure/ ← DB, external APIs, queue
│   └── CodeSheriff.Tests/      ← Unit + Integration tests
├── docs/                       ← Architecture diagrams, decisions
├── docker-compose.yml          ← Local dev setup
├── .gitignore
└── README.md
```

---

## 🚀 Getting Started (Local Dev)

### Prerequisites
- Node.js 20+
- .NET 9 SDK
- Docker Desktop
- GitHub Account
- Clerk Account
- Supabase Account
- Upstash Account

### 1. Clone & Setup
```bash
git clone https://github.com/YOUR_USERNAME/codesheriff.git
cd codesheriff
git checkout dev
```

### 2. Start Infrastructure
```bash
docker-compose up -d
```

### 3. Backend
```bash
cd backend/CodeSheriff.API
cp appsettings.example.json appsettings.Development.json
# Fill in your env vars
dotnet restore
dotnet ef database update
dotnet run
```

### 4. Frontend
```bash
cd frontend
cp .env.example .env.local
# Fill in your env vars
npm install
npm run dev
```

---

## 📅 Build Phases

| Phase | What | Status |
|---|---|---|
| **Phase 0** | Repo setup, structure, CI/CD | ✅ Done |
| **Phase 1** | Backend foundation, DB, migrations | 🔄 Next |
| **Phase 2** | Auth + GitHub integration | ⏳ Planned |
| **Phase 3** | AI Review Engine | ⏳ Planned |
| **Phase 4** | Frontend Dashboard | ⏳ Planned |
| **Phase 5** | Reports + Polish + Deploy | ⏳ Planned |

---

## 📐 Design Patterns Used

| Pattern | Layer | Purpose |
|---|---|---|
| Clean Architecture | All | Separation of concerns |
| CQRS | Application | Commands vs Queries |
| Repository | Infrastructure | Abstract data access |
| Mediator (MediatR) | Application | Decoupled handlers |
| Factory | AI Engine | Build review strategies |
| Strategy | Review Rules | Swap logic per language |
| Observer | Webhooks | React to GitHub events |
| Result Pattern | All | No exceptions for business logic |
| Decorator | Middleware | Auth, logging, rate limiting |

---

## 🔐 Environment Variables

### Backend (`appsettings.Development.json`)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "your-supabase-connection-string"
  },
  "Clerk": {
    "SecretKey": "sk_test_...",
    "PublishableKey": "pk_test_..."
  },
  "GitHub": {
    "WebhookSecret": "your-webhook-secret",
    "AppId": "your-github-app-id",
    "PrivateKey": "your-private-key"
  },
  "Anthropic": {
    "ApiKey": "sk-ant-..."
  },
  "Upstash": {
    "RedisUrl": "https://...",
    "RedisToken": "..."
  },
  "Resend": {
    "ApiKey": "re_..."
  }
}
```

### Frontend (`.env.local`)
```env
NEXT_PUBLIC_CLERK_PUBLISHABLE_KEY=pk_test_...
CLERK_SECRET_KEY=sk_test_...
NEXT_PUBLIC_CLERK_SIGN_IN_URL=/auth/sign-in
NEXT_PUBLIC_CLERK_SIGN_UP_URL=/auth/sign-up
NEXT_PUBLIC_CLERK_AFTER_SIGN_IN_URL=/dashboard
NEXT_PUBLIC_CLERK_AFTER_SIGN_UP_URL=/dashboard
NEXT_PUBLIC_API_URL=http://localhost:5000
```

---

## 👨‍💻 Built By

**NEGAN** — .NET Full Stack Developer  
*Portfolio project showcasing enterprise-grade architecture*

---

## 📄 License

MIT License — feel free to learn from it, don't copy it for interviews 😄
