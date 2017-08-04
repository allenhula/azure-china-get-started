$webClientName = "allenlWebClient"
$webClientIdentifierUri = "http://client.allenl.com/"
$webClientHomePage = "http://client.allenl.com/"
$webClientReplyUrl = "http://client.allenl.com/"
$webClientPassword = "Password01!"

$webApiName = "allenlWebApi"
$webApiIdentifierUri = "http://api.allenl.com/"
$webApiHomePage = "http://api.allenl.com/"
$webApiReplyUrl = "http://api.allenl.com/"
$webApiPassword = "Password02!"

Connect-AzureAD -AzureEnvironmentName AzureChinaCloud | Out-Null

# ======== web api start ===========

# create application for web api app
$webApiApp = New-AzureADApplication -DisplayName $webApiName -Homepage $webApiHomePage -IdentifierUris @($webApiIdentifierUri) -ReplyUrls @($webApiReplyUrl)
$webApiAppId = $webApiApp.AppId
$webApiObjectId = $webApiApp.ObjectId

# create client secret with provided password
New-AzureADApplicationPasswordCredential -ObjectId $webApiObjectId -Value $webApiPassword | Out-Null

# create service principal for web api app
New-AzureADServicePrincipal -AppId $webApiAppId -Tags @("WindowsAzureActiveDirectoryIntegratedApp") -AccountEnabled $true | Out-Null

# Web APi expose Private Read delegated permission
$privateReadDelegatedPermissionId = New-Guid
$privateReadDelegatedPermission = [Microsoft.Open.AzureAD.Model.OAuth2Permission]::new()
$privateReadDelegatedPermission.AdminConsentDescription = "Allow the application to read on behalf of the signed-in user."
$privateReadDelegatedPermission.AdminConsentDisplayName = "Private Read"
$privateReadDelegatedPermission.Id = $privateReadDelegatedPermissionId
$privateReadDelegatedPermission.IsEnabled = $true
$privateReadDelegatedPermission.Type = "User"
$privateReadDelegatedPermission.UserConsentDescription = "Allow the application to read on your behalf."
$privateReadDelegatedPermission.UserConsentDisplayName = "Private Read"
$privateReadDelegatedPermission.Value = "PrivateRead"

# Web API expose Public Read application permission
$publicReadApplicationPermissionId = New-Guid
$publicReadApplicationPermission = [Microsoft.Open.AzureAD.Model.AppRole]::new()
$publicReadApplicationPermission.AllowedMemberTypes = @("Application")
$publicReadApplicationPermission.DisplayName = "Public Read"
$publicReadApplicationPermission.Description = "Allow the application to read without user"
$publicReadApplicationPermission.IsEnabled = $true
$publicReadApplicationPermission.Id = $publicReadApplicationPermissionId
$publicReadApplicationPermission.Value = "PublicRead"

# set both permission to expose
Set-AzureADApplication -ObjectId $webApiObjectId -AppRoles @($publicReadApplicationPermission) -Oauth2Permissions @($privateReadDelegatedPermission)

# ======== web api end ===========

# ======== web client start ===========

# create application for web client app
$webClientApp = New-AzureADApplication -DisplayName $webClientName -Homepage $webClientHomePage -IdentifierUris @($webClientIdentifierUri) -ReplyUrls @($webClientReplyUrl)
$webClientAppId = $webClientApp.AppId
$webClientObjectId = $webClientApp.ObjectId

# create client secret with provided password
New-AzureADApplicationPasswordCredential -ObjectId $webClientObjectId -Value $webClientPassword | Out-Null

# create service principal for web client app
New-AzureADServicePrincipal -AppId $webClientAppId -Tags @("WindowsAzureActiveDirectoryIntegratedApp") -AccountEnabled $true | Out-Null

$webApiPrivateReadPermission = [Microsoft.Open.AzureAD.Model.ResourceAccess]::new()
$webApiPrivateReadPermission.Id = $privateReadDelegatedPermissionId
$webApiPrivateReadPermission.Type = "Scope"
$webApiPublicReadPermission = [Microsoft.Open.AzureAD.Model.ResourceAccess]::new()
$webApiPublicReadPermission.Id = $publicReadApplicationPermissionId
$webApiPublicReadPermission.Type = "Role"
$accessWebApiPermission = [Microsoft.Open.AzureAD.Model.RequiredResourceAccess]::new()
$accessWebApiPermission.ResourceAppId = $webApiAppId
$accessWebApiPermission.ResourceAccess = @($webApiPrivateReadPermission, $webApiPublicReadPermission)

# grant permission to access web api
Set-AzureADApplication -ObjectId $webClientObjectId -RequiredResourceAccess @($accessWebApiPermission)

# ======== web client end ===========
