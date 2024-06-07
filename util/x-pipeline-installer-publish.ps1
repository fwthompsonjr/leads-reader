param(
    [string]
    $version = "3.2.0"
)

$startedAt = [datetime]::UtcNow
$workfolder = [System.IO.Path]::GetDirectoryName( $MyInvocation.MyCommand.Path );
$rootfolder = [System.IO.Path]::GetDirectoryName( $workfolder );
# _\src\worker\bin\Release\net8.0
$binfolder = [System.IO.Path]::Combine( $workfolder, "src/worker/bin/release/net8.0" );
$project = "legallead.reader.service"
$zipFileName = [string]::Concat($project, "-", $version, ".zip");
$zipFileName = [System.IO.Path]::Combine( $rootfolder, $zipFileName );

if( [System.IO.Directory]::Exists( $binfolder ) -eq $false ) {
    throw "$project script folder is not found!"
}

if( [system.io.file]::Exists( $zipFileName ) -eq $true ) {
    [System.IO.File]::Delete( $zipFileName ) | Out-Null
}
Write-Output "Creating compressed file: ' $project '"

Compress-Archive -Path "$binfolder\*" -DestinationPath $zipFileName -Force

if( [system.io.file]::Exists( $zipFileName ) -eq $false ) {
    Write-Output "Compress $project to zip has failed."
    [Environment]::ExitCode = 1000;
    return 1000;
} else {
    Write-Output "ZIPFILE: $zipFileName is created."
}