# Splunkbase Listing Copy

Paste-ready copy for the Splunkbase listing of the **DarkStrata Threat
Intelligence Add-on for Splunk**. Keep this in sync with `CHANGELOG.md`,
`README.md` and `app.manifest` when the add-on changes.

- **Live listing:** [https://splunkbase.splunk.com/app/8955](https://splunkbase.splunk.com/app/8955)
- **App name:** DarkStrata Threat Intelligence Add-on for Splunk
- **Content type:** Add-on
- **Spelling:** British English (organisation, licence)

---

## Short description (tagline)

Ingest DarkStrata credential-exposure threat intelligence into Splunk and Enterprise Security. Detect compromised credentials, infostealer infections and third-party exposure in real time, with CIM-compliant data, ES correlation searches and adaptive-response actions.

Trimmed (if a character limit applies):

Ingest DarkStrata credential-exposure threat intelligence into Splunk Enterprise Security — CIM-compliant, with ES correlation searches and adaptive response.

---

## Overview (main description)

**DarkStrata Threat Intelligence Add-on for Splunk** brings DarkStrata's credential-exposure intelligence directly into Splunk and Splunk Enterprise Security (ES), so your SOC can detect and respond to compromised credentials before they're used against you.

DarkStrata continuously monitors breach data, infostealer/malware logs and third-party exposure for credentials tied to your organisation. This add-on pulls that intelligence into Splunk as structured, CIM-compliant events — ready for correlation, alerting, dashboards and automated response.

Use it to:

- **Detect compromised credentials** before they're used maliciously
- **Identify malware infections** through infostealer credential detection
- **Monitor third-party risk** by tracking corporate credentials exposed on external sites
- **Automate incident response** via ES notable events, correlation searches and adaptive-response actions
- **Enrich threat hunting** with credential-exposure context

Data is delivered in **STIX 2.1** format and mapped to the **Authentication** and **Threat Intelligence** CIM data models, so it works out of the box with Enterprise Security's threat-intelligence framework. Collection is incremental and checkpoint-based — only new intelligence is fetched on each run — with configurable batching, rate limiting and connection pooling for predictable performance.

---

## What's included (capabilities)

**Data collection**

- Two modular inputs: **Indicators** (`/stix/indicators`) and **Alerts** (`/stix/alerts`)
- Checkpoint-based incremental sync (KV Store)
- API credentials validated on save; optional proxy support (HTTP/SOCKS5)

**Data model & knowledge objects**

- Sourcetypes: `darkstrata:stix:observed-data`, `darkstrata:stix:alert`
- CIM mappings for **Authentication** and **Threat Intelligence**
- Pre-built event types, tags, macros and lookups

**Enterprise Security integration**

- Threat-intelligence KV Store collections
- Pre-built correlation searches for notable events
- **Adaptive-response actions**: update alert status, close alert, reopen alert, and get alert details (enrichment)

**Automation & privacy**

- Sample **Splunk SOAR** playbooks (credential-exposure triage, auto-acknowledge, alert enrichment)
- Optional **SHA-256 email hashing** for privacy/compliance
- Confidence-score filtering (maps STIX confidence to threat severity)

---

## Requirements

- **Splunk Enterprise** 9.0.0+ (latest 9.x / 10.x recommended) or **Splunk Cloud** (Victoria Experience)
- **Splunk Enterprise Security** 7.0.0+ (optional — required only for ES correlation searches and adaptive-response actions)
- An active **DarkStrata** subscription and an API key with the `siem:read` scope
- Outbound HTTPS connectivity to `api.darkstrata.io` (443)
- Python 3.9+ (bundled with Splunk 9.0+)

---

## Release notes — v1.1.1

- Adaptive-response actions now verify TLS server certificates and enforce TLS 1.2+
- Declared `python.required` (3.9 / 3.13) for Splunk Enterprise 10.2+ forward compatibility
- Added app icons and a valid alert-action icon
- Leaner, reproducible package (pinned dependencies)
- Passes Splunk AppInspect cloud vetting and SLIM validation

---

## Support & resources

- **Documentation:** https://darkstrata.io/en/docs/splunk/
- **Support:** support@darkstrata.io
- **Source / issues:** https://github.com/darkstrata/darkstrata-sdks (directory `integrations/splunk-ta`)
- **Privacy policy:** https://darkstrata.io/en/privacy-policy/
- **Licence:** Apache 2.0

---

## Splunkbase metadata fields

- **Categories:** Security, Fraud & Compliance · IT Operations
- **Compatible with:** Splunk Enterprise Security · Splunk SOAR · CIM
- **Tags/keywords:** threat intelligence, credential exposure, STIX, infostealer, dark web, compromised credentials, CIM, adaptive response
