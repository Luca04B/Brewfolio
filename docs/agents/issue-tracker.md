# Issue tracker: GitHub

Issues and specs for `Luca04B/Brewfolio` live as GitHub issues. Use the `gh` CLI for all operations.

## Conventions

- Create: `gh issue create --title "..." --body "..."`
- Read: `gh issue view <number> --comments`
- List: `gh issue list --state open`
- Comment: `gh issue comment <number> --body "..."`
- Add a label: `gh issue edit <number> --add-label "..."`
- Remove a label: `gh issue edit <number> --remove-label "..."`
- Close: `gh issue close <number> --comment "..."`

Run commands inside the repository so `gh` obtains the repository from the Git remote.

## Pull requests as a triage surface

**PRs as a request surface: no.**

GitHub Issues are the request and planning surface. Pull requests represent proposed implementations.

## Publishing and fetching tickets

When a skill says "publish to the issue tracker", create a GitHub issue.

When a skill says "fetch the relevant ticket", run:

`gh issue view <number> --comments`

## Wayfinding

The wayfinding map is a GitHub issue with linked child issues. Use native GitHub sub-issues and issue dependencies when available. Otherwise, record relationships with task lists and `Blocked by: #<number>` lines.
