param(
    [string]
    $version = "3.2.0"
)

$workfolder = [System.IO.Path]::GetDirectoryName( $MyInvocation.MyCommand.Path );
$readmeFile = [System.IO.Path]::Combine( $workfolder, "README.md" );
if( [system.io.file]::Exists( $readmeFile ) -eq $false ) {
    return;
}
$find1 = "## vX.Y.Z";
$find2 = "- ReleaseDate"
$mdContent = [System.IO.File]::ReadAllText( $readmeFile );
$hasChanged = $false;
$dte = [string]::Concat( " ( ", [DateTime]::UtcNow.ToString("s").Replace("T", " "), " )");
if ( $mdContent.IndexOf( $find1 ) -ge 0 ) {
    $mdContent = $mdContent.Replace( $find1, [string]::Concat( "## v", $version ));
    $hasChanged = $true;
}
if ( $mdContent.IndexOf( $find2 ) -ge 0 ) {
    $mdContent = $mdContent.Replace( $find2, $dte );
    $hasChanged = $true;
}

if ($hasChanged -eq $true ) {
    [System.IO.File]::Delete( $readmeFile ) | Out-Null;
    [System.IO.File]::WriteAllText( $readmeFile, $mdContent );
}