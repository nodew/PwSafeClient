
# PwSafeClient.CLI

`pwsafe` is a .NET global tool that provides a command-line interface for working with Password Safe (`.psafe3`) databases.

## Install

From the packaged tool:

- `dotnet tool install -g PasswordSafe.Cli`

Or from source:

- `dotnet pack -c Release`
- `dotnet tool install -g --add-source ./nupkg PasswordSafe.Cli`

## Configuration

Initialize configuration:

- `pwsafe config init`

The config file is created under your home directory:

- Windows: `%HOMEDRIVE%%HOMEPATH%\.pwsafe\config.json`
- Linux/macOS: `$HOME/.pwsafe/config.json`

You can override the home directory (useful for automation/tests) by setting:

- `PWSAFE_HOME` (Windows/Linux/macOS)

## Global options

- `--no-color`: Disable ANSI colors (useful for logs/CI).
- `--debug`: Show full exception stack traces (default is a short error message).

## Interactive mode

Start an interactive session (prompts once for the database password, then you can run any existing subcommands):

- `pwsafe interactive`
- `pwsafe interactive -a vault`
- `pwsafe interactive -f C:\path\to\vault.psafe3`

The session exits automatically after `idleTime` minutes of no input (configured via `pwsafe config set --idle-time`).

## Non-interactive unlock

Most commands that open a database support non-interactive password input:

- `--password-stdin`: Read the password from stdin
- `--password-env <VAR>`: Read the password from an environment variable

Example:

- `set PWSAFE_PASSWORD=CorrectHorseBatteryStaple!`
- `pwsafe db show -a vault --password-env PWSAFE_PASSWORD`

## Databases

Create an empty database and register it:

- `pwsafe db create C:\path\to\vault.psafe3 -a vault -d`

Add an existing database:

- `pwsafe db add -a vault -f C:\path\to\vault.psafe3`

List databases:

- `pwsafe db list`

Set default database:

- `pwsafe db set -a vault`

Show database info (defaults to the configured default db if no args are provided):

- `pwsafe db show`
- `pwsafe db show -a vault`
- `pwsafe db show -f C:\path\to\vault.psafe3`

Scripting output (some commands):

- `--json`: Output machine-readable JSON
- `--quiet`: Suppress non-essential output

## Entries

List entries:

- `pwsafe entry list` (uses default db)
- `pwsafe entry list -a vault`
- `pwsafe entry list -f C:\path\to\vault.psafe3`

Create an entry:

- `pwsafe entry new -a vault -t "GitHub" -u "me" -p "secret" --url "https://github.com"`

If `--password` is omitted, a password will be generated using the provided options:

- `pwsafe entry new -t "Example" --min-length 16 --max-length 24 --digits --uppercase --lowercase --special`

Show entry details:

- `pwsafe entry show <ID> -a vault`

Copy entry password to clipboard:

- `pwsafe entry get <ID> -a vault`

Clipboard safety options (`entry get` and `policy genpass`):

- `--print`: Print password to stdout (no clipboard)
- `--no-clipboard`: Do not copy to clipboard
- `--clear-after <SECONDS>`: Clear clipboard after N seconds (requires clipboard copy)

Update an entry:

- `pwsafe entry update <ID> -a vault --url "https://example.com"`

Remove an entry:

- `pwsafe entry remove <ID> -a vault`

Renew an entry password (uses the entry's stored policy):

- `pwsafe entry renew <ID> -a vault`

## Password policies

List policies:

- `pwsafe policy list -a vault`

Add/update/remove a policy:

- `pwsafe policy add -a vault -n "Default Policy" --length 20 -d 2 -u 2 -l 2 -s 2`
- `pwsafe policy update -a vault -n "Default Policy" --length 24 -d 2 -u 2 -l 2 -s 2`
- `pwsafe policy rm -a vault "Default Policy"`

Generate a password using a named policy:

- `pwsafe policy genpass -a vault -n "Default Policy"`

## Notes

- Many commands prompt for the database password interactively (unless you use `--password-stdin` / `--password-env`).
- `entry get` copies the password to the clipboard by default.

