#!/bin/bash

# Changelog Updater for OpenCoreMMO
# Automatically updates CHANGELOG.md based on individual conventional commits

# Colors and logging
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

log_info() { echo -e "${BLUE}[INFO]${NC} $1"; }
log_success() { echo -e "${GREEN}[SUCCESS]${NC} $1"; }
log_warning() { echo -e "${YELLOW}[WARNING]${NC} $1"; }
log_error() { echo -e "${RED}[ERROR]${NC} $1"; }

# Configuration
CHANGELOG_FILE="${CHANGELOG_FILE:-CHANGELOG.md}"

# Function to get commit category for changelog (Keep a Changelog format)
get_changelog_category() {
    local commit_msg="$1"

    if [[ $commit_msg =~ ^feat ]]; then
        echo "Added"
    elif [[ $commit_msg =~ ^fix ]]; then
        echo "Fixed"
    elif [[ $commit_msg =~ ^docs ]]; then
        echo "Documentation"
    elif [[ $commit_msg =~ ^style ]]; then
        echo "Style"
    elif [[ $commit_msg =~ ^refactor ]]; then
        echo "Changed"
    elif [[ $commit_msg =~ ^perf ]]; then
        echo "Performance"
    elif [[ $commit_msg =~ ^test ]]; then
        echo "Tests"
    elif [[ $commit_msg =~ ^build ]]; then
        echo "Build"
    elif [[ $commit_msg =~ ^ci ]]; then
        echo "CI/CD"
    elif [[ $commit_msg =~ ^chore ]]; then
        echo "Maintenance"
    elif [[ $commit_msg =~ ^revert ]]; then
        echo "Reverted"
    else
        echo "Changed"
    fi
}

# Function to format commit message for changelog
format_commit_for_changelog() {
    local commit_msg="$1"
    local commit_sha="$2"
    local author_name="$3"
    local author_email="$4"

    # Extract description (remove type and scope)
    local description=$(echo "$commit_msg" | sed -E 's/^[a-z]+(\([^)]+\))?(!)?: //')

    # Capitalize first letter
    description="$(echo "${description:0:1}" | tr '[:lower:]' '[:upper:]')${description:1}"

    # Get GitHub username from email if possible
    local github_user=""
    if [[ $author_email =~ ([^@]+)@users\.noreply\.github\.com ]]; then
        github_user="${BASH_REMATCH[1]}"
    elif [[ $author_email =~ ([^@]+)\.([^@]+)@users\.noreply\.github\.com ]]; then
        github_user="${BASH_REMATCH[2]}"
    fi

    # Format the entry
    if [[ -n "$github_user" && "$github_user" != "noreply" ]]; then
        echo "- $description ([@$github_user](https://github.com/$github_user))"
    else
        echo "- $description ($author_name)"
    fi
}

