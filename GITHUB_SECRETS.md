# 🔐 GitHub Secrets Setup Guide

> Go to: GitHub Repo → Settings → Secrets and Variables → Actions → New Repository Secret

---

## Required Secrets

### Frontend (Clerk)
| Secret Name | Where to Get |
|---|---|
| `NEXT_PUBLIC_CLERK_PUBLISHABLE_KEY` | Clerk Dashboard → API Keys |
| `CLERK_SECRET_KEY` | Clerk Dashboard → API Keys |
| `NEXT_PUBLIC_API_URL` | Your Railway backend URL |

### Frontend (Vercel Deploy)
| Secret Name | Where to Get |
|---|---|
| `VERCEL_TOKEN` | Vercel → Account Settings → Tokens |
| `VERCEL_ORG_ID` | Vercel → Project Settings |
| `VERCEL_PROJECT_ID` | Vercel → Project Settings |

### Backend (Railway Deploy)
| Secret Name | Where to Get |
|---|---|
| `RAILWAY_TOKEN` | Railway → Account Settings → Tokens |

### Monitoring
| Secret Name | Where to Get |
|---|---|
| `CODECOV_TOKEN` | codecov.io after linking your repo |

---

## Branch Protection Rules (GitHub)

Go to: Repo → Settings → Branches → Add Rule

### For `main` branch:
- ✅ Require a pull request before merging
- ✅ Require approvals: 1
- ✅ Require status checks to pass before merging
  - Select: `Frontend — Build & Lint`
  - Select: `Backend — Build & Test`
- ✅ Require branches to be up to date before merging
- ✅ Do not allow bypassing the above settings

### For `dev` branch:
- ✅ Require status checks to pass before merging
  - Select: `Frontend — Build & Lint`
  - Select: `Backend — Build & Test`
