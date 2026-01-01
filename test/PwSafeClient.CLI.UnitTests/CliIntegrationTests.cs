using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using Medo.Security.Cryptography.PasswordSafe;

namespace PwSafeClient.Cli.UnitTests;

[TestClass]
public sealed class CliIntegrationTests
{
    private const string PasswordEnvVar = "PWSAFE_TEST_PASSWORD";
    private static readonly SemaphoreSlim BuildLock = new(1, 1);
    private static bool CliBuilt;

    [TestMethod]
    public async Task ConfigPath_UsesPwSafeHomeOverride()
    {
        using var temp = new TempDirectory();
        var homeDir = Path.Combine(temp.Path, "home");
        Directory.CreateDirectory(homeDir);

        var result = await RunCliAsync(
            new Dictionary<string, string?>
            {
                ["PWSAFE_HOME"] = homeDir,
            },
            "config", "path", "--no-color");

        Assert.AreEqual(0, result.ExitCode, result.ToDebugString());

        var expectedConfigPath = Path.Combine(homeDir, ".pwsafe", "config.json");
        var normalizedStdout = result.Stdout.Replace("\r", string.Empty).Replace("\n", string.Empty);
        StringAssert.Contains(normalizedStdout, expectedConfigPath);
    }

    [TestMethod]
    public async Task EntryGet_Json_NoClipboard_DoesNotPrintPassword()
    {
        using var temp = new TempDirectory();
        var homeDir = Path.Combine(temp.Path, "home");
        Directory.CreateDirectory(homeDir);

        var dbPath = Path.Combine(temp.Path, "vault.psafe3");
        var password = "CorrectHorseBatteryStaple!";
        var entryId = CreateDatabaseWithSingleEntry(dbPath, password);

        var env = new Dictionary<string, string?>
        {
            ["PWSAFE_HOME"] = homeDir,
            [PasswordEnvVar] = password,
        };

        var initConfig = await RunCliAsync(env, "config", "init", "--no-color");
        Assert.AreEqual(0, initConfig.ExitCode, initConfig.ToDebugString());

        var addDb = await RunCliAsync(env, "db", "add", "-a", "vault", "-f", dbPath, "-d", "--force", "--no-color");
        Assert.AreEqual(0, addDb.ExitCode, addDb.ToDebugString());

        var result = await RunCliAsync(
            env,
            "entry", "get", entryId.ToString(),
            "--json",
            "--no-clipboard",
            "--password-env", PasswordEnvVar,
            "--no-color");

        Assert.AreEqual(0, result.ExitCode, result.ToDebugString());
        StringAssert.Contains(result.Stdout, "\"copiedToClipboard\": false");
        Assert.IsFalse(result.Stdout.Contains(password, StringComparison.Ordinal), "Password should never be printed in JSON mode.");
    }

    [TestMethod]
    public async Task EntryUpdate_PersistsToDatabaseFile()
    {
        using var temp = new TempDirectory();
        var homeDir = Path.Combine(temp.Path, "home");
        Directory.CreateDirectory(homeDir);

        var dbPath = Path.Combine(temp.Path, "vault.psafe3");
        var password = "CorrectHorseBatteryStaple!";
        var entryId = CreateDatabaseWithSingleEntry(dbPath, password);

        var env = new Dictionary<string, string?>
        {
            ["PWSAFE_HOME"] = homeDir,
            [PasswordEnvVar] = password,
        };

        Assert.AreEqual(0, (await RunCliAsync(env, "config", "init", "--no-color")).ExitCode);
        Assert.AreEqual(0, (await RunCliAsync(env, "db", "add", "-a", "vault", "-f", dbPath, "-d", "--force", "--no-color")).ExitCode);

        var newUrl = "https://example.com";
        var update = await RunCliAsync(env, "entry", "update", entryId.ToString(), "--url", newUrl, "--password-env", PasswordEnvVar, "--no-color");
        Assert.AreEqual(0, update.ExitCode, update.ToDebugString());

        var doc = Document.Load(dbPath, password);
        var updated = doc.Entries.FirstOrDefault(e => e.Uuid == entryId);
        Assert.IsNotNull(updated);
        Assert.AreEqual(newUrl, updated.Url);
    }

