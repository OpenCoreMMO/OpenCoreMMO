# Versioning Strategy

OpenCoreMMO follows **Semantic Versioning (SemVer)** principles with automated release management to ensure predictable, reliable software delivery.

## 🎯 Overview

Our versioning strategy balances predictability for users with flexibility for rapid development, using automation to maintain consistency and reduce manual overhead.

```mermaid
graph TD
    A[Development] --> B[Conventional Commits]
    B --> C[Automated Analysis]
    C --> D{Change Type}
    D -->|feat| E[Minor Version]
    D -->|fix| F[Patch Version]
    D -->|BREAKING CHANGE| G[Major Version]
    E --> H[Release Generation]
    F --> H
    G --> H
    H --> I[Automated Changelog]
    I --> J[Git Tag & Deploy]
```

## 📊 Semantic Versioning Structure

### Version Format: `MAJOR.MINOR.PATCH`

```
v2.1.3
│ │ │
│ │ └─ PATCH: Bug fixes, security patches
│ └─── MINOR: New features, backward compatible
└───── MAJOR: Breaking changes, API changes
```

### Version Components

| Component | When to Increment | Examples | Impact |
|-----------|------------------|----------|---------|
| **MAJOR** | Breaking changes | API changes, removed features | ⚠️ **Breaking** |
| **MINOR** | New features | New endpoints, enhanced functionality | ✅ **Compatible** |
| **PATCH** | Bug fixes | Security fixes, performance improvements | 🔧 **Safe** |

## 🚀 Release Types

### Stable Releases

```mermaid
timeline
    title Release Timeline
    
    section Major Release
        v2.0.0 : New architecture
               : Breaking API changes
               : Major feature overhaul
    
    section Minor Releases
        v2.1.0 : Guild system
        v2.2.0 : Trading system
        v2.3.0 : PvP enhancements
    
    section Patch Releases
        v2.3.1 : Security fixes
        v2.3.2 : Performance improvements
        v2.3.3 : Bug fixes
```

### Pre-release Versions

| Type | Format | Purpose | Stability |
|------|--------|---------|-----------|
| **Alpha** | `v2.1.0-alpha.1` | Early development, internal testing | 🔴 **Unstable** |
| **Beta** | `v2.1.0-beta.1` | Feature complete, community testing | 🟡 **Testing** |
| **RC** | `v2.1.0-rc.1` | Release candidate, final validation | 🟢 **Stable** |

## 🤖 Automated Version Bumping

### Commit-Based Analysis

Our automation analyzes conventional commits to determine version increments:

```mermaid
graph LR
    A[Commits Analysis] --> B{Commit Types}
    B -->|feat:| C[Minor Bump]
    B -->|fix:| D[Patch Bump]
    B -->|BREAKING CHANGE:| E[Major Bump]
    B -->|docs:, style:, test:| F[No Bump]
    
    C --> G[Generate Release]
    D --> G
    E --> G
```

### Version Calculation Rules

```javascript
// Simplified version bump logic
function calculateVersion(commits, currentVersion) {
  let hasMajor = commits.some(c => c.breaking);
  let hasMinor = commits.some(c => c.type === 'feat');
  let hasPatch = commits.some(c => c.type === 'fix');
  
  if (hasMajor) return bumpMajor(currentVersion);
  if (hasMinor) return bumpMinor(currentVersion);
  if (hasPatch) return bumpPatch(currentVersion);
  
  return currentVersion; // No release needed
}
```

## 📋 Release Process

### Automated Release Workflow

```mermaid
sequenceDiagram
    participant D as Developer
    participant CI as CI/CD
    participant R as Repository
    participant P as Production
    
    D->>R: Push to main branch
    R->>CI: Trigger release pipeline
    CI->>CI: Analyze commits since last tag
    CI->>CI: Calculate new version
    CI->>CI: Generate changelog
    CI->>R: Create tag and release
    CI->>P: Deploy to production
    R->>D: Notify release created
```

### Manual Release Commands

```bash
# Generate standard release
npm run release

# Generate first release (no version bump)
npm run release:first

# Dry run (preview changes)
npm run release:dry

# Pre-release versions
npm run release -- --prerelease alpha
npm run release -- --prerelease beta
npm run release -- --prerelease rc
```

## 🏷️ Tagging Strategy

### Tag Naming Convention

```
v1.0.0          # Stable release
v1.1.0-alpha.1  # Alpha pre-release
v1.1.0-beta.2   # Beta pre-release
v1.1.0-rc.1     # Release candidate
```

### Tag Metadata

```bash
# Lightweight tag (automated)
git tag v1.2.0

# Annotated tag with metadata
git tag -a v1.2.0 -m "Release v1.2.0: Guild System Implementation

Features:
- Complete guild management system
- Guild wars and alliances
- Enhanced player progression

Bug Fixes:
- Resolved memory leak in combat system
- Fixed database connection issues

BREAKING CHANGES:
- Player API endpoint structure updated
"
```

## 📈 Version History & Tracking

### Release Artifacts

Each release generates:

1. **Git Tag**: Permanent reference point
2. **GitHub Release**: Release notes and assets
3. **Changelog Entry**: Detailed change documentation
4. **Docker Images**: Tagged container images
5. **Documentation**: Version-specific docs

### Change Documentation

```mermaid
graph TD
    A[Conventional Commits] --> B[standard-version]
    B --> C[Parse Commit History]
    C --> D[Group by Type]
    D --> E[Generate Sections]
    E --> F[Format Changelog]
    F --> G[Update CHANGELOG.md]
    
    style F fill:#e8f5e8
```

## 🔄 Backward Compatibility

### Compatibility Matrix

| Change Type | Version Impact | Backward Compatible | Migration Required |
|-------------|----------------|--------------------|--------------------|
| **Bug Fix** | PATCH | ✅ Yes | ❌ No |
| **New Feature** | MINOR | ✅ Yes | ❌ No |
| **Deprecation** | MINOR | ⚠️ Warning | 🟡 Eventually |
| **Breaking Change** | MAJOR | ❌ No | ✅ Yes |

### Deprecation Policy

```mermaid
timeline
    title Feature Deprecation Timeline
    
    section Warning Phase
        v2.1.0 : Feature marked deprecated
               : Warning messages added
               : Documentation updated
    
    section Transition Phase
        v2.2.0 : Alternative provided
        v2.3.0 : Migration tools available
    
    section Removal Phase
        v3.0.0 : Deprecated feature removed
               : Breaking change documented
```

## 🚨 Emergency Releases

### Hotfix Versioning

```bash
# Emergency patch release
git checkout main
git checkout -b hotfix/security-fix
# ... make critical fixes ...
git checkout main
git merge hotfix/security-fix
npm run release  # Auto-increments patch version
```

### Security Release Process

```mermaid
graph TD
    A[Security Issue Identified] --> B[Create Hotfix Branch]
    B --> C[Implement Fix]
    C --> D[Security Review]
    D --> E[Emergency Testing]
    E --> F[Fast-track Release]
    F --> G[Security Advisory]
    G --> H[Immediate Deployment]
```

## 📊 Release Analytics

### Metrics Tracking

We monitor:
- **Release Frequency**: Target monthly minor releases
- **Time to Release**: From commit to production
- **Rollback Rate**: Percentage of releases requiring rollback
- **Breaking Change Impact**: User adoption of major versions

### Release Health Dashboard

```mermaid
graph TD
    A[Release Metrics] --> B[Frequency Analysis]
    A --> C[Quality Metrics]
    A --> D[Adoption Tracking]
    
    B --> E[Sprint Velocity]
    C --> F[Bug Discovery Rate]
    D --> G[Version Distribution]
    
    E --> H[Process Optimization]
    F --> H
    G --> H
```

## 🎯 Version Planning

### Release Roadmap

| Quarter | Target Version | Major Features |
|---------|----------------|----------------|
| **Q1 2025** | v2.1.0 | Guild System, Enhanced PvP |
| **Q2 2025** | v2.2.0 | Trading System, Market Economy |
| **Q3 2025** | v2.3.0 | World Events, Dynamic Content |
| **Q4 2025** | v3.0.0 | Architecture Modernization |

### Breaking Change Planning

```mermaid
gantt
    title Breaking Change Timeline
    dateFormat  YYYY-MM-DD
    section API v2 Deprecation
    Announce deprecation    :2025-01-01, 30d
    Provide migration tools :2025-02-01, 60d
    Remove in v3.0         :2025-07-01, 1d
    
    section Database Schema
    Schema v2 introduction  :2025-03-01, 30d
    Dual schema support    :2025-04-01, 90d
    Schema v1 removal      :2025-07-01, 1d
```

## 🛠️ Development Guidelines

### Version-Aware Development

1. **Feature Flags**: Use flags for major features
2. **Gradual Rollout**: Implement features incrementally
3. **Backward Compatibility**: Maintain API compatibility within major versions
4. **Documentation**: Update docs with version-specific information

### Pre-release Testing

```mermaid
graph TD
    A[Feature Complete] --> B[Alpha Release]
    B --> C[Internal Testing]
    C --> D[Beta Release]
    D --> E[Community Testing]
    E --> F[Release Candidate]
    F --> G[Production Validation]
    G --> H[Stable Release]
```

---

> 🎯 **Goal**: Predictable, reliable releases that balance rapid innovation with stability and user confidence in our versioning system.
