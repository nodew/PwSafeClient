# Use Select-Object and -match to extract version pattern
$version = $args[0] -match '(^[^-]+-)?v([0-9]+\.[0-9]+\.[0-9]+(-[a-zA-Z0-9.]+)?)$' | Select-Object -ExpandProperty Matches | Select-Object -ExpandProperty Groups | Where-Object Name -eq '2'

# Output the extracted version
Write-Host $version
