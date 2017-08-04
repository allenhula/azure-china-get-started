param (
    [Parameter(Mandatory=$true)]
    [string]$TenantId,
    [Parameter(Mandatory=$true)]
    [string]$DisplayName,
    [Parameter(Mandatory=$true)]
    [string]$HomePage,
    [Parameter(Mandatory=$true)]
    [string[]]$IdentifierUris,
    [Parameter(Mandatory=$true)]
    [string[]]$ReplyUrls,
    [Parameter(Mandatory=$true)]
    [string]$Password,
    [bool]$IsMooncake = $true
)

function Get-AuthToken
{
       param
       (
              [Parameter(Mandatory=$true)]
              [string]$TenantId,
              [bool]$IsMooncake
       )

       $adal = "${env:ProgramFiles(x86)}\Microsoft SDKs\Azure\PowerShell\ServiceManagement\Azure\Services\Microsoft.IdentityModel.Clients.ActiveDirectory.dll"
       $adalforms = "${env:ProgramFiles(x86)}\Microsoft SDKs\Azure\PowerShell\ServiceManagement\Azure\Services\Microsoft.IdentityModel.Clients.ActiveDirectory.WindowsForms.dll"
       [System.Reflection.Assembly]::LoadFrom($adal) | Out-Null
       [System.Reflection.Assembly]::LoadFrom($adalforms) | Out-Null

       $clientId = "1950a258-227b-4e31-a9cf-717495945fc2" 
       $redirectUri = [System.Uri]::new("urn:ietf:wg:oauth:2.0:oob")
       $resourceUri = "https://graph.windows.net/"
       $aadInstance = "https://login.windows.net"

       if($IsMooncake) {
            $resourceUri = "https://graph.chinacloudapi.cn/"
            $aadInstance = "https://login.chinacloudapi.cn"
       }

       $authority = "$aadInstance/$TenantId"
       $authContext = New-Object "Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContext" -ArgumentList $authority
       $promptBehavior = [Microsoft.IdentityModel.Clients.ActiveDirectory.PromptBehavior]::Auto
       $platformParameter = New-Object "Microsoft.IdentityModel.Clients.ActiveDirectory.PlatformParameters" -ArgumentList $promptBehavior
       $authResult = $authContext.AcquireTokenAsync($resourceUri, $clientId, $redirectUri, $platformParameter).Result
       return $authResult.AccessToken
}

$accessToken = Get-AuthToken $TenantId $IsMooncake

$headers = @{
    'Content-Type'='application/json';
    'Authorization'="Bearer $accessToken"
}

$passwordCred = @{
    'keyId'= New-Guid;
    'endDate'=[DateTime]::UtcNow.AddYears(1).ToString('u').Replace(' ', 'T');    
    'startDate'=[DateTime]::UtcNow.ToString('u').Replace(' ', 'T');  
    'value'=$Password
}

$oauth2Permission = @{
    "adminConsentDescription" = "Allow the application to access $DisplayName on behalf of the signed-in user.";
    "adminConsentDisplayName" = "Access $DisplayName";
    "id" = New-Guid;
    "isEnabled" =  $true;
    "type" = "User";
    "userConsentDescription" = "Allow the application to access $DisplayName on your behalf.";
    "userConsentDisplayName" = "Access $DisplayName";
    "value" = "user_impersonation"
}

$payload = @{
    'displayName' = $DisplayName;
    'homepage'= $HomePage;
    'identifierUris'= $IdentifierUris;
    'replyUrls'= $ReplyUrls;
    'passwordCredentials'= @($passwordCred);
    'oauth2Permissions' = @($oauth2Permission)
}
$payloadJson = ConvertTo-Json $payload
$resourceBaseUri = "https://graph.windows.net"
if($IsMooncake) {
    $resourceBaseUri = "https://graph.chinacloudapi.cn"
}
$requestUri = "$resourceBaseUri/$TenantId/applications?api-version=1.6"
$result = Invoke-RestMethod -Uri $requestUri -Headers $headers -Body $payloadJson -Method POST
$appId = $result.appId

$spPayload = @{
    'appId' = $appId;
    'accountEnabled'= $true;
    'tags'= @("WindowsAzureActiveDirectoryIntegratedApp")
}
$spPayloadJson = ConvertTo-Json $spPayload
$spRequestUri = "$resourceBaseUri/$TenantId/servicePrincipals?api-version=1.6"
$spResult = Invoke-RestMethod -Uri $spRequestUri -Headers $headers -Body $spPayloadJson -Method POST
$spId = $spResult.objectId

# output
Write-Host ""
Write-Host "Client Name:         $DisplayName" -ForegroundColor Green
Write-Host "Client Id:           $appId" -ForegroundColor Green
Write-Host "Client Secret:       $Password" -ForegroundColor Green
Write-Host "ServicePrincipal Id: $spId" -ForegroundColor Green

