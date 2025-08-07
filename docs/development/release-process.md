# Release Process

OpenCoreMMO implements a streamlined, automated release process that ensures quality, reliability, and rapid delivery through CI/CD automation and comprehensive testing.

## 🎯 Overview

Our release process emphasizes automation, quality gates, and rapid feedback loops to deliver stable software with minimal manual intervention.

```mermaid
graph TD
    A[Development] --> B[Feature Branch]
    B --> C[Pull Request]
    C --> D[Code Review]
    D --> E[Merge to Develop]
    E --> F[Integration Tests]
    F --> G[Release Branch]
    G --> H[Release Testing]
    H --> I[Merge to Main]
    I --> J[Automated Release]
    J --> K[Production Deploy]
    
    style J fill:#e8f5e8
    style K fill:#e3f2fd
```

## 🚀 Release Types & Cadence

### Release Schedule

| Type | Frequency | Purpose | Scope |
|------|-----------|---------|-------|
| **Major** | Quarterly | Breaking changes, major features | 🏗️ **Architectural** |
| **Minor** | Monthly | New features, enhancements | ✨ **Feature** |
| **Patch** | As needed | Bug fixes, security updates | 🔧 **Maintenance** |
| **Hotfix** | Emergency | Critical production issues | 🚨 **Emergency** |

### Release Timeline

```mermaid
gantt
    title Monthly Release Cycle
    dateFormat  YYYY-MM-DD
    section Development
    Feature Development    :dev1, 2025-01-01, 21d
    Code Review & Testing  :test1, after dev1, 5d
    
    section Release Prep
    Release Branch Created :rel1, after test1, 1d
    Release Testing       :test2, after rel1, 2d
    Documentation Update  :doc1, after rel1, 2d
    
    section Deployment
    Production Release    :prod1, after test2, 1d
    Post-Release Monitor  :mon1, after prod1, 3d
```

## 📋 Release Preparation

### Pre-Release Checklist

```mermaid
graph TD
    A[Release Planning] --> B{Feature Complete?}
    B -->|No| C[Continue Development]
    B -->|Yes| D[Create Release Branch]
    
    D --> E[Version Bump]
    E --> F[Update Changelog]
    F --> G[Update Documentation]
    G --> H[Run Full Test Suite]
    H --> I{All Tests Pass?}
    I -->|No| J[Fix Issues]
    I -->|Yes| K[Tag Release]
    
    J --> H
    K --> L[Deploy to Staging]
    L --> M[Staging Validation]
    M --> N[Production Release]
```

### Automated Release Commands

```bash
# Create release from develop branch
git checkout develop
git pull origin develop

# Generate release (automatically determines version)
npm run release

# Or create specific release types
npm run release -- --release-as major
npm run release -- --release-as minor
npm run release -- --release-as patch

# Preview release changes
npm run release:dry
```

## 🏗️ Release Branch Workflow

### Creating Release Branches

```bash
# Automated via npm script
npm run release

# Manual process
git checkout develop
git checkout -b release/v1.2.0
```

### Release Branch Rules

- **Source**: Always created from `develop` branch
- **Naming**: `release/v{version}` (e.g., `release/v1.2.0`)
- **Purpose**: Final testing and bug fixes only
- **Lifetime**: Temporary, deleted after merge
- **Protection**: Same as develop branch

## 🧪 Testing & Quality Gates

### Automated Testing Pipeline

```mermaid
sequenceDiagram
    participant D as Developer
    participant CI as CI Pipeline
    participant T as Test Suite
    participant S as Security Scan
    participant Q as Quality Gate
    
    D->>CI: Push to release branch
    CI->>T: Run unit tests
    T->>CI: Results
    CI->>T: Run integration tests
    T->>CI: Results
    CI->>S: Security vulnerability scan
    S->>CI: Scan results
    CI->>Q: Quality metrics check
    Q->>CI: Gate status
    CI->>D: Pipeline results
```

### Quality Gates

| Gate | Criteria | Blocking | Action on Failure |
|------|----------|----------|-------------------|
| **Unit Tests** | 100% pass rate | ✅ Yes | Fix failing tests |
| **Integration Tests** | 100% pass rate | ✅ Yes | Fix integration issues |
| **Code Coverage** | >80% coverage | ✅ Yes | Add missing tests |
| **Security Scan** | No high/critical vulns | ✅ Yes | Address vulnerabilities |
| **Performance** | No regression >10% | ⚠️ Warning | Investigate performance |
| **Documentation** | All endpoints documented | ⚠️ Warning | Update documentation |

## 📦 Release Artifacts

### Generated Artifacts

```mermaid
graph TD
    A[Release Creation] --> B[Git Tag]
    A --> C[GitHub Release]
    A --> D[Docker Images]
    A --> E[Changelog Update]
    A --> F[Documentation]
    
    B --> G[Version Reference]
    C --> H[Release Notes]
    D --> I[Container Registry]
    E --> J[Change History]
    F --> K[API Docs]
```

### Artifact Details

| Artifact | Location | Purpose |
|----------|----------|---------|
| **Git Tag** | Repository | Version reference point |
| **Release Notes** | GitHub Releases | User-facing changes |
| **Docker Images** | Container Registry | Deployment artifacts |
| **Changelog** | CHANGELOG.md | Detailed change history |
| **API Documentation** | docs/api/ | Technical reference |

## 🚀 Deployment Process

### Staging Deployment

```bash
# Automated staging deployment
git push origin release/v1.2.0
# Triggers CI/CD pipeline to staging environment
```

### Production Deployment

