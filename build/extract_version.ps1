$regex = '(^[^-]+-)?v([0-9]+\.[0-9]+\.[0-9]+(-[a-zA-Z0-9.]+)?)$'
$matchResult = [regex]::Match($args[0], $regex)

if ($matchResult.Success) {
  $match = $matchResult  | Select-Object -ExpandProperty Groups | Where-Object Name -eq '2'
  $version = $match.Value
  Write-Output $version
} else {
  Write-Error "Not match"
}
