$nl = [Environment]::NewLine
Write-Output "Download python to install...$nl"

$url = "https://devstorage.blob.core.chinacloudapi.cn/files/python-3.5.2-amd64.exe"
$outFile = "${env:TEMP}\python-3.5.2-amd64.exe"
Write-Output "Downloading $url to $outFile$nl"
Invoke-WebRequest $url -OutFile $outFile

Write-Output "Installing$nl"
Start-Process "$outFile" -ArgumentList "/quiet", "InstallAllUsers=1" -Wait

Write-Output "Update pip and add dependency"
py -m pip install -U pip -i http://mirrors.aliyun.com/pypi/simple/ --trusted-host mirrors.aliyun.com 
py -m pip install cryptography -i http://mirrors.aliyun.com/pypi/simple/ --trusted-host mirrors.aliyun.com
py -m pip install azure-batch -i http://mirrors.aliyun.com/pypi/simple/ --trusted-host mirrors.aliyun.com
py -m pip install azure-storage -i http://mirrors.aliyun.com/pypi/simple/ --trusted-host mirrors.aliyun.com
	
Write-Output "Done$nl" 