using Medo.Security.Cryptography.PasswordSafe;

using PwSafeClient.AppCore.Configuration;
using PwSafeClient.AppCore.Vault;
using PwSafeClient.AppCore.Vault.Editing;

namespace PwSafeClient.Shared.UnitTests;

[TestClass]
public sealed class VaultSessionGroupOperationsTests
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
    public void CreateGroup_AddsEmptyGroupHeader()
    {
        var doc = new Document("password");
        var session = CreateSession(doc);

        var result = session.CreateGroup("Finance.Banking");

        Assert.IsTrue(result.IsSuccess);
        CollectionAssert.Contains(session.GetEmptyGroupPaths().ToList(), "Finance.Banking");
    }

    [TestMethod]
    public void CreateGroup_Duplicate_Fails()
    {
        var doc = new Document("password");
        var session = CreateSession(doc);
        _ = session.CreateGroup("Work");

        var result = session.CreateGroup("Work");

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("Group already exists.", result.ErrorMessage);
    }

    [TestMethod]
    public void RenameGroup_UpdatesEntryGroups()
    {
        var doc = new Document("password");
        doc.Entries.Add(new Entry
        {
            Title = "Entry",
            Password = "secret",
            Group = "Work.Personal"
        });

        var session = CreateSession(doc);

        var result = session.RenameGroup("Work", "Office");

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(1, result.AffectedEntries);
        Assert.AreEqual("Office.Personal", doc.Entries[0].Group.ToString());
    }

    [TestMethod]
    public void RenameGroup_RejectsMoveIntoSelf()
    {
        var doc = new Document("password");
        var session = CreateSession(doc);

        var result = session.RenameGroup("Work", "Work.Archive");

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("Cannot move a group into itself.", result.ErrorMessage);
    }

    [TestMethod]
    public void DeleteEmptyGroup_RemovesHeader()
    {
        var doc = new Document("password");
        var session = CreateSession(doc);
        _ = session.CreateGroup("Temp");

        var result = session.DeleteEmptyGroup("Temp");

        Assert.IsTrue(result.IsSuccess);
        CollectionAssert.DoesNotContain(session.GetEmptyGroupPaths().ToList(), "Temp");
    }

    [TestMethod]
    public void DeleteEmptyGroup_ReturnsNotFoundForNonEmptyGroup()
    {
        var doc = new Document("password");
        doc.Entries.Add(new Entry
        {
            Title = "Entry",
            Password = "secret",
            Group = "Work"
        });

        var session = CreateSession(doc);

        var result = session.DeleteEmptyGroup("Work");

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("Group not found.", result.ErrorMessage);
    }

    [TestMethod]
    public void MoveEntry_UpdatesGroup()
    {
        var doc = new Document("password");
        doc.Entries.Add(new Entry
        {
            Title = "Entry",
            Password = "secret",
            Group = "Old"
        });

        var session = CreateSession(doc);

        var result = session.MoveEntry(0, "New");

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("New", doc.Entries[0].Group.ToString());
    }
}
