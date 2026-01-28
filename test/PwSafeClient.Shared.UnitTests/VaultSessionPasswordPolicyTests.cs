using Medo.Security.Cryptography.PasswordSafe;

using PwSafeClient.AppCore.Configuration;
using PwSafeClient.AppCore.Vault;
using PwSafeClient.AppCore.Vault.Editing;

namespace PwSafeClient.Shared.UnitTests;

[TestClass]
public sealed class VaultSessionPasswordPolicyTests
{
    private sealed class TestConfigStore : IAppConfigurationStore
    {
        public Task<AppConfiguration> LoadAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(new AppConfiguration());

        public Task SaveAsync(AppConfiguration configuration, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private static VaultSession CreateSession(Document document)
    {
        var session = new VaultSession(new TestConfigStore());
        typeof(VaultSession)
            .GetField("_document", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(session, document);

        typeof(VaultSession)
            .GetProperty(nameof(VaultSession.IsReadOnly), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)!
            .SetValue(session, document.IsReadOnly);

        typeof(VaultSession)
            .GetProperty(nameof(VaultSession.CurrentFilePath), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)!
            .SetValue(session, "test.psafe3");

        return session;
    }

    [TestMethod]
    public void SavePasswordPolicy_PersistsToDocument()
    {
        var doc = new Document("password");
        var session = CreateSession(doc);

        var policy = new VaultPasswordPolicySnapshot
        {
            Name = "Strong",
            TotalPasswordLength = 12,
            MinimumLowercaseCount = 2,
            MinimumUppercaseCount = 2,
            MinimumDigitCount = 2,
            MinimumSymbolCount = 1,
            Style = PasswordPolicyStyle.UseLowercase | PasswordPolicyStyle.UseUppercase | PasswordPolicyStyle.UseDigits | PasswordPolicyStyle.UseSymbols,
            SymbolSet = "!@#"
        };

        var result = session.SavePasswordPolicy(policy);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(1, doc.NamedPasswordPolicies.Count);
        Assert.AreEqual("Strong", doc.NamedPasswordPolicies[0].Name);
        Assert.AreEqual(12, doc.NamedPasswordPolicies[0].TotalPasswordLength);
    }

    [TestMethod]
    public void CreateEntry_RejectsPasswordThatViolatesPolicy()
    {
        var doc = new Document("password");
        var session = CreateSession(doc);

        var policy = new VaultPasswordPolicySnapshot
        {
            Name = "DigitsOnly",
            TotalPasswordLength = 6,
            MinimumLowercaseCount = 0,
            MinimumUppercaseCount = 0,
            MinimumDigitCount = 6,
            MinimumSymbolCount = 0,
            Style = PasswordPolicyStyle.UseDigits,
            SymbolSet = string.Empty
        };

        session.SavePasswordPolicy(policy);

        var request = new VaultEntryEditRequest
        {
            Title = "Test",
            Password = "abcdef",
            PasswordPolicyName = "DigitsOnly"
        };

        var result = session.CreateEntry(request);

        Assert.IsFalse(result.IsSuccess);
        StringAssert.Contains(result.ErrorMessage ?? string.Empty, "digits");
    }

    [TestMethod]
    public void SavePasswordPolicy_RenamesExistingPolicy()
    {
        var doc = new Document("password");
        var session = CreateSession(doc);

        var original = new VaultPasswordPolicySnapshot
        {
            Name = "Legacy",
            TotalPasswordLength = 10,
            MinimumLowercaseCount = 1,
            MinimumUppercaseCount = 1,
            MinimumDigitCount = 1,
            MinimumSymbolCount = 1,
            Style = PasswordPolicyStyle.UseLowercase | PasswordPolicyStyle.UseUppercase | PasswordPolicyStyle.UseDigits | PasswordPolicyStyle.UseSymbols,
            SymbolSet = "!@#"
        };

        session.SavePasswordPolicy(original);

        var updated = new VaultPasswordPolicySnapshot
        {
            Name = "Updated",
            TotalPasswordLength = 12,
            MinimumLowercaseCount = 2,
            MinimumUppercaseCount = 2,
            MinimumDigitCount = 2,
            MinimumSymbolCount = 1,
            Style = PasswordPolicyStyle.UseLowercase | PasswordPolicyStyle.UseUppercase | PasswordPolicyStyle.UseDigits | PasswordPolicyStyle.UseSymbols,
            SymbolSet = "!@#"
        };

        var result = session.SavePasswordPolicy(updated, "Legacy");

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(1, doc.NamedPasswordPolicies.Count);
        Assert.AreEqual("Updated", doc.NamedPasswordPolicies[0].Name);
    }

    [TestMethod]
    public void SetDefaultPasswordPolicy_ReturnsErrorForMissingPolicy()
    {
        var doc = new Document("password");
        var session = CreateSession(doc);

        var result = session.SetDefaultPasswordPolicy("Missing");

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("Policy not found.", result.ErrorMessage);
    }

    [TestMethod]
    public void CreateEntry_AllowsHexOnlyPolicy()
    {
        var doc = new Document("password");
        var session = CreateSession(doc);

        session.SavePasswordPolicy(new VaultPasswordPolicySnapshot
        {
            Name = "HexOnly",
            TotalPasswordLength = 6,
            MinimumLowercaseCount = 0,
            MinimumUppercaseCount = 0,
            MinimumDigitCount = 0,
            MinimumSymbolCount = 0,
            Style = PasswordPolicyStyle.UseHexDigits,
            SymbolSet = string.Empty
        });

        var request = new VaultEntryEditRequest
        {
            Title = "Hex Entry",
            Password = "a1b2c3",
            PasswordPolicyName = "HexOnly"
        };

        var result = session.CreateEntry(request);

        Assert.IsTrue(result.IsSuccess);
    }
}
