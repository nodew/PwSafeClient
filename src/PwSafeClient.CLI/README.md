# The best Password Safe CLI tool

## Installation

Install as dotnet tool

```bash
dotnet tool install --global PasswordSafe.Cli
```

Or download the latest release from [GitHub Release](https://github.com/nodew/PwSafeClient/releases).

## Configuration

```json
// ~/.pwsafe/config.json
{
    "defaultDb": "<alias>", // The default database to operate if you don't specific the alias or the file path to your pwsafe database in the command line.
    "databases": {
        "<alias>": "The Absolute file path to your PasswordSafe file",
        ...
    },
    "idleTime": 5, // Default to be 5 minutes, which means the interactive window will automatically close in 5 minutes if there's no operation.
    "maxBackupCount": 5 // Default to keep 5 backup files.
}
```

## Commands

```bash
Usage:
  pwsafe [command] [options]

Options:
  --version       Show version information
  -?, -h, --help  Show help and usage information

Commands:
  config           Manage your pwsafe config file
  choose <ALIAS>   Choose a database to operate
  listdb           List all databases
  showdb           Show the detail of PasswordSafe database
  createdb <FILE>  Create an empty new PasswordSafe v3 database file
  list             List the items in database
  add              Add a new entry to the database
  get <ID>         Get the password of an entry
  renew <ID>       Renew the password of an entry
  update <ID>      Update the properties of an entry
  rm <ID>          Remove an entry or group from the database
  policy           Manage your password policies
  unlock           Unlock a database
```

### Manage configuration via CLI

#### 1. Init config

```bash
$ pwsafe config init
```

#### 2. Set alias for a pwsafe database file

```bash
$ pwsafe config set --alias <alias> --file <filepath>
# OR
$ pwsafe config set -a <alias> -f <filepath>
# Make the database as default
$ pwsafe config set -a <alias> -f <filepath> --default
```

#### 3. Remove an alias

```bash
$ pwsafe config rm <alias>
```

#### 4. Set the default database

```bash
$ pwsafe choose <alias>
```

#### 5. List all configured databases

```bash
$ pwsafe listdb
```

### Manage database via CLI

#### 1. Create an empty database

```bash
$ pwsafe createdb <filepath> --alias <alias>
# To make the new database as default database
$ pwsafe createdb <filepath> --alias <alias> --default
# Force to create a new database if file exists, use it on your own risk!
$ pwsafe createdb <filepath> --alias <alias> --force
```

#### 2. Show metadata of an database

```bash
$ pwsafe showdb --alias <alias>
# OR
$ pwsafe showdb --file <filepath>
```

### Manage the entries in database in CLI

#### 1. List available entries

```bash
# List entries of the default database
$ pwsafe list
# List entries of the given alias
$ pwsafe list --alias <alias>
# List entries of the given filepath
$ pwsafe list --file <filepath>
# List entries in tree view
$ pwsafe list --mode tree
# Filter items by title
$ pwsafe list --filter <title>
```

PS. the `alias` option and the `file` option are available for the following sections.

#### 2. Get password

```bash
$ pwsafe get <ID>
Enter your password: ******
Successfully copy password to clipboard
```

#### 3. Add a new entry

```bash
$ pwsafe --title <title> --username <username> --password <password> --group <group> --url <url> --email <email> --notes <notes>
# Read the password interactively
$ pwsafe --title <title> --username <username> --password
# Generate the password via named password policies
$ pwsafe --title <title> --username <username> --policy <policy>
```

PS. the sub-groups are separated by `.`, for example: `A/B/C` is represented as `A.B.C`

#### 4. Renew the password

```bash
$ pwsafe renew <ID>
# Renew with given password
$ pwsafe renew <ID> --password <password>
# Renew with given password interactively
$ pwsafe renew <ID> --password
# Renew the password via named password policies
$ pwsafe renew <ID> --policy <policy>
```

#### 5. Update the properties of an entry

```bash
$ pwsafe update <ID> --title <title> --username <username> --group <group> --url <url> --email <email> --notes <notes>
```

#### 6. Remove an entry or entire group

```bash
# Remove an entry
$ pwsafe rm <ID>
# Remove a group
$ pwsafe rm --group <group>
```

### Manage the password policy

#### 1. List existing password policies

```bash
$ pwsafe policy list
```

#### 2. Add new password policy

```bash
$ pwsafe policy add --name "Sample" \
                    --length 12 \
                    --uppercase 2 \
                    --lowercase 2 \
                    --digits 1 \
                    --symbols 1 \
                    --symbol-chars "@#$%&" \
                    --easy-vision
```

#### 3. Update an existing password policy

```bash
$ pwsafe policy update --name "Sample" \
                       --length 12 \
                       --uppercase 2 \
                       --lowercase 2 \
                       --digits 1 \
                       --symbols=-1 \
                       --easy-vision
```

#### 4. Remove a password policy

```bash
$ pwsafe policy rm --name "Sample"
```

#### 5. Generate password for given policy

```bash
$ pwsafe policy genpass --name "Sample"
```

### Interactive mode

It's boring to enter password for every operation, if you want to do a lot operations, you can unlock the database in interactively mode. The session will automatically exit if you don't take any actions.

```bash
# It will be unlock as read mode by default.
$ pwsafe unlock
# Unlock database in read-write mode
$ pwsafe unlock --read-only false
```