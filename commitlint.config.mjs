// Commit message theo Conventional Commits — xem .claude/rules/git-commit-rules.md
export default {
  extends: ['@commitlint/config-conventional'],
  rules: {
    'type-enum': [
      2,
      'always',
      ['build', 'chore', 'ci', 'docs', 'feat', 'fix', 'perf', 'refactor', 'revert', 'style', 'test', 'hotfix']
    ],
    'header-max-length': [2, 'always', 100]
  }
}
