function URLEncode {
    param([string]$String)
    $String = [System.Web.HttpUtility]::UrlEncode($String)
    $String = $String.Replace("+", "%20")
    return $String
}

$url = $args[0]
$encodedUrl = URLEncode $url
Write-Host $encodedUrl