$workfolder = [System.IO.Path]::GetDirectoryName( $MyInvocation.MyCommand.Path );
$projectFile = [System.IO.Path]::Combine( $workfolder, "legallead.reader.service.csproj" );
$readMe = [System.IO.Path]::Combine( $workfolder, "README.md" );
$jsonReadMe = [System.IO.Path]::Combine( $workfolder, "z-app-version.json" );
$backupReadMe = [System.IO.Path]::Combine( $workfolder, "README.backup.md" );
$releaseNotesFile = [System.IO.Path]::Combine( $workfolder, "RELEASE-NOTES.txt" );
$versionLine = '| x.y.z | {{ date }} | {{ description }} |'

function getVersionNumber {
    if ( [System.IO.File]::Exists( $jsonReadMe ) -eq $false ) { 
        return "1.0.0"; 
    }
    $jscontent = [System.IO.File]::ReadAllText( $jsonReadMe ) | ConvertFrom-Json
    return $jscontent.Item(0).id
    
}

function updateReadMe() {
    if ( [System.IO.File]::Exists( $readMe ) -eq $false ) { 
        Write-Output "Source file is not found."
        [System.Environment]::ExitCode = 1;
        return 1; 
    }
    if ( [System.IO.File]::Exists( $jsonReadMe ) -eq $false ) { 
        Write-Output "JSON file is not found."
        [System.Environment]::ExitCode = 1;
        return 1; 
    }
    [System.IO.File]::Copy( $readMe, $backupReadMe, $true ) 
    $details = @();
    $jscontent = [System.IO.File]::ReadAllText( $jsonReadMe ) | ConvertFrom-Json
    $jscontent.GetEnumerator() | ForEach-Object {
        $item = $_;
        $index = $item.id;
        $description = $item.description;
        $dte = $item.date;
        $details += ("| $index | $dte | $description |   ")
    }
    if ($details.Length -eq 0 ) { return; }

    $rmtransform = [string]::Join( [Environment]::NewLine, $details );
    $rmcontent = [System.IO.File]::ReadAllText( $readMe )
    $rmcontent = $rmcontent.Replace( $versionLine, $rmtransform )
    [System.IO.File]::Delete( $readMe ) 
    [System.IO.File]::WriteAllText( $readMe, $rmcontent );
}

function getReleaseNotes() {
    try {
        $tb = "     ";
        $dashes = "$tb - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - "
        $txt = [System.IO.File]::ReadAllText( $jsonReadMe ) | ConvertFrom-Json
        $dat = @("", "$tb legallead.reader.service", $dashes);
        $txt.GetEnumerator() | ForEach-Object {
            $nde = $_;
            $ln = "$tb : $($nde.id) - $($nde.date) - $($nde.description)"
            $dat += $ln
        }
        $dat += $dashes
        $dat += ""
        $detail = [string]::Join( [Environment]::NewLine, $dat)
        $detail += "    "
        return $detail
    } catch {
        return $null
    }
}

function updateVersionNumbers() {

    if ( [System.IO.File]::Exists( $projectFile ) -eq $false ) { 
        Write-Output "Source file is not found."
        [System.Environment]::ExitCode = 1;
        return 1; 
    }
    $hasXmlChange = $false;
    $nodeNames = @( 'AssemblyVersion', 'FileVersion', 'Version' );
    $versionStamp = getVersionNumber
    [xml]$prj = [System.IO.File]::ReadAllText( $projectFile );
    $nde = $prj.DocumentElement.SelectSingleNode('PropertyGroup')
    if ( $nde -eq $null ) { return; }
    $nde.ChildNodes | ForEach-Object {
        [System.Xml.XmlNode]$child = $_;
        $nodeName = $child.Name;
        $nodeValue = $child.InnerText;
        if ( $nodeNames.Contains( $nodeName ) -eq $true -and $nodeValue.Equals($versionStamp) -eq $false ) {
            $hasXmlChange = $true;
            $child.InnerText = $versionStamp;
        }
    }
    if ($hasXmlChange -eq $true ) {
        $prj.Save( $projectFile );
    }
}

function updateReleaseTextFile( $text, $destination )
{
    if ( [System.IO.File]::Exists( $destination ) -eq $false ) { return; }
    $tempName = [string]([System.IO.Path]::GetFileName( $destination )).Replace(".txt", ".backup.txt");
    $sourcePath = [System.IO.Path]::GetDirectoryName( $destination );
    $tempFile = [System.IO.Path]::Combine( $sourcePath, $tempName );
    if ( [System.IO.File]::Exists( $tempFile ) -eq $true ) { 
        [System.IO.File]::Delete( $tempFile ) | Out-Null;
    }
    [System.IO.File]::Copy( $destination, $tempFile, $true ) | Out-Null;
    try {
        $actualText = [System.IO.File]::ReadAllText( $destination );
        if ( $actualText -eq $text ) { return; }
        [System.IO.File]::WriteAllText( $destination, $text );
    }
    finally {
        if ( [System.IO.File]::Exists( $tempFile ) -eq $true ) { 
            [System.IO.File]::Delete( $tempFile ) | Out-Null;
        }
    }
}

$tmp = getReleaseNotes

updateVersionNumbers
updateReadMe
updateReleaseTextFile -text $tmp -destination $releaseNotesFile