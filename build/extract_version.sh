#!/bin/bash

# Use sed to extract version pattern
version=$(echo "$1" | sed -E 's/^([^-]+-)?v([0-9]+\.[0-9]+\.[0-9]+(-[a-zA-Z0-9.]+)?)$/\2/')

echo "$version"
