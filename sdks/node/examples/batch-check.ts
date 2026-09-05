/**
 * Batch credential check example for @darkstrata/credential-check
 *
 * This example demonstrates how to efficiently check multiple
 * credentials in a single batch operation.
 *
 * Run: npx tsx examples/batch-check.ts
 */

import { DarkStrataCredentialCheck, hashCredential } from '../src/index.js';

async function main() {
  // Create a client
  const client = new DarkStrataCredentialCheck({
    apiKey: process.env.DARKSTRATA_API_KEY ?? 'your-api-key-here',
  });

  // Define credentials to check
  const credentials = [
    { email: 'alice@example.com', password: 'alice123' },
    { email: 'bob@example.com', password: 'bob456' },
    { email: 'charlie@example.com', password: 'charlie789' },
    { email: 'diana@example.com', password: 'diana012' },
  ];

  console.log(`Checking ${credentials.length} credentials...`);
  console.log('---');

  // 1. Hash every credential locally. Only the hashes are handed to the client;
  //    the plaintext emails and passwords never leave this process.
  const hashes = credentials.map((c) => hashCredential(c.email, c.password));

  // 2. Check the hashes in one batch. The SDK groups them by 5-6 character
  //    prefix, fetches each prefix once, and compares the full hashes locally.
  //    results[i] corresponds to hashes[i] (and so to credentials[i]).
  const startTime = Date.now();
  const results = await client.checkHashBatch(hashes);
  const duration = Date.now() - startTime;

  // Process results
  const compromised = results.filter((r) => r.found);
  const safe = results.filter((r) => !r.found);

  console.log(`\nResults (completed in ${duration}ms):`);
  console.log('');

  results.forEach((result, i) => {
    const status = result.found ? '⚠️  COMPROMISED' : '✓  Safe';
    console.log(`  ${credentials[i]!.email}: ${status}`);
  });

  console.log('');
  console.log('Summary:');
  console.log(`  - Total checked: ${results.length}`);
  console.log(`  - Compromised: ${compromised.length}`);
  console.log(`  - Safe: ${safe.length}`);

  // Show how many API calls were made (grouped by prefix)
  const uniquePrefixes = new Set(results.map((r) => r.metadata.prefix));
  console.log(`  - API calls made: ${uniquePrefixes.size} (grouped by prefix)`);
}

main().catch(console.error);
