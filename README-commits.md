# OpenCoreMmo Server - Commit Validation and Changelog

This repository uses [Conventional Commits](https://www.conventionalcommits.org/) specification for commit messages and automatic changelog generation.

## Commit Message Format

All commit messages must follow the Conventional Commits format:

```
<type>[optional scope]: <description>

[optional body]

[optional footer(s)]
```

### Types

- **feat**: A new feature
- **fix**: A bug fix
- **docs**: Documentation only changes
- **style**: Changes that do not affect the meaning of the code
- **refactor**: A code change that neither fixes a bug nor adds a feature
- **perf**: A code change that improves performance
- **test**: Adding missing tests or correcting existing tests
- **build**: Changes that affect the build system or external dependencies
- **ci**: Changes to CI configuration files and scripts
- **chore**: Other changes that don't modify src or test files
- **revert**: Reverts a previous commit

### Examples

```bash
feat: add new outfit system validation
fix: resolve protocol compatibility issue with TFS
docs: update API documentation for outfit endpoints
refactor: optimize creature outfit packet handling
```

## Using the Tools

### Interactive Commits (Recommended)

Use Commitizen for guided commit creation:

```bash
npm run commit
```

This will prompt you through creating a properly formatted commit message.

### Manual Commits

You can still use regular `git commit`, but messages will be validated:

```bash
git add .
git commit -m "feat: add new feature"
```

Invalid commit messages will be rejected.

### Generating Releases

Create a new release with automatic changelog generation:

```bash
npm run release
```

This will:
1. Bump the version in `package.json`
2. Generate/update `CHANGELOG.md`
3. Create a git tag
4. Create a release commit

### Combined Commit and Release

For convenience, you can commit and release in one command:

```bash
npm run commit-and-release
```

## Validation Rules

- Subject line must be lowercase
- Subject line cannot end with a period
- Header must be 72 characters or less
- Body lines must be 100 characters or less
- Body and footer must have blank lines before them

## Available Scripts

- `npm run commit` - Interactive commit creation
- `npm run release` - Generate release and changelog
- `npm run commit-and-release` - Combined commit and release workflow

## Dependencies

This project uses:
- [commitlint](https://commitlint.js.org/) - Lint commit messages
- [husky](https://typicode.github.io/husky/) - Git hooks
- [commitizen](https://commitizen-tools.github.io/commitizen/) - Interactive commit tool
- [standard-version](https://github.com/conventional-changelog/standard-version) - Release automation
