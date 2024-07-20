$setting = @{
    "email" = "frank.thompson.jr@gmail.com"
    "name" = "Frank Thompson"
}
$locations = @(
    "C:\_g\leads-reader",
    "C:\_g\lui\fwthompsonjr\leads-ui",
    "C:\_d\lead-old"
);
$readOnly = $true
$currentLc = Get-Location
try
{
    $uemail = $setting.email;
    $uname = $setting.name;

    foreach( $loc in $locations )
    {
        Write-Output "User settings for folder $loc";
        Set-Location $loc
        if ($readOnly -eq $false )
        {
            git config user.email $uemail
            git config user.name $uname
        }
        git config --get user.email 
        git config --get user.name
    }
}
finally {
    Set-Location $currentLc
}