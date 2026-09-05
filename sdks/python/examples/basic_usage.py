"""
Basic usage example for darkstrata-credential-check

This example demonstrates how to check a single credential
for exposure in data breaches.

Run: python examples/basic_usage.py
"""

import asyncio
import os

from darkstrata_credential_check import DarkStrataCredentialCheck, hash_credential


async def main() -> None:
    # Create a client with your API key
    api_key = os.environ.get("DARKSTRATA_API_KEY", "your-api-key-here")

    async with DarkStrataCredentialCheck(api_key=api_key) as client:
        # Check a credential
        email = "user@example.com"
        password = "password123"

        print(f"Checking credential for: {email}")
        print("---")

        # 1. Hash the credential locally: SHA-256 of "email:password".
        #    The plaintext email and password never leave this process.
        hash_value = hash_credential(email, password)

        # 2. Check the hash. The SDK sends only the first 5-6 characters (the
        #    k-anonymity prefix) to the API and compares the full hash locally.
        result = await client.check_hash(hash_value)

        if result.found:
            print("WARNING: This credential was found in a data breach!")
            print("   You should change this password immediately.")
        else:
            print("This credential was not found in known breaches.")

        print("")
        print("Metadata:")
        print(f"  - Prefix queried: {result.metadata.prefix}")
        print(f"  - Total matches for prefix: {result.metadata.total_results}")
        print(f"  - Checked at: {result.metadata.checked_at.isoformat()}")
        print(f"  - Cached result: {result.metadata.cached_result}")


if __name__ == "__main__":
    asyncio.run(main())
