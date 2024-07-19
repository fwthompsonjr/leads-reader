<#
    configure component
#>
param(
    [bool]$isServer = $false,
    [bool]$isHeadless = $true,
    [bool]$applyUpdate = $false
)
$workfolder = [System.IO.Path]::GetDirectoryName( $MyInvocation.MyCommand.Path );
$operationsFile = [System.IO.Path]::Combine( $workfolder, "src/worker/_configuration/operation-mode.txt" );
$appSettingsFile = [System.IO.Path]::Combine( $workfolder, "src\worker\appsettings.json" );
if ( [System.IO.File]::Exists( $operationsFile ) -eq $false ) { return; }
if ( [System.IO.File]::Exists( $appSettingsFile ) -eq $false ) { return; }

Write-Host "Configuration is found"
$modeName = "single";
if ($isServer -eq $true) { $modeName = "server"; }
$ops = @{ 
    "mode" = $modeName 
    "headless" = $isHeadless
    } | ConvertTo-Json;

$config = [System.IO.File]::ReadAllText( $appSettingsFile ) | ConvertFrom-Json
if ( $null -eq $config.OperationMode ) { return; }

$config.OperationMode = $modeName;
$settings = ($config | ConvertTo-Json);
if ($applyUpdate -ne $true ) { return; }
Write-Host "Configuration Headless := $isHeadless"
Write-Host "Configuration OperationMode := $modeName"

[System.IO.File]::WriteAllText( $operationsFile, $ops );
[System.IO.File]::WriteAllText( $appSettingsFile, $settings );