# Function to add commit to changelog
add_commit_to_changelog() {
    local commit_msg="$1"
    local commit_sha="$2"
    local author_name="$3"
    local author_email="$4"

    if [[ ! -f "$CHANGELOG_FILE" ]]; then
        log_error "Changelog file not found: $CHANGELOG_FILE"
        return 1
    fi

    local category=$(get_changelog_category "$commit_msg")
    local formatted_entry=$(format_commit_for_changelog "$commit_msg" "$commit_sha" "$author_name" "$author_email")

    log_info "Adding to $category: $formatted_entry"

    # Create a temporary file
    local temp_file=$(mktemp)

    # Check if entry already exists to avoid duplicates
    if grep -q "^- $description" "$CHANGELOG_FILE"; then
        log_info "Entry already exists in changelog, skipping"
        return 0
    fi

    # Process the changelog
    local in_unreleased=false
    local category_found=false
    local category_section="### $category"
    local added_entry=false    while IFS= read -r line || [[ -n "$line" ]]; do
        # Check if we're entering the Unreleased section
        if [[ $line =~ ^\#\#[[:space:]]*\[Unreleased\] ]]; then
            echo "$line" >> "$temp_file"
            in_unreleased=true
            continue
        fi

        # Check if we've left the Unreleased section (found another ## section)
        if [[ $in_unreleased == true && $line =~ ^\#\#[[:space:]]+ && ! $line =~ \[Unreleased\] ]]; then
            # If we haven't added our entry yet, add the category and entry before this section
            if [[ $added_entry == false ]]; then
                echo "" >> "$temp_file"
                echo "$category_section" >> "$temp_file"
                echo "$formatted_entry" >> "$temp_file"
                added_entry=true
            fi
            echo "$line" >> "$temp_file"
            in_unreleased=false
            continue
        fi

        # If we're in unreleased and find our category, add the entry after the header
        if [[ $in_unreleased == true && $line == "$category_section" ]]; then
            echo "$line" >> "$temp_file"
            echo "$formatted_entry" >> "$temp_file"
            category_found=true
            added_entry=true
            continue
        fi

        # If we're in unreleased and hit another category without finding ours, add our category first
        if [[ $in_unreleased == true && $category_found == false && $line =~ ^\#\#\#[[:space:]]+ ]]; then
            echo "$category_section" >> "$temp_file"
            echo "$formatted_entry" >> "$temp_file"
            echo "" >> "$temp_file"
            echo "$line" >> "$temp_file"
            category_found=true
            added_entry=true
            continue
        fi

        echo "$line" >> "$temp_file"

    done < "$CHANGELOG_FILE"

    # If we're still in unreleased and haven't added our entry, add it at the end
    if [[ $in_unreleased == true && $added_entry == false ]]; then
        echo "" >> "$temp_file"
        echo "$category_section" >> "$temp_file"
        echo "$formatted_entry" >> "$temp_file"
    fi

    # Replace the original file
    mv "$temp_file" "$CHANGELOG_FILE"
    log_success "Changelog updated successfully"
}

# Function to process a single commit (called from pre-receive)
process_commit() {
    local commit_sha="$1"

    if [[ -z "$commit_sha" ]]; then
        log_error "No commit SHA provided"
        return 1
    fi

    # Get commit information
    local commit_msg=$(git log --format=%s -n 1 "$commit_sha")
    local author_name=$(git log --format=%an -n 1 "$commit_sha")
    local author_email=$(git log --format=%ae -n 1 "$commit_sha")

    # Check if it's a conventional commit
    if [[ $commit_msg =~ ^(feat|fix|docs|style|refactor|perf|test|build|ci|chore|revert)(\(.+\))?: ]]; then
        add_commit_to_changelog "$commit_msg" "$commit_sha" "$author_name" "$author_email"
        return 0
    else
        log_info "Skipping non-conventional commit: $commit_msg"
        return 0
    fi
}

# Function to process multiple commits (for bulk updates)
process_commits() {
    local commit_list="$1"
    local updated_count=0

    for commit in $commit_list; do
        if process_commit "$commit"; then
            ((updated_count++))
        fi
    done

    log_success "Processed $updated_count commits"
}

# Main execution
if [[ "${BASH_SOURCE[0]}" == "${0}" ]]; then
    case "${1:-}" in
        --commit)
            if [[ -z "$2" ]]; then
                log_error "Usage: $0 --commit <commit_sha>"
                exit 1
            fi
            process_commit "$2"
            ;;
        --commits)
            if [[ -z "$2" ]]; then
                log_error "Usage: $0 --commits <commit_list>"
                exit 1
            fi
            process_commits "$2"
            ;;
        --help)
            echo "Changelog Updater for OpenCoreMMO"
            echo ""
            echo "Usage: $0 [options]"
            echo "Options:"
            echo "  --commit <sha>   Process a single commit"
            echo "  --commits <list> Process multiple commits (space-separated)"
            echo "  --help           Show this help message"
            echo ""
            echo "This script is typically called automatically by Git hooks."
            echo "It updates CHANGELOG.md following Keep a Changelog format."
            ;;
        *)
            log_error "Invalid usage. Use --help for options."
            exit 1
            ;;
    esac
fi
