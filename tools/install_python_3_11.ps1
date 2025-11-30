Write-Host "Starting Python 3.11 installer attempt script"
$versions = @('3.11.8','3.11.7','3.11.6','3.11.5')
$installer = $null
foreach ($v in $versions) {
    $url = "https://www.python.org/ftp/python/$v/python-$v-amd64.exe"
    Write-Host "Trying $url"
    try {
        $out = Join-Path $env:TEMP "python-$v-amd64.exe"
        Invoke-WebRequest -Uri $url -OutFile $out -UseBasicParsing -ErrorAction Stop
        $installer = $out
        break
    } catch {
        Write-Host "Download failed for $v : $($_.Exception.Message)"
    }
}

if ($installer) {
    Write-Host "Downloaded installer to $installer - attempting silent install (may prompt UAC)..."
    try {
        Start-Process -FilePath $installer -ArgumentList '/quiet','InstallAllUsers=1','PrependPath=1' -Wait -Verb RunAs -ErrorAction Stop
        Write-Host 'Installer process completed.'
    } catch {
        Write-Host "Installer launch failed: $($_.Exception.Message)"
    }
} else {
    Write-Host 'No installer downloaded from tested versions.'
}

try {
    py -3.11 --version
} catch {
    Write-Host 'py -3.11 not present'
}
