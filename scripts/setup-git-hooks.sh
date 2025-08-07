#!/bin/bash

# Script to configure Git hooks for Conventional Commits validation
# Usage: ./scripts/setup-git-hooks.sh

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

log_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

log_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

log_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Check if we're in a Git repository
if [ ! -d ".git" ]; then
    log_error "This script must be run from the root of a Git repository"
    exit 1
fi

log_info "🔧 Setting up Git hooks for OpenCoreMMO..."

# Create hooks directory if it doesn't exist
mkdir -p .git/hooks

# Backup existing hooks
if [ -f ".git/hooks/pre-commit" ]; then
    log_warning "Backing up existing pre-commit hook..."
    cp .git/hooks/pre-commit .git/hooks/pre-commit.backup.$(date +%Y%m%d_%H%M%S)
fi

if [ -f ".git/hooks/commit-msg" ]; then
    log_warning "Backing up existing commit-msg hook..."
    cp .git/hooks/commit-msg .git/hooks/commit-msg.backup.$(date +%Y%m%d_%H%M%S)
fi

# Install pre-commit hook
cat > .git/hooks/pre-commit << 'EOF'
#!/bin/bash

# Pre-commit hook - 100% shell script, no external dependencies

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

log_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

log_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

log_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Function to validate large files
check_file_sizes() {
    local max_size=10485760  # 10MB in bytes
    local large_files=$(git diff --cached --name-only | xargs -I {} find {} -size +${max_size}c 2>/dev/null || true)

    if [ -n "$large_files" ]; then
        log_error "❌ Large files detected (>10MB):"
        echo "$large_files"
        echo ""
        echo "💡 Consider using Git LFS for large binary files"
        return 1
    fi
    return 0
}

# Function to check whitespace issues
check_whitespace() {
    if git diff --cached --check > /dev/null 2>&1; then
        return 0
    else
        log_error "❌ Whitespace issues detected:"
        git diff --cached --check
        echo ""
        echo "💡 Run: git add -u && git commit --amend"
        return 1
    fi
}

# Function to check C# files
check_csharp_files() {
    local csharp_files=$(git diff --cached --name-only | grep '\.cs$' || true)

    if [ -z "$csharp_files" ]; then
        return 0
    fi

    log_info "🔍 Checking C# files (${csharp_files})"

    # Check basic syntax of C# files
    for file in $csharp_files; do
        if [ -f "$file" ]; then
            # Check for problematic non-ASCII characters
            if grep -P '[^\x00-\x7F]' "$file" > /dev/null 2>&1; then
                log_warning "⚠️  Non-ASCII characters found in $file"
            fi

            # Check for mixed tabs and spaces
            if grep -P '^\t+ +' "$file" > /dev/null 2>&1 || grep -P '^ +\t' "$file" > /dev/null 2>&1; then
                log_warning "⚠️  Mixed tabs and spaces in $file"
            fi
        fi
    done

    # Try to format with dotnet if available
    if command -v dotnet &> /dev/null; then
        log_info "📝 Formatting C# code with dotnet format..."

        # Create file list for dotnet format
        local files_list=$(echo $csharp_files | tr '\n' ' ')

        if dotnet format --include $files_list --severity info > /dev/null 2>&1; then
            log_success "✅ C# formatting completed"

            # Re-stage formatted files
            echo "$csharp_files" | xargs git add
        else
            log_warning "⚠️  dotnet format failed, continuing..."
        fi
    else
        log_warning "⚠️  dotnet CLI not found, skipping automatic formatting"
    fi

    return 0
}

# Main checks
log_info "🚀 Running pre-commit checks..."

# 1. Check file sizes
if ! check_file_sizes; then
    exit 1
fi

# 2. Check whitespace
if ! check_whitespace; then
    exit 1
fi

# 3. Check C# files
if ! check_csharp_files; then
    exit 1
fi

log_success "🎉 All pre-commit checks passed!"
log_info "📝 Files being committed: $(git diff --cached --name-only | wc -l) file(s)"

exit 0
EOF

# Install commit-msg hook
cat > .git/hooks/commit-msg << 'EOF'
#!/bin/bash

# Commit-msg hook - 100% bash validation for Conventional Commits

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m'

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

log_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

log_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

log_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

# Function to show examples
show_examples() {
    echo ""
    echo -e "${CYAN}📚 Valid commit examples:${NC}"
    echo ""
    echo "  feat(outfit): add real-time outfit change notifications"
    echo "  fix(protocol): resolve CreatureOutfitPacket parsing error"
    echo "  docs: update installation guide for macOS"
    echo "  test(combat): add unit tests for damage calculation"
    echo "  refactor(database): improve connection pooling"
    echo "  perf(network): optimize packet processing"
    echo "  style: fix code formatting and remove trailing spaces"
    echo "  build: update .NET SDK to version 9.0"
    echo "  ci: add automated testing workflow"
    echo "  chore: update dependencies"
    echo ""
    echo -e "${CYAN}🔧 Allowed types:${NC}"
    echo "  feat, fix, docs, style, refactor, perf, test, build, ci, chore, revert"
    echo ""
    echo -e "${CYAN}🎯 Recommended scopes:${NC}"
    echo "  protocol, outfit, combat, network, database, lua, api, core, test"
    echo ""
}

# Read commit message
commit_msg_file="$1"
commit_msg=$(head -n 1 "$commit_msg_file" | sed 's/[[:space:]]*$//')

# Arrays of allowed types and scopes
allowed_types=("feat" "fix" "docs" "style" "refactor" "perf" "test" "build" "ci" "chore" "revert")
recommended_scopes=("protocol" "outfit" "combat" "network" "database" "lua" "api" "core" "test")

# Ignore special commits
if [[ $commit_msg =~ ^(Merge|Revert|fixup!|squash!) ]]; then
    log_info "🔄 Special commit detected, skipping validation"
    exit 0
fi

# Ignore empty commits
if [[ -z "$commit_msg" || "$commit_msg" =~ ^[[:space:]]*$ ]]; then
    log_error "❌ Empty commit message!"
    show_examples
    exit 1
fi

# Length validation (first line)
msg_length=${#commit_msg}
if [[ $msg_length -gt 72 ]]; then
    log_error "❌ First line too long ($msg_length characters, max 72)"
    echo "Message: '$commit_msg'"
    echo ""
    echo "💡 Tip: Use the first line for summary and add details in following lines"
    exit 1
fi

if [[ $msg_length -lt 10 ]]; then
    log_error "❌ Message too short ($msg_length characters, min 10)"
    echo "Message: '$commit_msg'"
    show_examples
    exit 1
fi

# Regex to extract commit components
if [[ $commit_msg =~ ^([a-z]+)(\(([^)]+)\))?(!)?: (.+)$ ]]; then
    type="${BASH_REMATCH[1]}"
    scope="${BASH_REMATCH[3]}"
    breaking="${BASH_REMATCH[4]}"
    description="${BASH_REMATCH[5]}"
else
    log_error "❌ Invalid commit format!"
    echo "Message: '$commit_msg'"
    echo ""
    echo "Expected format: <type>[optional scope]: <description>"
    show_examples
    exit 1
fi

# Validate type
type_valid=false
for valid_type in "${allowed_types[@]}"; do
    if [[ "$type" == "$valid_type" ]]; then
        type_valid=true
        break
    fi
done

if [[ "$type_valid" != true ]]; then
    log_error "❌ Type '$type' is not allowed!"
    echo "Allowed types: ${allowed_types[*]}"
    show_examples
    exit 1
fi

# Validate scope (if present)
if [[ -n "$scope" ]]; then
    scope_recommended=false
    for rec_scope in "${recommended_scopes[@]}"; do
        if [[ "$scope" == "$rec_scope" ]]; then
            scope_recommended=true
            break
        fi
    done

    if [[ "$scope_recommended" != true ]]; then
        log_warning "⚠️  Scope '$scope' is not in the recommended list"
        echo "Recommended scopes: ${recommended_scopes[*]}"
        echo ""
    fi

    # Validate scope format
    if [[ ! $scope =~ ^[a-z0-9-]+$ ]]; then
        log_error "❌ Scope must contain only lowercase letters, numbers and hyphens"
        echo "Invalid scope: '$scope'"
        exit 1
    fi
fi

# Validate description
if [[ ${#description} -lt 3 ]]; then
    log_error "❌ Description too short (${#description} characters, min 3)"
    echo "Description: '$description'"
    exit 1
fi

# Validate that description doesn't start with uppercase
if [[ $description =~ ^[A-Z] ]]; then
    log_warning "⚠️  Description should not start with uppercase letter"
    echo "Description: '$description'"
    echo "Suggestion: '$(echo "$description" | sed 's/^./\L&/')'"
    echo ""
fi

# Validate that description doesn't end with period
if [[ $description =~ \.$ ]]; then
    log_warning "⚠️  Description should not end with period"
    echo "Description: '$description'"
    echo ""
fi

# Type-specific validations
case "$type" in
    "feat")
        if [[ ! $description =~ (add|implement|create|introduce) ]]; then
            log_warning "⚠️  For 'feat', consider using verbs like: add, implement, create, introduce"
        fi
        ;;
    "fix")
        if [[ ! $description =~ (fix|resolve|correct|repair) ]]; then
            log_warning "⚠️  For 'fix', consider using verbs like: fix, resolve, correct, repair"
        fi
        ;;
    "docs")
        if [[ ! $description =~ (add|update|improve|fix) ]]; then
            log_warning "⚠️  For 'docs', consider using verbs like: add, update, improve, fix"
        fi
        ;;
esac

# Success
if [[ -n "$scope" ]]; then
    log_success "✅ Valid commit: $type($scope) - $description"
else
    log_success "✅ Valid commit: $type - $description"
fi

if [[ -n "$breaking" ]]; then
    log_warning "⚠️  BREAKING CHANGE detected! Make sure to document it in the commit body"