    [TestMethod]
    public async Task EntryNew_GeneratesPasswordWhenNotProvided()
    {
        using var temp = new TempDirectory();
        var homeDir = Path.Combine(temp.Path, "home");
        Directory.CreateDirectory(homeDir);

        var dbPath = Path.Combine(temp.Path, "vault.psafe3");
        var password = "CorrectHorseBatteryStaple!";
        CreateEmptyDatabase(dbPath, password);

        var env = new Dictionary<string, string?>
        {
            ["PWSAFE_HOME"] = homeDir,
            [PasswordEnvVar] = password,
        };

        Assert.AreEqual(0, (await RunCliAsync(env, "config", "init", "--no-color")).ExitCode);
        Assert.AreEqual(0, (await RunCliAsync(env, "db", "add", "-a", "vault", "-f", dbPath, "-d", "--force", "--no-color")).ExitCode);

        var title = "GeneratedPasswordEntry";
        var create = await RunCliAsync(
            env,
            "entry", "new",
            "-t", title,
            "--min-length", "16",
            "--max-length", "16",
            "--lowercase",
            "--uppercase",
            "--digits",
            "--special",
            "--password-env", PasswordEnvVar,
            "--no-color");

        Assert.AreEqual(0, create.ExitCode, create.ToDebugString());

        var doc = Document.Load(dbPath, password);
        var added = doc.Entries.FirstOrDefault(e => e.Title == title);
        Assert.IsNotNull(added);
        Assert.IsFalse(string.IsNullOrWhiteSpace(added.Password));
        Assert.AreEqual(16, added.Password.Length);
    }

    [TestMethod]
    public async Task EntryList_TreeView_PrintsGroupsAndEntries()
    {
        using var temp = new TempDirectory();
        var homeDir = Path.Combine(temp.Path, "home");
        Directory.CreateDirectory(homeDir);

        var dbPath = Path.Combine(temp.Path, "vault.psafe3");
        var password = "CorrectHorseBatteryStaple!";

        var (id1, id2, id3) = CreateDatabaseWithMultipleEntries(dbPath, password);

        var env = new Dictionary<string, string?>
        {
            ["PWSAFE_HOME"] = homeDir,
            [PasswordEnvVar] = password,
        };

        Assert.AreEqual(0, (await RunCliAsync(env, "config", "init", "--no-color")).ExitCode);
        Assert.AreEqual(0, (await RunCliAsync(env, "db", "add", "-a", "vault", "-f", dbPath, "-d", "--force", "--no-color")).ExitCode);

        var result = await RunCliAsync(
            env,
            "entry", "list",
            "--tree-view",
            "--password-env", PasswordEnvVar,
            "--no-color");

        Assert.AreEqual(0, result.ExitCode, result.ToDebugString());

        StringAssert.Contains(result.Stdout, "Entries");
        StringAssert.Contains(result.Stdout, "group1");
        StringAssert.Contains(result.Stdout, "group2");
        StringAssert.Contains(result.Stdout, "group3");

        StringAssert.Contains(result.Stdout, "GitHub");
        StringAssert.Contains(result.Stdout, id1.ToString());

        StringAssert.Contains(result.Stdout, "Azure");
        StringAssert.Contains(result.Stdout, id2.ToString());

        StringAssert.Contains(result.Stdout, "RootEntry");
        StringAssert.Contains(result.Stdout, id3.ToString());
    }

    private static Guid CreateDatabaseWithSingleEntry(string dbPath, string password)
    {
        var doc = new Document(password);
        var entry = new Entry
        {
            Title = "GitHub",
            UserName = "me",
            Password = "super-secret",
            Url = "https://github.com",
            Notes = "test",
            Group = "",
            CreationTime = DateTime.UtcNow,
            LastModificationTime = DateTime.UtcNow,
        };

        doc.Entries.Add(entry);
        doc.Save(dbPath);

        return entry.Uuid;
    }

