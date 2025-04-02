using Medo.Security.Cryptography.PasswordSafe;

namespace PwSafeClient.Shared.UnitTests;

[TestClass]
public class GroupBuilderTests
{
    [TestMethod]
    public void BuildGroupTest()
    {
        // Arrange
        Entry entry1 = new()
        {
            Group = new GroupPath("group1", "group1-1"),
        };

        Entry entry2 = new()
        {
            Group = new GroupPath("group2", "group2-1"),
        };

        Entry entry3 = new()
        {
            Group = new GroupPath("group1", "group1-2"),
        };

        GroupBuilder groupBuilder = new([entry1, entry2, entry3]);

        // Act
        var group = groupBuilder.Build();

        // Assert
        Assert.AreEqual(2, group.Children.Count);
        Assert.AreEqual("group1", group.Children[0].Name);
        Assert.AreEqual("group2", group.Children[1].Name);

        var group1 = group.Children[0];
        Assert.AreEqual(2, group1.Children.Count);
        Assert.AreEqual("group1-1", group1.Children[0].Name);
        Assert.AreEqual("group1-2", group1.Children[1].Name);

        var group2 = group.Children[1];
        Assert.AreEqual(1, group2.Children.Count);
    }
}
