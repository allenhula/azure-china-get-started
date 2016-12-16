# Key Vault Sample
Get-started sample code for Key Vault.

Two samples are included, one is using available .NET SDK, another is using JAVA to invoke REST API as JAVA SDK is not ready yet.

Scenario for this sample:
- Admin get the app URL from Dev
- Admin update corresponding parameters in script KeyVaultDemo.ps1 and run it.
- Admin provide the output as below of script to Dev
-- Target tenant ID is: 123ddad8-66d7-47a8-8f9f-1316152d9587
-- Storage connection string key vault URI: https://kvdemo.vault.azure.cn:443/secrets/stConnectionString/0c35774bac2949199b8ffcff1dbdd500
-- App Client Id is: 123c315c-7a1a-4e0c-9d45-e7764c17446a
-- App Client Key is: Password01!
- Dev update app’s web config with above output values
- Now the app can run successfully to get storage connection string from Key Vault and connect to Storage: