# Contributing to OpenCoreMMO

Welcome to OpenCoreMMO! We're excited to have you contribute to our modern .NET MMORPG server implementation.

## 🎯 Project Management & Priorities

> **📋 Current Development Status**  
> All current priorities, features in development, and future roadmap items are managed through our [GitHub Projects](https://github.com/opencoremmo/opencoremmo/projects).
>
> **Before starting any work:**
> 1. Check the [active project boards](https://github.com/opencoremmo/opencoremmo/projects) for current priorities
> 2. Review [open issues](https://github.com/opencoremmo/opencoremmo/issues) for bugs and feature requests
> 3. Join relevant [discussions](https://github.com/opencoremmo/opencoremmo/discussions) for feature planning

## 📚 Comprehensive Documentation

We've created detailed documentation to help you contribute effectively:

### 🚀 Essential Reading

- **[Contributing Guidelines](./docs/contributing/README.md)** - Complete guide for new contributors
- **[Branch Strategy](./docs/contributing/branch-strategy.md)** - How we organize our codebase
- **[Commit Guidelines](./docs/contributing/commit-guidelines.md)** - Writing meaningful commits

### 🏗️ Development Processes

- **[Versioning Strategy](./docs/development/versioning-strategy.md)** - Our semantic versioning approach
- **[Release Process](./docs/development/release-process.md)** - How we deliver software

## ⚡ Quick Start

### 1. Set Up Development Environment

```bash
# Clone the repository
git clone https://github.com/opencoremmo/opencoremmo.git
cd opencoremmo/server

# Install commit validation tools
npm install

# Create your feature branch
git checkout -b feature/your-feature-name
```

### 2. Make Your Changes

```bash
# Make your changes
# ... develop your feature ...

# Commit normally (validation is automatic)
git commit -m "feat: add new feature description"

# The system will automatically:
# ✅ Validate your commit format
# ✅ Update changelog if needed
```

### 3. Submit Your Contribution

1. Push your branch: `git push origin feature/your-feature-name`
2. Create a Pull Request on GitHub
3. Address review feedback
4. Celebrate your contribution! 🎉

## 📋 Commit Format (Quick Reference)

We use [Conventional Commits](https://www.conventionalcommits.org/):

```
<type>[optional scope]: <description>

[optional body]

[optional footer(s)]
```

**Common types:**
- `feat:` - New features
- `fix:` - Bug fixes  
- `docs:` - Documentation changes
- `style:` - Code formatting
- `refactor:` - Code refactoring
- `test:` - Adding tests
- `chore:` - Maintenance tasks

**Example:**
```bash
feat(auth): add OAuth2 integration
fix(database): resolve connection timeout
docs: update API documentation
```

For detailed commit guidelines, see [Commit Guidelines](./docs/contributing/commit-guidelines.md).

## 🤝 Getting Help

- **📖 Documentation**: Check our [comprehensive docs](./docs/)
- **💬 Discussions**: Join [GitHub Discussions](https://github.com/opencoremmo/opencoremmo/discussions)
- **🐛 Issues**: Report bugs or request features via [GitHub Issues](https://github.com/opencoremmo/opencoremmo/issues)

## 🎯 What We're Looking For

| Type | Description | Getting Started |
|------|-------------|-----------------|
| 🐛 **Bug Fixes** | Resolve existing issues | Check [bug reports](https://github.com/opencoremmo/opencoremmo/labels/bug) |
| ✨ **Features** | Add new functionality | Review [feature requests](https://github.com/opencoremmo/opencoremmo/labels/enhancement) |
| 📚 **Documentation** | Improve docs and guides | See documentation issues |
| 🧪 **Testing** | Add or improve tests | Check test coverage reports |

---

> 🚀 **Ready to contribute?** Start by checking our [GitHub Projects](https://github.com/opencoremmo/opencoremmo/projects) for current priorities, then dive into our [detailed documentation](./docs/) for guidance!
