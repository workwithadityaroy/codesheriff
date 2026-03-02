#!/bin/bash

# ================================================
# CodeSheriff — Git Repository Setup Script
# Run this ONCE after creating your GitHub repo
# ================================================

set -e

echo "🔍 CodeSheriff — Git Setup"
echo "================================"

# Check if git is installed
if ! command -v git &> /dev/null; then
    echo "❌ Git is not installed. Please install git first."
    exit 1
fi

# Step 1: Initialize git
echo ""
echo "📁 Step 1: Initializing git repository..."
git init
git add .
git commit -m "chore: initial project scaffold — Phase 0"

echo "✅ Initial commit done"

# Step 2: Create main branch (PROD)
echo ""
echo "🌿 Step 2: Setting up MAIN branch (PROD)..."
git branch -M main

echo "✅ main branch ready"

# Step 3: Create dev branch (DEV)
echo ""
echo "🌿 Step 3: Creating DEV branch..."
git checkout -b dev
git commit --allow-empty -m "chore: dev branch initialized"

echo "✅ dev branch ready"

# Step 4: Prompt for remote
echo ""
echo "🔗 Step 4: Connect to GitHub"
echo "-----------------------------------"
echo "Go to GitHub and create a new repository named 'codesheriff'"
echo "Then run these commands:"
echo ""
echo "  git remote add origin https://github.com/YOUR_USERNAME/codesheriff.git"
echo "  git push -u origin main"
echo "  git push -u origin dev"
echo ""
echo "Then in GitHub Settings → Branches:"
echo "  • Set 'main' as default branch"
echo "  • Add branch protection rule for 'main':"
echo "    ✅ Require pull request before merging"
echo "    ✅ Require status checks to pass (CI)"
echo "    ✅ Require branches to be up to date"
echo "    ✅ Do not allow bypassing above settings"
echo ""
echo "================================"
echo "✅ Phase 0 Git Setup Complete!"
echo "================================"
echo ""
echo "📋 Branch Summary:"
echo "  main → PRODUCTION (protected, PR only)"
echo "  dev  → DEVELOPMENT (all features branch from here)"
echo ""
echo "📋 Next Steps:"
echo "  1. Create GitHub repo"
echo "  2. Add GitHub Secrets (Vercel token, Railway token, Clerk keys)"
echo "  3. Run 'Phase 1 Let's Go'"
