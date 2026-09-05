//! Basic usage example for the DarkStrata Credential Check SDK.
//!
//! This example demonstrates:
//! - Creating a client with default configuration
//! - Hashing a credential locally and checking the hash
//! - Using check options (date filter, custom HMAC)
//!
//! Run with:
//! ```bash
//! DARKSTRATA_API_KEY=your-key cargo run --example basic_usage
//! ```

use darkstrata_credential_check::{
    crypto_utils::hash_credential, CheckOptions, ClientOptions, DarkStrataCredentialCheck,
};
use std::env;

#[tokio::main]
async fn main() -> Result<(), Box<dyn std::error::Error>> {
    // Get API key from environment variable
    let api_key = env::var("DARKSTRATA_API_KEY")
        .expect("DARKSTRATA_API_KEY environment variable must be set");

    // Create a client with default configuration
    let client = DarkStrataCredentialCheck::new(ClientOptions::new(api_key))?;

    println!("DarkStrata Credential Check - Basic Usage Example\n");

    // Example 1: Check a single credential
    println!("1. Checking a single credential...");
    let (email, password) = ("user@example.com", "password123");

    // 1. Hash the credential locally: SHA-256 of "email:password".
    //    The plaintext email and password never leave this process.
    let hash = hash_credential(email, password);
    println!("   Hash: {}", hash);

    // 2. Check the hash. The SDK sends only the first 5-6 characters (the
    //    k-anonymity prefix) to the API and compares the full hash locally.
    let result = client.check_hash(&hash, None).await?;

    println!("   Found in breach: {}", result.found);
    println!("   Prefix used: {}", result.metadata.prefix);
    println!(
        "   Total matching hashes: {}",
        result.metadata.total_results
    );
    println!("   HMAC source: {}", result.metadata.hmac_source);
    println!();

    // Example 2: Check with date filter (breaches since January 2024)
    println!("2. Checking with date filter...");
    let options = CheckOptions::new().since_epoch_day(19724); // 2024-01-01

    let result = client.check_hash(&hash, Some(options)).await?;

    println!("   Found in breach (since 2024-01-01): {}", result.found);
    if let Some(since) = result.metadata.filter_since {
        println!("   Filter applied: epoch day {}", since);
    }
    println!();

    // Example 3: Check cache status
    println!("3. Cache status:");
    println!("   Cached entries: {}", client.cache_size());

    // Perform same check again to see caching in action
    let result = client.check_hash(&hash, None).await?;
    println!("   Result from cache: {}", result.metadata.cached_result);
    println!();

    // Clear cache
    client.clear_cache();
    println!("   Cache cleared. New size: {}", client.cache_size());

    println!("\nBasic usage example completed successfully!");

    Ok(())
}
