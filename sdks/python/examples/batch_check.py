"""
Batch credential check example for darkstrata-credential-check

This example demonstrates how to efficiently check multiple
credentials in a single batch operation.

Run: python examples/batch_check.py
"""

import asyncio
import os
import time

from darkstrata_credential_check import Credential, DarkStrataCredentialCheck, hash_credential


async def main() -> None:
    # Create a client
    api_key = os.environ.get("DARKSTRATA_API_KEY", "your-api-key-here")

    async with DarkStrataCredentialCheck(api_key=api_key) as client:
        # Define credentials to check
        credentials = [
            Credential(email="alice@example.com", password="alice123"),
            Credential(email="bob@example.com", password="bob456"),
            Credential(email="charlie@example.com", password="charlie789"),
            Credential(email="diana@example.com", password="diana012"),
        ]

        print(f"Checking {len(credentials)} credentials...")
        print("---")

        # 1. Hash every credential locally. Only the hashes are handed to the client;
        #    the plaintext emails and passwords never leave this process.
        hashes = [hash_credential(c.email, c.password) for c in credentials]

        # 2. Check the hashes in one batch. The SDK groups them by 5-6 character
        #    prefix, fetches each prefix once, and compares the full hashes locally.
        #    results[i] corresponds to hashes[i] (and so to credentials[i]).
        start_time = time.time()
        results = await client.check_hash_batch(hashes)
        duration_ms = (time.time() - start_time) * 1000

        # Process results
        compromised = [r for r in results if r.found]
        safe = [r for r in results if not r.found]

        print(f"\nResults (completed in {duration_ms:.0f}ms):")
        print("")

        for i, result in enumerate(results):
            status = "COMPROMISED" if result.found else "Safe"
            print(f"  {credentials[i].email}: {status}")

        print("")
        print("Summary:")
        print(f"  - Total checked: {len(results)}")
        print(f"  - Compromised: {len(compromised)}")
        print(f"  - Safe: {len(safe)}")

        # Show how many API calls were made (grouped by prefix)
        unique_prefixes = set(r.metadata.prefix for r in results)
        print(f"  - API calls made: {len(unique_prefixes)} (grouped by prefix)")


if __name__ == "__main__":
    asyncio.run(main())
