Remove-Item -Force -Recurse .\FinalPackages -ErrorAction SilentlyContinue
$finalPackagesDir = md .\FinalPackages
Get-ChildItem -Recurse -Include *.nuspec | %{ `
    $dir = Split-Path $_
    pushd $dir
    $projectFile = Get-ChildItem *.csproj
    nuget pack -OutputDirectory $finalPackagesDir -Symbols -Properties 'Configuration=Release' -Build $projectFile
    popd
}