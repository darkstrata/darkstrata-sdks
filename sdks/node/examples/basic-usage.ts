/**
 * Basic usage example for @darkstrata/credential-check
 *
 * This example demonstrates how to check a single credential
 * for exposure in data breaches.
 *
 * Run: npx tsx examples/basic-usage.ts
 */

import { DarkStrataCredentialCheck, hashCredential } from '../src/index.js';

async function main() {
  // Create a client with your API key
  const client = new DarkStrataCredentialCheck({
    apiKey: process.env.DARKSTRATA_API_KEY ?? 'your-api-key-here',
  });

  // Check a credential
  const email = 'user@example.com';
  const password = 'password123';

  console.log(`Checking credential for: ${email}`);
  console.log('---');

  // 1. Hash the credential locally: SHA-256 of "email:password".
  //    The plaintext email and password never leave this process.
  const hash = hashCredential(email, password);

  // 2. Check the hash. The SDK sends only the first 5-6 characters (the
  //    k-anonymity prefix) to the API and compares the full hash locally.
  const result = await client.checkHash(hash);

  if (result.found) {
    console.log('⚠️  WARNING: This credential was found in a data breach!');
    console.log('   You should change this password immediately.');
  } else {
    console.log('✓  This credential was not found in known breaches.');
  }

  console.log('');
  console.log('Metadata:');
  console.log(`  - Prefix queried: ${result.metadata.prefix}`);
  console.log(`  - Total matches for prefix: ${result.metadata.totalResults}`);
  console.log(`  - Checked at: ${result.metadata.checkedAt.toISOString()}`);
  console.log(`  - Cached result: ${result.metadata.cachedResult}`);
}

main().catch(console.error);
