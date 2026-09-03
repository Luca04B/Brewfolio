# Project agent skills

This directory contains the versioned agent workflows used for Brewfolio. The
skills come from [mattpocock/skills](https://github.com/mattpocock/skills) and
are installed project-locally so Codex and contributors use the same workflow.

`skills-lock.json` is the source of truth for the installed selection and its
upstream hashes. Keep both `.agents/skills/` and `skills-lock.json` in Git.

## Usage

Run `$setup-matt-pocock-skills` once before using the engineering flow. Start
with `$ask-matt` when you are unsure which workflow fits a task.

Keep Brewfolio-specific rules in `AGENTS.md` and the documents it references.
Leave installed skills unchanged so upstream updates remain reviewable. If a
workflow needs project-specific behavior, create a separately named Brewfolio
skill instead of silently forking an installed one.

## Maintenance

```bash
npx skills@latest check
npx skills@latest update
git diff -- .agents skills-lock.json
```

Review every update before committing it. Remove an unused skill with:

```bash
npx skills@latest remove <skill-name> --yes
```

The copied upstream material is distributed under the license in
[`licenses/mattpocock-skills.txt`](licenses/mattpocock-skills.txt).
