#!/usr/bin/env bash
# Renames the "Atlas" codename (solution, projects, namespaces, folders, config keys)
# to a new name, e.g.:  scripts/rename-codename.sh Northwind
#
# Run it on a clean working tree, then review with `git status` / `git diff`,
# build, test and commit. The displayed company name is NOT affected: that is
# stored in the database and edited from /admin/settings.
set -euo pipefail

old="Atlas"
new="${1:-}"

if [[ ! "$new" =~ ^[A-Z][A-Za-z0-9]*$ ]]; then
    echo "Usage: $0 NewName   (PascalCase letters/digits, e.g. Northwind)" >&2
    exit 1
fi

if [[ "$new" == "$old" ]]; then
    echo "The new name is the same as the current one." >&2
    exit 1
fi

cd "$(git rev-parse --show-toplevel)"

if [[ -n "$(git status --porcelain)" ]]; then
    echo "Commit or stash your changes first; the rename should be a single reviewable commit." >&2
    exit 1
fi

old_lower="$(echo "$old" | tr '[:upper:]' '[:lower:]')"
new_lower="$(echo "$new" | tr '[:upper:]' '[:lower:]')"

# 1. Replace the codename inside tracked text files (binary files are skipped).
git ls-files -z | while IFS= read -r -d '' file; do
    [[ -f "$file" ]] || continue
    grep -Iq . "$file" 2>/dev/null || continue
    if grep -q -e "$old" -e "$old_lower" "$file"; then
        sed -i.bak -e "s/${old}/${new}/g" -e "s/${old_lower}/${new_lower}/g" "$file"
        rm -f "$file.bak"
    fi
done

# 2. Rename tracked files and folders whose path contains the codename.
git ls-files | grep "$old" | while IFS= read -r path; do
    target="${path//$old/$new}"
    mkdir -p "$(dirname "$target")"
    git mv -k "$path" "$target"
done

# 3. Remove folders left empty by the moves.
find src tests -type d -empty -delete 2>/dev/null || true

echo "Renamed ${old} -> ${new}."
echo "Next: dotnet build && dotnet test, then review 'git status' and commit."
echo "Note: the user-secrets id and Data Protection application name changed, so re-run"
echo "'dotnet user-secrets set' for local secrets and expect existing sign-ins to be reset."