    private static (Guid id1, Guid id2, Guid id3) CreateDatabaseWithMultipleEntries(string dbPath, string password)
    {
        var doc = new Document(password);

        var entry1 = new Entry
        {
            Title = "GitHub",
            UserName = "me",
            Password = "super-secret",
            Url = "https://github.com",
            Notes = "test",
            Group = "group1.group2",
            CreationTime = DateTime.UtcNow,
            LastModificationTime = DateTime.UtcNow,
        };

        var entry2 = new Entry
        {
            Title = "Azure",
            UserName = "you",
            Password = "another-secret",
            Url = "https://portal.azure.com",
            Notes = "test",
            Group = "group1.group3",
            CreationTime = DateTime.UtcNow,
            LastModificationTime = DateTime.UtcNow,
        };

        var entry3 = new Entry
        {
            Title = "RootEntry",
            UserName = "root",
            Password = "root-secret",
            Url = "https://example.com",
            Notes = "test",
            Group = "",
            CreationTime = DateTime.UtcNow,
            LastModificationTime = DateTime.UtcNow,
        };

        doc.Entries.Add(entry1);
        doc.Entries.Add(entry2);
        doc.Entries.Add(entry3);
        doc.Save(dbPath);

        return (entry1.Uuid, entry2.Uuid, entry3.Uuid);
    }

    private static void CreateEmptyDatabase(string dbPath, string password)
    {
        var doc = new Document(password);
        doc.Save(dbPath);
    }

    private static async Task<CliRunResult> RunCliAsync(IDictionary<string, string?> environment, params string[] args)
    {
        var solutionRoot = FindSolutionRoot();
        await EnsureCliBuiltAsync(solutionRoot);

        var cliDll = Path.Combine(solutionRoot, "src", "PwSafeClient.CLI", "bin", "Debug", "net9.0", "pwsafe.dll");
        if (!File.Exists(cliDll))
        {
            throw new FileNotFoundException("CLI output not found after build.", cliDll);
        }

        var psi = new ProcessStartInfo
        {
            FileName = "dotnet",
            WorkingDirectory = solutionRoot,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
        };

        psi.ArgumentList.Add(cliDll);

        foreach (var arg in args)
        {
            psi.ArgumentList.Add(arg);
        }

        foreach (var (key, value) in environment)
        {
            psi.Environment[key] = value;
        }

        using var process = Process.Start(psi) ?? throw new InvalidOperationException("Failed to start dotnet process.");

        var stdoutTask = process.StandardOutput.ReadToEndAsync();
        var stderrTask = process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        return new CliRunResult(
            process.ExitCode,
            await stdoutTask,
            await stderrTask);
    }

    private static async Task EnsureCliBuiltAsync(string solutionRoot)
    {
        if (CliBuilt)
        {
            return;
        }

        await BuildLock.WaitAsync();
        try
        {
            if (CliBuilt)
            {
                return;
            }

            var cliProject = Path.Combine(solutionRoot, "src", "PwSafeClient.CLI", "PwSafeClient.CLI.csproj");
            var build = await RunDotnetAsync(solutionRoot, new[] { "build", cliProject, "-c", "Debug", "-v", "q" }, environment: null);
            if (build.ExitCode != 0)
            {
                throw new InvalidOperationException($"Failed to build CLI project.\n{build.ToDebugString()}");
            }

            CliBuilt = true;
        }
        finally
        {
            BuildLock.Release();
        }
    }

    private static async Task<CliRunResult> RunDotnetAsync(string workingDirectory, string[] args, IDictionary<string, string?>? environment)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "dotnet",
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
        };

        foreach (var arg in args)
        {
            psi.ArgumentList.Add(arg);
        }

        if (environment != null)
        {
            foreach (var (key, value) in environment)
            {
                psi.Environment[key] = value;
            }
        }

        using var process = Process.Start(psi) ?? throw new InvalidOperationException("Failed to start dotnet process.");

        var stdoutTask = process.StandardOutput.ReadToEndAsync();
        var stderrTask = process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        return new CliRunResult(
            process.ExitCode,
            await stdoutTask,
            await stderrTask);
    }

    private static string FindSolutionRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current != null)
        {
            var sln = Path.Combine(current.FullName, "PwSafeClient.sln");
            if (File.Exists(sln))
            {
                return current.FullName;
            }
            current = current.Parent;
        }

        throw new InvalidOperationException("Could not locate solution root (PwSafeClient.sln).");
    }

    private sealed record CliRunResult(int ExitCode, string Stdout, string Stderr)
    {
        public string ToDebugString() => $"ExitCode: {ExitCode}\nSTDOUT:\n{Stdout}\nSTDERR:\n{Stderr}";
    }

    private sealed class TempDirectory : IDisposable
    {
        public TempDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "PwSafeClient.CLI.UnitTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            try
            {
                Directory.Delete(Path, recursive: true);
            }
            catch
            {
                // ignore cleanup failures
            }
        }
    }
}
