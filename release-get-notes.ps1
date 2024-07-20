<#
	steps:
	1. get latest version number from release default = param version
	2. get notes
	3. return notes or default
C:\_g\leads-reader\src\worker\z-app-version.json
#>
$changes = @( "## Changes in this release   " );
$defaultChanges = @( "Application update");
function getVersion( $source ) {
    if ( [System.IO.File]::Exists( $source ) -eq $false ) { return $null }
    $content = [System.IO.File]::ReadAllText( $source );
    try
    {
        $js = $content | ConvertFrom-Json
        return $js.Item(0).id;
    }
    catch { return $null; }
}

function getVersionNotes( $source, $nbr ) {
    if ( [System.IO.File]::Exists( $source ) -eq $false ) { return $null }
    if ( $null -eq $nbr ) { return $null }
    $content = [System.IO.File]::ReadAllText( $source );
    try
    {
        $js = $content | ConvertFrom-Json
        foreach( $item in $js )
        {
            if ( $item.id -ne $nbr ) { continue; }
            return $item.notes;
        }
        return $null;
    }
    catch { return $null; }
}
function getChanges($arr)
{
    $details = $defaultChanges;
    if ($null -ne $arr ) { $details = $arr }
    $notes = @( $changes[0] );
    $lineid = 1;
    foreach( $n in $details ) { 
        $tx = [string]::Concat( ($lineid++).ToString(), ". ", $n, "   ");
        $notes += $tx
    }
    $response = [string]::Join( [Environment]::NewLine, $notes );
    return $response;
}

$workfolder = [System.IO.Path]::GetDirectoryName( $MyInvocation.MyCommand.Path );
$versionFile = [System.IO.Path]::Combine( $workfolder, "src/worker/z-app-version.json" );
$versionNoteFile = [System.IO.Path]::Combine( $workfolder, "src/worker/z-app-version-notes.json" );
$version = getVersion -source $versionFile
$versionNotes = getVersionNotes -source $versionNoteFile -nbr $version
$found = getChanges -arr $versionNotes
try {
echo "RELEASE_CHANGES=$found" >> $env:GITHUB_ENV
} catch {}
Write-Output $found;