fi

exit 0
EOF

# Make hooks executable
chmod +x .git/hooks/pre-commit
chmod +x .git/hooks/commit-msg

# Create pre-push hook for changelog updates
cat > .git/hooks/pre-push << 'PREPUSH_EOF'
#!/bin/bash

# Pre-push hook for automatic changelog updates

# Colors and logging
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

log_info() { echo -e "${BLUE}[INFO]${NC} $1"; }
log_success() { echo -e "${GREEN}[SUCCESS]${NC} $1"; }
log_warning() { echo -e "${YELLOW}[WARNING]${NC} $1"; }
log_error() { echo -e "${RED}[ERROR]${NC} $1"; }

# Function to update changelog for commit
update_changelog_for_commit() {
    local commit_sha="$1"

    # Check if changelog update script exists
    if [[ ! -f "scripts/update-changelog.sh" ]]; then
        log_warning "Changelog update script not found, skipping changelog update"
        return 0
    fi

    # Check if CHANGELOG.md exists
    if [[ ! -f "CHANGELOG.md" ]]; then
        log_warning "CHANGELOG.md not found, skipping changelog update"
        return 0
    fi

    # Run the changelog update script
    if ./scripts/update-changelog.sh --commit "$commit_sha" > /dev/null 2>&1; then
        log_info "📝 Updated changelog for commit ${commit_sha:0:8}"
        return 0
    else
        log_warning "Failed to update changelog for commit $commit_sha"
        return 0
    fi
}

# Read pre-push input
remote="$1"
url="$2"

log_info "🔄 Updating changelog for commits being pushed to $remote"

updated_count=0

# Process commits being pushed
while read local_ref local_sha remote_ref remote_sha; do
    # Skip if deleting branch
    if [[ "$local_sha" == "0000000000000000000000000000000000000000" ]]; then
        continue
    fi

    # Get commits to be pushed
    if [[ "$remote_sha" == "0000000000000000000000000000000000000000" ]]; then
        # New branch - get all commits
        commits=$(git rev-list "$local_sha")
    else
        # Existing branch - get new commits only
        commits=$(git rev-list "$remote_sha..$local_sha" 2>/dev/null || git rev-list "$local_sha")
    fi

    # Update changelog for each conventional commit
    for commit in $commits; do
        commit_msg=$(git log --format=%s -n 1 "$commit" 2>/dev/null || continue)

        # Check if it's a conventional commit
        if [[ $commit_msg =~ ^(feat|fix|docs|style|refactor|perf|test|build|ci|chore|revert)(\(.+\))?: ]]; then
            if update_changelog_for_commit "$commit"; then
                ((updated_count++))
            fi
        fi
    done
done

if [[ $updated_count -gt 0 ]]; then
    log_success "✅ Updated changelog with $updated_count commit(s)"

    # Stage the updated changelog
    if [[ -f "CHANGELOG.md" ]]; then
        git add CHANGELOG.md
        log_info "📋 Staged updated CHANGELOG.md"
    fi
else
    log_info "ℹ️  No conventional commits found to update changelog"
fi

exit 0
PREPUSH_EOF

chmod +x .git/hooks/pre-push

# Recommended Git configurations
log_info "⚙️  Setting up recommended Git configurations..."

# Configure core.autocrlf for Windows compatibility
git config core.autocrlf input

# Configure push default
git config push.default simple

# Configure rebase by default for pulls
git config pull.rebase true

log_success "🎉 Git hooks configured successfully!"
echo ""
echo "📋 System configured (100% bash, no JS dependencies):"
echo "   ✅ pre-commit  - Checks files, whitespace and formats C#"
echo "   ✅ commit-msg  - Validates Conventional Commits with robust validation"
echo "   ✅ pre-push    - Automatically updates CHANGELOG.md from conventional commits"
echo ""
echo "🚀 Features:"
echo "   • Complete commit format validation"
echo "   • Automatic C# code formatting (if dotnet available)"
echo "   • Automatic CHANGELOG.md updates on push"
echo "   • Whitespace checking"
echo "   • File size verification"
echo "   • Smart suggestions to improve commits"
echo ""
echo "💡 Useful commands:"
echo "   git commit                       - Commit with automatic validation"
echo "   git push                         - Push with automatic changelog update"
echo "   dotnet format                    - Manual code formatting"
echo "   ./scripts/update-changelog.sh    - Manual changelog update for specific commits"
echo ""
echo "📚 Quick commit types guide:"
echo "   feat:     new feature"
echo "   fix:      bug fix"
echo "   docs:     documentation changes"
echo "   style:    formatting, semicolons, etc"
echo "   refactor: refactoring without functionality change"
echo "   perf:     performance improvements"
echo "   test:     adding or updating tests"
echo "   build:    build system changes"
echo "   ci:       CI/CD changes"
echo "   chore:    maintenance tasks"
echo ""
echo "🎯 Recommended scopes:"
echo "   protocol, outfit, combat, network, database, lua, api, core, test"
echo ""
