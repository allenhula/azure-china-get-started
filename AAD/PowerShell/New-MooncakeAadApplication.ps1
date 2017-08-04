param (
    [Parameter(Mandatory=$true)]
    [string]$DisplayName,
    [Parameter(Mandatory=$true)]
    [string]$HomePage,
    [Parameter(Mandatory=$true)]
    [string[]]$IdentifierUris,
    [Parameter(Mandatory=$true)]
    [string[]]$ReplyUrls,
    [Parameter(Mandatory=$true)]
    [string]$Password
)

# use below command to install AzureAD module if needed
#Install-Module AzureAD

Connect-AzureAD -AzureEnvironmentName AzureChinaCloud | Out-Null

$defaultOAuth2Permission = [Microsoft.Open.AzureAD.Model.OAuth2Permission]::new()
$defaultOAuth2Permission.AdminConsentDescription = "Allow the application to access $DisplayName on behalf of the signed-in user."
$defaultOAuth2Permission.AdminConsentDisplayName = "Access $DisplayName"
$defaultOAuth2Permission.Id = New-Guid
$defaultOAuth2Permission.IsEnabled = $true
$defaultOAuth2Permission.Type = "User"
$defaultOAuth2Permission.UserConsentDescription = "Allow the application to access $DisplayName on your behalf."
$defaultOAuth2Permission.UserConsentDisplayName = "Access $DisplayName"
$defaultOAuth2Permission.Value = "user_impersonation"

$newAadApp = New-AzureADApplication -DisplayName $DisplayName -Homepage $HomePage -IdentifierUris $IdentifierUris -ReplyUrls $ReplyUrls -Oauth2Permissions @($defaultOAuth2Permission)
$appId = $newAadApp.AppId

# create client secret with provided password
New-AzureADApplicationPasswordCredential -ObjectId ($newAadApp.ObjectId) -Value $Password | Out-Null

$newSp = New-AzureADServicePrincipal -AppId $appId -Tags @("WindowsAzureActiveDirectoryIntegratedApp") -AccountEnabled $true
$spId = $newSp.ObjectId

# output
Write-Host ""
Write-Host "Client Name:         $DisplayName" -ForegroundColor Green
Write-Host "Client Id:           $appId" -ForegroundColor Green
Write-Host "Client Secret:       $Password" -ForegroundColor Green
Write-Host "ServicePrincipal Id: $spId" -ForegroundColor Green