```mermaid
graph TD
    A[Release Branch Ready] --> B[Merge to Main]
    B --> C[Automated Tag Creation]
    C --> D[Build Production Images]
    D --> E[Deploy to Production]
    E --> F[Health Checks]
    F --> G{Deployment Healthy?}
    G -->|Yes| H[Release Complete]
    G -->|No| I[Automatic Rollback]
    I --> J[Alert Team]
```

### Deployment Commands

```bash
# Manual production deployment
git checkout main
git merge release/v1.2.0
git push origin main
# Automated pipeline deploys to production

# Emergency rollback
git checkout main
git revert HEAD
git push origin main
```

## 📊 Release Validation

### Health Checks

```mermaid
graph TD
    A[Deployment Complete] --> B[System Health Check]
    B --> C[API Endpoint Validation]
    C --> D[Database Connectivity]
    D --> E[External Service Integration]
    E --> F[Performance Baseline]
    F --> G{All Checks Pass?}
    G -->|Yes| H[Release Validated]
    G -->|No| I[Initiate Rollback]
```

### Monitoring & Alerts

Post-deployment monitoring includes:

- **Application Health**: Response times, error rates
- **Infrastructure**: CPU, memory, disk usage
- **Business Metrics**: User activity, feature adoption
- **Security**: Unusual access patterns, vulnerabilities

## 🔄 Post-Release Process

### Release Retrospective

```mermaid
timeline
    title Post-Release Activities
    
    section Immediate (0-2 hours)
        Health Monitoring   : Monitor system metrics
        User Feedback      : Track support channels
        
    section Short-term (2-24 hours)
        Performance Analysis : Compare to baseline
        Bug Reports         : Monitor issue tracker
        
    section Long-term (1-7 days)
        Adoption Metrics    : Feature usage analytics
        Retrospective      : Team improvement discussion
```

### Cleanup Tasks

```bash
# Delete release branch
git branch -d release/v1.2.0
git push origin --delete release/v1.2.0

# Merge back to develop
git checkout develop
git merge main
git push origin develop

# Update project board
# Move completed issues to "Done"
```

## 🚨 Emergency Release Process

### Hotfix Workflow

```mermaid
graph TD
    A[Critical Issue Identified] --> B[Create Hotfix Branch]
    B --> C[Implement Fix]
    C --> D[Emergency Testing]
    D --> E[Fast-track Review]
    E --> F[Merge to Main]
    F --> G[Deploy Immediately]
    G --> H[Monitor Closely]
    H --> I[Merge to Develop]
```

### Hotfix Commands

```bash
# Create hotfix from main
git checkout main
git checkout -b hotfix/critical-security-fix

# Implement and test fix
# ... make changes ...
git add .
git commit -m "fix: resolve critical security vulnerability"

# Fast-track release
npm run release
git checkout main
git merge hotfix/critical-security-fix
git push origin main
```

## 📈 Release Metrics

### Key Performance Indicators

| Metric | Target | Measurement |
|--------|--------|-------------|
| **Lead Time** | <2 weeks | Feature to production |
| **Deployment Frequency** | Monthly | Regular releases |
| **Mean Time to Recovery** | <1 hour | Incident resolution |
| **Change Failure Rate** | <5% | Failed deployments |

### Release Dashboard

```mermaid
graph TD
    A[Release Metrics] --> B[Deployment Success Rate]
    A --> C[Release Frequency]
    A --> D[Rollback Rate]
    A --> E[Time to Market]
    
    B --> F[Process Optimization]
    C --> F
    D --> F
    E --> F
```

## 🛠️ Tools & Automation

### Automated Commit System

Our release process is powered by automatic commit validation and changelog generation:

```mermaid
graph LR
    A[Git Commit] --> B[Commitlint Validation]
    B --> C{Valid Format?}
    C -->|Yes| D[Commit Success]
    C -->|No| E[Show Error & Block]
    D --> F[Auto Changelog Update]
    F --> G[Ready for Release]
    
    style D fill:#e8f5e8
    style F fill:#e3f2fd
    style E fill:#ffebee
```

**✨ Key Features:**
- **Zero Configuration**: Just use `git commit` normally
- **Automatic Validation**: Ensures conventional commit format
- **No Size Limits**: Write detailed commit messages
- **Auto Changelog**: Updates automatically on valid commits
- **Release Ready**: `npm run release` generates versions and tags

### Release Toolchain

| Tool | Purpose | Configuration |
|------|---------|---------------|
| **commitlint** | Commit message validation | `commitlint.config.js` |
| **standard-version** | Version bumping, changelog | `.versionrc.json` |
| **Husky** | Git hooks validation | `.husky/` |
| **GitHub Actions** | CI/CD pipeline | `.github/workflows/` |
| **Docker** | Containerization | `Dockerfile` |
| **Terraform** | Infrastructure | `infrastructure/` |

### Configuration Files

```
server/
├── .versionrc.json          # standard-version config
├── commitlint.config.js     # Commit validation
├── .github/
│   └── workflows/
│       ├── release.yml      # Release pipeline
│       └── deploy.yml       # Deployment pipeline
└── scripts/
    ├── release.sh           # Release automation
    └── deploy.sh            # Deployment scripts
```

## 🎓 Best Practices

### For Release Managers

1. **Automate Everything**: Minimize manual steps
2. **Test Thoroughly**: Never skip quality gates
3. **Communicate Clearly**: Keep stakeholders informed
4. **Monitor Actively**: Watch post-deployment metrics
5. **Learn Continuously**: Improve process based on feedback

### For Developers

1. **Follow Conventions**: Use conventional commits
2. **Test Locally**: Ensure tests pass before pushing
3. **Document Changes**: Update relevant documentation
4. **Monitor Deployments**: Watch your features in production

---

> 🎯 **Success Criteria**: Reliable, predictable releases that deliver value to users while maintaining system stability and team confidence.
