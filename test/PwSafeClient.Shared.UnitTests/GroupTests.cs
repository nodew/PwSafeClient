using Medo.Security.Cryptography.PasswordSafe;

namespace PwSafeClient.Shared.UnitTests;

[TestClass]
public class GroupTests
{
    [TestMethod]
    public void GetGroupPathTest()
    {
        // Arrange
        var root = new Group();
        var group1 = new Group("group1", root);
        var group2 = new Group("group2", group1);
        var group3 = new Group("group3", group2);

        // Act
        var groupPath = group3.GetGroupPath();

        // Assert
        Assert.AreEqual(new GroupPath("group1", "group2", "group3"), groupPath);
        Assert.AreEqual("group1.group2.group3", groupPath.ToString());
    }

    [TestMethod]
    public void InsertByGroupPathTest()
    {
        // Arrange
        var root = new Group();
        var groupPath = new GroupPath(["group1", "group2", "group3"]);

        // Act
        root.InsertByGroupPath(groupPath);

        // Assert
        Assert.AreEqual("group1", root.Children[0].Name);
        Assert.AreEqual("group2", root.Children[0].Children[0].Name);
        Assert.AreEqual("group3", root.Children[0].Children[0].Children[0].Name);
    }

    [TestMethod]
    public void InsertBySegmentsTest()
    {
        // Arrange
        var root = new Group();

        // Act
        root.InsertBySegments(["group1", "group2", "group3"]);

        // Assert
        Assert.AreEqual("group1", root.Children[0].Name);
        Assert.AreEqual("group2", root.Children[0].Children[0].Name);
        Assert.AreEqual("group3", root.Children[0].Children[0].Children[0].Name);

        root.InsertBySegments(["group1", "group4"]);

        var group1 = root.Children[0];

        Assert.IsNotNull(group1);
        Assert.AreEqual(2, group1.Children.Count);
        Assert.AreEqual("group2", group1.Children[0].Name);
        Assert.AreEqual("group4", group1.Children[1].Name);
    }

    [TestMethod]
    public void GetSubGroupsByGroupPathTest()
    {
        // Arrange
        var root = new Group();
        root.InsertBySegments(["group1", "group2", "group3"]);
        root.InsertBySegments(["group1", "group2", "group4"]);

        // Act
        var targetGroup = root.GetSubGroupByGroupPath(new GroupPath("group1", "group2"));

        // Assert
        Assert.IsNotNull(targetGroup);
        Assert.AreEqual(2, targetGroup.Children.Count);
        Assert.AreEqual("group2", targetGroup.Name);
    }

    [TestMethod]
    public void GetSubGroupsBySegmentsTest()
    {
        // Arrange
        var root = new Group();
        root.InsertBySegments(["group1", "group2", "group3"]);
        root.InsertBySegments(["group1", "group2", "group4"]);

        // Act
        var targetGroup = root.GetSubGroupBySegments(["group1", "group2"]);

        // Assert
        Assert.IsNotNull(targetGroup);
        Assert.AreEqual(2, targetGroup.Children.Count);
        Assert.AreEqual("group2", targetGroup.Name);
    }
}
