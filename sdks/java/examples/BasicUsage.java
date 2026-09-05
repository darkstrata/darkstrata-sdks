import io.darkstrata.credentialcheck.*;
import io.darkstrata.credentialcheck.exception.*;

/**
 * Basic usage example for the DarkStrata Credential Check SDK.
 */
public class BasicUsage {

    public static void main(String[] args) {
        // Create a client with your API key
        try (DarkStrataCredentialCheck client = new DarkStrataCredentialCheck(
                ClientOptions.builder("your-api-key").build()
        )) {
            String email = "user@example.com";
            String password = "password123";

            // 1. Hash the credential locally: SHA-256 of "email:password".
            //    The plaintext email and password never leave this process.
            String hash = CryptoUtils.hashCredential(email, password);

            // 2. Check the hash. The SDK sends only the first 5-6 characters (the
            //    k-anonymity prefix) to the API and compares the full hash locally.
            CheckResult result = client.checkHash(hash);

            if (result.isFound()) {
                System.out.println("WARNING: Credential found in breach database!");
            } else {
                System.out.println("Credential not found in any known breaches.");
            }

            // Print metadata
            CheckMetadata metadata = result.getMetadata();
            System.out.println("Hash prefix: " + metadata.getPrefix());
            System.out.println("Total results for prefix: " + metadata.getTotalResults());
            System.out.println("HMAC source: " + metadata.getHmacSource());
            System.out.println("Checked at: " + metadata.getCheckedAt());

        } catch (ValidationException e) {
            System.err.println("Validation error: " + e.getMessage());
            if (e.getField() != null) {
                System.err.println("Field: " + e.getField());
            }
        } catch (AuthenticationException e) {
            System.err.println("Authentication failed: " + e.getMessage());
        } catch (DarkStrataException e) {
            System.err.println("Error: " + e.getMessage());
            System.err.println("Retryable: " + e.isRetryable());
        }
    }
}
