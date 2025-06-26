import angularEslint from '@angular-eslint/eslint-plugin';
import angularTemplate from '@angular-eslint/eslint-plugin-template';
import angularParser from '@angular-eslint/template-parser';
import globals from "globals";
import tsParser from '@typescript-eslint/parser';
import tsEslintPlugin from '@typescript-eslint/eslint-plugin';
import stylistic from '@stylistic/eslint-plugin'

export default [
  {
    files: ['**/*.ts'],
    languageOptions: {
      globals: {
        ...globals.browser,
        ...globals.node,
      },
      parser: tsParser,
      parserOptions: {
        project: ['./tsconfig.json'],
        tsconfigRootDir: import.meta.dirname,
        sourceType: 'module',
      },
    },
    plugins: {
      '@typescript-eslint': tsEslintPlugin,
      '@angular-eslint': angularEslint,
      '@stylistic': stylistic,
    },
    rules: {
      ...tsEslintPlugin.configs.recommended.rules,
      ...angularEslint.configs.recommended.rules,
      ...tsEslintPlugin.configs.strict.rules,
      ...tsEslintPlugin.configs.stylistic.rules,
      ...stylistic.configs.recommended.rules
    },
  },
  {
    files: ['**/*.html'],
    languageOptions: {
      parser: angularParser,
    },
    plugins: {
      '@angular-eslint/template': angularTemplate,
    },
    rules: {
      ...angularTemplate.configs.recommended.rules,
    },
  },
  {
    files: ['**/*.spec.ts'],
    languageOptions: {
      globals: {
        ...globals.jasmine,
      },
      parser: tsParser,
      parserOptions: {
        project: ['./tsconfig.spec.json'],
        tsconfigRootDir: import.meta.dirname,
        sourceType: 'module',
      },
    },
    plugins: {
      '@typescript-eslint': tsEslintPlugin,
      '@angular-eslint': angularEslint,
    },
    rules: {
      ...tsEslintPlugin.configs.recommended.rules,
      ...angularEslint.configs.recommended.rules,
    },
  },
];