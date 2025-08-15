#!/usr/bin/env node

const { spawn } = require('child_process');
const path = require('path');

// Get command line arguments (excluding node and script name)
const args = process.argv.slice(2);

// Check if any file/folder arguments are provided
const hasTargets = args.length > 0 && !args.every(arg => arg.startsWith('--'));

let command, commandArgs;

if (hasTargets) {
  // Run ESLint directly on specific files/folders
  command = 'npx';
  commandArgs = ['eslint', ...args];
  console.log(`🔍 Linting specific targets: ${args.filter(arg => !arg.startsWith('--')).join(', ')}`);
} else {
  // Run Angular CLI lint on entire project
  command = 'node';
  commandArgs = [
    '--max-old-space-size=8192',
    './node_modules/@angular/cli/bin/ng',
    'lint',
    ...args // Pass through any flags like --fix
  ];
  console.log('🔍 Linting entire project...');
}

// Spawn the appropriate command
const child = spawn(command, commandArgs, {
  stdio: 'inherit',
  shell: true,
  cwd: process.cwd()
});

child.on('close', (code) => {
  process.exit(code);
});

child.on('error', (error) => {
  console.error('Error running lint command:', error);
  process.exit(1);
});
