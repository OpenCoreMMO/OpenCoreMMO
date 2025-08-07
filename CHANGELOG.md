# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Implement automatic changelog updates on push (brewertonsantos)
- Initial release preparation

### Fixed
- Adjustments to compile extensions, and remove premium tiles from tiles.json (Felipe Muniz)
- Various stability improvements

### Tests
- Validate automatic changelog update system (brewertonsantos)

### Documentation
- Added comprehensive contributing guidelines
- Setup conventional commits workflow

### Build
- Configured development environment setup
- Added pre-commit hooks for code quality

---

## How to Generate Changelog

This changelog is automatically generated from commit messages following [Conventional Commits](https://www.conventionalcommits.org/).

```bash
# Generate changelog for new commits
npm run changelog

# Initialize complete changelog from all commits
npm run changelog:init
```

### Commit Types

| Type | Description | Changelog Section |
|------|-------------|-------------------|
| `feat` | New feature | ✨ Features |
| `fix` | Bug fix | 🐛 Bug Fixes |
| `docs` | Documentation | 📚 Documentation |
| `style` | Code style | 💅 Code Style |
| `refactor` | Code refactoring | ♻️ Code Refactoring |
| `perf` | Performance | ⚡ Performance |
| `test` | Tests | 🧪 Tests |
| `build` | Build system | 🔧 Build System |
| `ci` | CI/CD | 👷 CI/CD |
| `chore` | Maintenance | 🏠 Chores |
| `revert` | Revert | ⏪ Reverts |

All contributors are automatically credited with links to their GitHub profiles.
