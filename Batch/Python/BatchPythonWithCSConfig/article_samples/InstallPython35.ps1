$nl = [Environment]::NewLine
Write-Output "Download python to install...$nl"

$url = "https://www.python.org/ftp/python/3.5.2/python-3.5.2-amd64.exe"
$outFile = "${env:TEMP}\python-3.5.2-amd64.exe"
Write-Output "Downloading $url to $outFile$nl"
Invoke-WebRequest $url -OutFile $outFile

Write-Output "Installing$nl"
Start-Process "$outFile" -ArgumentList "/quiet", "InstallAllUsers=1" -Wait

Write-Output "Update pip and add dependency"
py -m pip install -U pip 
py -m pip install cryptography 
py -m pip install azure-batch
py -m pip install azure-storage
	
Write-Output "Done$nl" 