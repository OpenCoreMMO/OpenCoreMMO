module.exports = {
  extends: ['@commitlint/config-conventional'],
  rules: {
    'type-enum': [
      2,
      'always',
      [
        'feat',
        'fix',
        'docs',
        'style',
        'refactor',
        'perf',
        'test',
        'build',
        'ci',
        'chore',
        'revert'
      ]
    ],
    'subject-case': [2, 'never', ['sentence-case', 'start-case', 'pascal-case', 'upper-case']],
    'subject-empty': [2, 'never'],
    'subject-full-stop': [2, 'never', '.'],
    'header-max-length': [0],  // Desabilita limitação de tamanho
    'body-max-line-length': [0],  // Desabilita limitação de tamanho 
    'footer-max-line-length': [0],  // Desabilita limitação de tamanho
    'body-leading-blank': [1, 'always'],
    'footer-leading-blank': [1, 'always']
  }
};
