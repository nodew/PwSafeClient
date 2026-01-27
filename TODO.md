# PwSafeClient MAUI Application - TODO List

This document outlines areas for improvement and features to be implemented for the Password Safe MAUI mobile/desktop client.

---

## 🔴 Critical / High Priority

### 1. Core Features - Not Yet Implemented

#### 1.1 Group Management
- [ ] **Implement Group Creation** - Currently shows "Not Implemented" message (see `VaultViewModel.cs:236`)
- [ ] Add ability to rename groups
- [ ] Add ability to delete empty groups
- [ ] Add ability to move entries between groups
- [ ] Add group hierarchy visualization in tree view
- [ ] Implement drag-and-drop for entries/groups reorganization

#### 1.2 Cloud Sync Functionality
- [ ] **Implement Cloud Sync backend** - `CloudSyncViewModel` is a stub with no actual sync logic
- [ ] Add support for major cloud providers:
  - [ ] iCloud Drive (iOS/macOS)
  - [ ] Google Drive
  - [ ] Dropbox
  - [ ] OneDrive
  - [ ] WebDAV
- [ ] Implement conflict resolution when syncing
- [ ] Add sync status indicators
- [ ] Implement background sync scheduling
- [ ] Add "Sync on cellular" toggle functionality

#### 1.3 Password Policies - Complete Implementation
- [ ] **Connect Password Policies to actual vault storage** - Currently shows preview-only messages
- [ ] Persist custom password policies in the `.psafe3` file
- [ ] Allow selecting policy when generating passwords
- [ ] Add policy validation during entry creation/editing
- [ ] Import/export password policies

### 2. Entry Management Improvements

#### 2.1 Password Generation in Entry Edit
- [ ] **Add password generator button in EntryEditPage** - Currently no way to generate passwords
- [ ] Show password strength indicator
- [ ] Allow selecting password policy for generation
- [ ] Add password history view
- [ ] Show password age/expiration warnings

#### 2.2 Entry Fields Enhancement
- [ ] Add support for custom fields
- [ ] Add email field (separate from username)
- [ ] Add TOTP/2FA secret support with code generation
- [ ] Add file attachment support
- [ ] Add password expiration date field
- [ ] Add autotype sequence field
- [ ] Add entry icons/favicons

### 3. Security Improvements

#### 3.1 Clipboard Security
- [ ] Verify clipboard auto-clear is working on all platforms
- [ ] Add universal clipboard protection (prevent cloud sync of clipboard)
- [ ] Add option to use system keychain for temporary password storage
- [ ] Implement secure text field to prevent screen capture

#### 3.2 Authentication Enhancements
- [ ] Add password strength meter for master password
- [ ] Add key file support as second factor
- [ ] Add YubiKey/hardware security key support
- [ ] Implement password entropy calculation display
- [ ] Add brute-force protection (lockout after failed attempts)

---

## 🟡 Medium Priority

### 4. User Experience Improvements

#### 4.1 Search & Navigation
- [ ] Add advanced search (by URL, notes, custom fields)
- [ ] Add search filters (by group, date modified, password age)
- [ ] Add recent entries quick access
- [ ] Add favorites/starred entries
- [ ] Implement breadcrumb navigation with tap-to-navigate
- [ ] Add pull-to-refresh on vault list
- [ ] Add sorting options (alphabetical, date modified, password age)

#### 4.2 Entry Details Page
- [ ] Add "Open URL" button to launch browser
- [ ] Add QR code display for password sharing
- [ ] Add password history viewer
- [ ] Add entry modification history/audit log
- [ ] Improve copy feedback with toast notifications

#### 4.3 UI/UX Polish
- [ ] Add empty state illustrations
- [ ] Add onboarding tutorial for first-time users
- [ ] Add haptic feedback for copy actions
- [ ] Add swipe actions on entry list (delete, copy password)
- [ ] Add floating action button for quick entry creation
- [ ] Improve loading states with skeleton screens
- [ ] Add animations for transitions

### 5. Internationalization & Localization

#### 5.1 Language Support
- [ ] Implement proper i18n framework (not just language setting)
- [ ] Add resource files for all UI strings
- [ ] Add RTL language support
- [ ] Support at least: English, Chinese, Spanish, French, German, Japanese

### 6. Platform-Specific Features

#### 6.1 iOS-Specific
- [ ] Add Autofill extension for Safari/apps
- [ ] Add Lock Screen widget for quick access
- [ ] Add Siri Shortcuts integration
- [ ] Add Spotlight search integration
- [ ] Support App Groups for extension data sharing

#### 6.2 Android-Specific
- [ ] Add Autofill Service implementation
- [ ] Add Quick Settings tile
- [ ] Add home screen widget
- [ ] Add notification for password expiration reminders
- [ ] Support split-screen/multi-window mode

