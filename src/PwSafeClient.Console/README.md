# The best Password Safe CLI tool

## Configuration

```json
// ~/pwsafe.json
{
    "databases": {
        "default": "Default path to your PasswordSafe file"
    }
}
```

## Commands

### Get password

```sh
$ pwsafe --title facebook
Enter your password: ******
Successfully copy password to clipboard

$ pwsafe --alias default --title facebook

$ pwsafe --file /a/b/c/pwsafe.psafe3 --title facebook
```

### Show metadata of a PasswordSafe file

```sh
$ pwsafe showdb

$ pwsafe showdb --alias default

$ pwsafe showdb --file /a/b/c/pwsafe.psafe3

```

### Create a new PasswordSafe file

```sh
$ pwsafe createdb pwsafe --path /a/b/c
```

### List available items in a PasswordSafe file

```sh
$ pwsafe list

$ pwsafe list --alias default

$ pwsafe list --file /a/b/c/pwsafe.psafe3

$ pwsafe list -group # show items in specific group
```

### [TODO] Create new password / Update password


