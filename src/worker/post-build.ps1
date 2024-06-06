$workfolder = [System.IO.Path]::GetDirectoryName( $MyInvocation.MyCommand.Path );
$readMe = [System.IO.Path]::Combine( $workfolder, "README.md" );
$backupReadMe = [System.IO.Path]::Combine( $workfolder, "README.backup.md" );

if ( [System.IO.File]::Exists( $readMe ) -eq $false ) { 
    Write-Output "README file is not found."
    [System.Environment]::ExitCode = 1;
    return 1; 
}
if ( [System.IO.File]::Exists( $backupReadMe ) -eq $false ) { 
    Write-Output "README back up file is not found."
    [System.Environment]::ExitCode = 1;
    return 1; 
}
[System.IO.File]::Copy( $backupReadMe, $readMe, $true ) 
[System.IO.File]::Delete( $backupReadMe ) 