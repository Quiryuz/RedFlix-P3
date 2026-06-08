$root = Join-Path $PSScriptRoot "..\Views"
Get-ChildItem $root -Recurse -Include Create.cshtml,Edit.cshtml | ForEach-Object {
    $text = [IO.File]::ReadAllText($_.FullName)
    $newText = $text.Replace('ViewBag.ShowCreate = true', 'ViewBag.ShowCreate = false')
    if ($text -ne $newText) {
        [IO.File]::WriteAllText($_.FullName, $newText)
        Write-Host "Fixed $($_.FullName)"
    }
}