#### 6.3 Windows-Specific
- [ ] Add system tray icon with quick actions
- [ ] Add global hotkey for quick access
- [ ] Add Jump List integration
- [ ] Improve Windows Hello integration reliability

#### 6.4 macOS-Specific
- [ ] Add menu bar integration
- [ ] Add Touch Bar support (older Macs)
- [ ] Add Spotlight integration
- [ ] Add Safari extension for autofill

### 7. Database Management

#### 7.1 Multiple Database Support
- [ ] Add ability to have multiple databases open simultaneously
- [ ] Add database switching from vault view
- [ ] Add database merge functionality
- [ ] Add "last accessed" timestamp display

#### 7.2 Database Operations
- [ ] Add database statistics view (entry count, group count, password age distribution)
- [ ] Add database integrity check
- [ ] Add database optimization/compact
- [ ] Add scheduled automatic backups
- [ ] Add backup encryption with separate password

---

## 🟢 Low Priority / Nice to Have

### 8. Import/Export Enhancements

#### 8.1 Import Support
- [ ] Import from KeePass (.kdbx)
- [ ] Import from 1Password export
- [ ] Import from LastPass export
- [ ] Import from Bitwarden export
- [ ] Import from Chrome/Firefox password manager
- [ ] Import from CSV with field mapping

#### 8.2 Export Enhancements
- [ ] Export to KeePass format
- [ ] Export to encrypted ZIP
- [ ] Add selective export (by group or tag)
- [ ] Add print-friendly export for emergency kit

### 9. Advanced Features

#### 9.1 Sharing & Collaboration
- [ ] Add secure password sharing (encrypted link)
- [ ] Add emergency access / trusted contacts
- [ ] Add organization/team support

#### 9.2 Automation
- [ ] Add URL pattern matching for auto-fill hints
- [ ] Add browser extension communication protocol
- [ ] Add SSH key storage and agent support

### 10. Accessibility

- [ ] Add full VoiceOver/TalkBack support
- [ ] Add high contrast mode
- [ ] Add font size customization
- [ ] Add screen reader optimizations
- [ ] Ensure all interactive elements have proper labels

### 11. Testing & Quality

- [ ] Add MAUI UI tests
- [ ] Add integration tests for vault operations
- [ ] Add accessibility testing
- [ ] Add performance benchmarks
- [ ] Add crash reporting (opt-in)
- [ ] Add usage analytics (opt-in)

### 12. Documentation

- [ ] Add in-app help documentation
- [ ] Add FAQ section
- [ ] Add security whitepaper
- [ ] Add developer documentation for contributions
- [ ] Add migration guide from other password managers

---

## 🔧 Technical Debt & Refactoring

### Code Quality
- [ ] Add comprehensive unit tests for MAUI ViewModels
- [ ] Add code coverage tracking for MAUI project
- [ ] Implement proper error handling with user-friendly messages
- [ ] Add logging throughout the application
- [ ] Refactor duplicate code patterns in ViewModels
- [ ] Add XML documentation comments to public APIs

### Architecture
- [ ] Consider implementing Repository pattern for data access
- [ ] Add caching layer for frequently accessed data
- [ ] Implement proper state management (consider Redux pattern)
- [ ] Add offline-first architecture for cloud sync
- [ ] Consider implementing Clean Architecture

### Performance
- [ ] Add virtualization for large entry lists
- [ ] Optimize vault loading for large databases
- [ ] Add lazy loading for entry details
- [ ] Profile and optimize memory usage
- [ ] Add startup performance optimizations

---

## 📋 Implementation Notes

### Priority Guidelines
1. **Critical items** should be addressed first as they represent core functionality gaps
2. **Medium priority** items improve user experience significantly
3. **Low priority** items are enhancements that can be added incrementally

### Dependencies
- Cloud sync requires implementing authentication for each provider
- Autofill features require platform-specific implementations
- TOTP support requires adding a TOTP library (e.g., OtpNet)

### Breaking Changes to Consider
- Adding custom fields may require database format extensions
- Key file support needs careful security review
- Cloud sync needs conflict resolution strategy

---

## 📊 Current Implementation Status Summary

| Feature Category | Status |
|-----------------|--------|
| Core Vault Operations | ✅ Complete |
| Entry CRUD | ✅ Complete |
| Group Browsing | ✅ Complete |
| Group Creation/Management | ❌ Not Implemented |
| Password Generation (standalone) | ✅ Complete |
| Password Generation (in entry) | ❌ Missing UI |
| Biometric Auth | ✅ Complete |
| Auto-lock | ✅ Complete |
| Backup/Restore | ✅ Complete |
| Export Data | ✅ Complete |
| Cloud Sync | ❌ Not Implemented |
| Password Policies | ⚠️ Partial (UI only) |
| Localization | ❌ Not Implemented |
| Autofill (iOS/Android) | ❌ Not Implemented |

---

*Last Updated: January 2026*
