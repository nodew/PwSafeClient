# PwSafe Client Functional Analysis (Based on Official Password Safe Specification)

## Important Notice

This document analyzes the PwSafeClient mockup functionality **based strictly on the official Password Safe specification**, not generic password manager features.

## Password Safe Official Specification Reference

- **Official Website:** https://www.pwsafe.org/
- **Format Specifications:**
 - [Password Safe v3 Format](https://github.com/pwsafe/pwsafe/blob/master/docs/formatV3.txt)
 - [Password Safe v4 Format](https://github.com/pwsafe/pwsafe/blob/master/docs/formatV4.txt)
- **Official Implementation:** https://github.com/pwsafe/pwsafe

## Table of Contents

1. [Features Supported by Password Safe](#1-features-supported-by-password-safe)
2. [Missing Official Features in Mockup](#2-missing-official-features-in-mockup)
3. [Unreasonable Flows in Mockup](#3-unreasonable-flows-in-mockup)
4. [Implementation Priority](#4-implementation-priority)

## 1. Features Supported by Password Safe

### 1.1 Password Safe Entry Types

According to official specifications, Password Safe **only has the following types**:

#### ✅ Standard Entry
Each entry includes the following fields:
- **Title** (required) - 0x01
- **Username** - 0x02
- **Password** (required) - 0x04
- **Notes** - 0x03
- **URL** - 0x0B
- **Email** - 0x14
- **Group** (for folder organization)
- **UUID** - 0x0A (entry unique identifier)
- **Creation Time** - 0x05
- **Password Modification Time** - 0x06
- **Last Access Time** - 0x07
- **Password Expiry Time** - 0x08
- **Password Policy** - 0x09
- **Password History** - 0x0F (historical password records)
- **Autotype** - 0x0C
- **Run Command** - 0x12
- **Protected Entry** - 0x13

#### ✅ Alias Entry
- References another entry's password
- Used for shared password scenarios
- Syntax: `[base]` or `[group.title.username]`

#### ✅ Shortcut Entry
- References another complete entry (including username and password)
- Unlike Alias, Shortcut references the entire entry

#### ✅ Attachments
- Supported from v3.70
- Each entry can have **one** file attachment
- Fields in v4 format: 0x22 (content), 0x23 (filename), 0x24 (type)

#### ❌ Types NOT Supported by Password Safe
- ❌ Credit Card - Not a separate type
- ❌ Identity - Not a separate type
- ❌ Bank Account - Not a separate type
- ❌ License - Not a separate type
- ❌ SSH Key - Not a separate type

**Important:** Password Safe uses a **unified Entry structure**. All sensitive information is stored as standard entries, with additional information recorded in the Notes field.

### 1.2 Core Features Supported by Password Safe

#### ✅ Password History
**Official documentation confirms: Password Safe natively supports password history**

- Field type: 0x0F
- Format: `nn nn YYYYMMDDHHMMSS password`
- Stores timestamps and content for each password
- Configurable save quantity (global or per-entry)
- Displayed in the "Additional" tab of the Entry edit dialog

**Mockup Status:** ❌ Missing password history feature

**Impact:**
- Users cannot view historical passwords
- Cannot revert to previous passwords
- Does not comply with Password Safe official specification

**Solution:**
Add password history functionality:
1. Add "Password History" section in Entry Detail page
2. Display format:
 - Date/time: 2024-01-15 14:30:25
 - Password: [Show] [Copy] buttons
3. Clicking historical password allows:
 - Show password
 - Copy to clipboard
 - Restore as current password (requires confirmation)
4. Configure in Settings:
 - Number of historical passwords to save (default: 5)
 - Enable for all entries

#### ✅ Groups/Folders
- Password Safe uses **dot-separated** hierarchical structure
- Example: `Banking.Credit Cards.Chase`
- Group name stored in Entry's Group field
- Not a standalone record type

**Mockup Status:** ✅ Basic folder functionality supported

**Improvement Suggestions:**
- Support multi-level nesting (e.g., Group.SubGroup.SubSubGroup)
- Display complete path breadcrumbs
- Support drag-and-drop to move entries between groups

#### ✅ Password Policy
- Field type: 0x09
- Can be set per-entry
- Can also use Named Policy
- Includes: length, character sets, excluded characters, etc.

**Mockup Status:** ✅ Mentioned in Settings, but not detailed enough

**Improvement Suggestions:**
Enhance password policy functionality:
1. Apply policy when Generate Password is used
2. Manage named policies in Settings:
 - Policy name (e.g., "High Security", "Legacy Systems")
 - Length range
 - Character sets (upper/lower case, numbers, symbols)
 - Excluded characters
 - Password patterns
3. Select policy when Add/Edit Entry
4. Verify password compliance with policy

#### ✅ Password Expiry
- Field type: 0x08
- Stores password validity period
- Can set expiry date or days

**Mockup Status:** ❌ No expiration reminder feature

**Solution:**
Add password expiration functionality:
1. Display expiration status in Entry Detail
2. Start warning 7 days before expiration
3. Mark as red after expiration
4. Display expiration icon in Password List
5. Configure default validity period in Settings (e.g., 90 days)

### 1.3 Features NOT Supported by Password Safe

#### ❌ Recycle Bin
**Official confirmation: Password Safe has no recycle bin feature**
- Deletion is permanent
- Relies on backups to recover accidentally deleted entries

**Mockup Suggestion:**
- Must have clear confirmation dialog before deletion
- Suggestion: "This will permanently delete the entry. This cannot be undone."
- Provide regular backup functionality

#### ❌ Tags/Labels
**Official confirmation: Password Safe only has Groups, no Tags**
- Can only organize via Group field
- Does not support multi-tag classification
- An entry can only belong to one Group

**Mockup Suggestion:**
- Do not add tag functionality (incompatible with format)
- Use Group's hierarchical structure to organize entries
- Search can compensate for lack of tags

#### ❌ Custom Fields
**Official confirmation: Password Safe v3/v4 format has fixed schema**
- Field types are predefined (0x01-0x24, etc.)
- Does not support user-defined field types
- Additional information can only be stored in Notes field

**Mockup Suggestion:**
- Do not promise custom field functionality
- Fully utilize Notes field
- Can provide Notes templates (e.g., "Email: xxx\nPhone: xxx")

## 2. Missing Official Features in Mockup

### 2.1 Password History View
**Severity:** 🔴 High

**Current Status:** Entry Detail page only displays current password

**pwsafe Support:** ✅ Yes, field 0x0F

**Solution:**
Add "Password History" section in Entry Detail page:
- Display history list with [Show] [Copy] [Restore] actions
- Settings: "Save password history" + retention count

### 2.2 Password Expiration Reminder
**Severity:** 🟡 Medium

**Current Status:** No expiration-related functionality

**pwsafe Support:** ✅ Yes, field 0x08

**Solution:**
1. Display in Entry Detail: ⚠️ Password expires in 7 days
2. In Password List: Netflix [!] (expiration icon)
3. Settings: Default password lifetime: [90] days, Warn before expiry: [7] days

### 2.3 Alias and Shortcut Support
**Severity:** 🟡 Medium

**Current Status:** No Alias entry support visible

**pwsafe Support:** ✅ Yes, via Password field reference

**Solution:**
Add Alias Entry type:
- Create New → Password Entry / Alias Entry / Shortcut Entry
- Alias configuration: Base Entry: [Select Entry...], Display: [g:Netflix]
- Display in list: Netflix Login → [Alias icon]

## 3. Unreasonable Flows in Mockup

### 3.1 Entry Deletion Without Warning
**Issue:** Deletion is permanent (pwsafe has no recycle bin), but Delete button has no clear warning

**Solution:**
Display confirmation dialog after clicking Delete:
- "⚠️ Delete Entry"
- "Are you sure you want to permanently delete 'Netflix'?"
- "This action CANNOT be undone. Consider backing up your database first."
- [Cancel] [Delete Permanently]
- Delete button should be red warning style

### 3.2 Import/Export Format
**Issue:** Supported formats not clearly specified

**pwsafe Official Support:**
- ✅ .psafe3 (v3 format, recommended)
- ✅ .dat (v1/v2 format, old version)
- ✅ .txt (plaintext export, not recommended)
- ✅ .xml (plaintext export)

### 3.3 Sync Functionality Over-Promise
**Issue:** Mentions "Cloud Sync", but pwsafe itself does not provide cloud sync service

**pwsafe Reality:**
- Official pwsafe **does not provide cloud sync service**
- Users need to manually place .psafe3 file in cloud drive (Dropbox, OneDrive, etc.)
- Relies on file system sync, pwsafe only handles file locking

**Solution:**
Settings → Sync:
- Database Location: Local only / Cloud storage folder
- Note: "Password Safe does not provide cloud sync service. You need to place the .psafe3 file in a cloud-synced folder."
- Current location: /Users/xxx/Dropbox/Personal.psafe3
- [Change Location...]

## 4. Implementation Priority

### Phase 1: Fix Specification Discrepancies (2-3 weeks)

#### P0 - Critical Features (Must Implement)

1. **Password History Feature** (3 days)
 - Display historical password list
 - Copy historical password
 - Restore historical password
 - Configure save quantity

2. **Delete Confirmation Dialog** (1 day)
 - Clear warning of permanent deletion
 - Suggest backup
 - Double confirmation

3. **Clarify Sync Mechanism** (2 days)
 - Remove misleading "Cloud Sync"
 - Explain file location selection
 - Handle file conflicts

4. **Password Expiration Reminder** (3 days)
 - Display expiration status
 - Expiration warning
 - Configure default validity period

**Subtotal: 9 days**

### Phase 2: Enhance Password Safe Specific Features (2-3 weeks)

#### P1 - High Priority

1. **Alias/Shortcut Entries** (4 days)
2. **Password Policy Management** (3 days)
3. **Attachment Support** (4 days)
4. **Improved Group Management** (3 days)

**Subtotal: 14 days**

### Phase 3: Import/Export and Compatibility (1-2 weeks)

#### P2 - Medium Priority

1. **Enhanced Import Functionality** (3 days)
2. **Enhanced Export Functionality** (3 days)
3. **Improved Search** (2 days)

**Subtotal: 8 days**

### Features NOT to Implement (Incompatible with pwsafe)

#### ❌ Explicitly Not Implementing

1. ❌ **Recycle Bin** - pwsafe format does not support
2. ❌ **Tag System** - Only Groups
3. ❌ **Custom Fields** - Fixed schema
4. ❌ **Multiple Entry Types** - Unified Entry structure
5. ❌ **Built-in TOTP Generator** - Beyond pwsafe scope
6. ❌ **Cloud Sync Service** - pwsafe does not provide
7. ❌ **Sharing Features** - Single-user design
8. ❌ **Version Control** - Relies on backups
9. ❌ **Audit Logs** - Format does not support
10. ❌ **Batch Operations** - Can be implemented but not priority

Implementing these features would require extending the pwsafe format or using additional metadata files, which would break compatibility with official pwsafe.

## 5. Summary

### Revised Statistics

**Features to Implement:** 11 (not 42 as before)
**Estimated Total Time:** 31 days (about 6 weeks, not 76-104 days)

### Priority Distribution

| Priority | Features | Days | Description |
| P0 | 4 | 9 | Fix specification discrepancies |
| P1 | 4 | 14 | Enhance official features |
| P2 | 3 | 8 | Import/export compatibility |
| **Total** | **11** | **31** | **~6 weeks** |

### Core Principles

1. **Strictly Follow pwsafe Format Specification**
 - Do not add features unsupported by format
 - Maintain compatibility with official pwsafe

2. **Do Not Over-Promise**
 - Clearly state this is a pwsafe client
 - Do not mislead users about generic password manager functionality

3. **Focus on Core Value**
 - Do well in reading/writing pwsafe format
 - Provide good mobile experience
 - Fully support all officially defined fields

4. **Clear Feature Boundaries**
 - Clearly mark pwsafe limitations in UI
 - Provide explanations and alternatives
 - Avoid user confusion

## References

### Official Documentation
- [Password Safe Official Site](https://www.pwsafe.org/)
- [Password Safe v3 Format Specification](https://github.com/pwsafe/pwsafe/blob/master/docs/formatV3.txt)
- [Password Safe v4 Format Specification](https://github.com/pwsafe/pwsafe/blob/master/docs/formatV4.txt)
- [Password Safe Official Repository](https://github.com/pwsafe/pwsafe)

### Implementation References
- [Medo.PasswordSafe](https://github.com/medo64/Medo.PasswordSafe) - C# library
- [pypwsafe](https://github.com/ronys/pypwsafe) - Python library

**Document Version:** 1.1 (Corrected)
**Last Updated:** 2024-01-26
**Status:** Corrected based on official pwsafe specification
