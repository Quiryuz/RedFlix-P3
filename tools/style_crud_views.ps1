$base = Join-Path $PSScriptRoot "..\Views"
$modules = @{
    "usuarios" = @("Usuarios", "Gestion de cuentas y accesos", "Nuevo usuario")
    "Roles" = @("Roles", "Tipos de usuario del sistema", "Nuevo rol")
    "permisos" = @("Permisos", "Permisos del sistema", "Nuevo permiso")
    "perfiles" = @("Perfiles", "Perfiles por cuenta de usuario", "Nuevo perfil")
    "listas" = @("Listas", "Listas personalizadas", "Nueva lista")
    "listaContenidoes" = @("Contenido de listas", "Peliculas y series en listas", "Agregar contenido")
    "favoritos" = @("Favoritos", "Contenido marcado como favorito", "Nuevo favorito")
    "calificaciones" = @("Calificaciones", "Puntajes de peliculas y series", "Nueva calificacion")
    "climas" = @("Clima", "Registros de informacion climatica", "Nuevo registro")
    "cotizaciones" = @("Cotizaciones", "Historial de tipos de cambio", "Nueva cotizacion")
}

foreach ($folder in $modules.Keys) {
    $meta = $modules[$folder]
    $title = $meta[0]
    $subtitle = $meta[1]
    $createText = $meta[2]
    $indexPath = Join-Path $base "$folder\Index.cshtml"
    if (Test-Path $indexPath) {
        $c = Get-Content $indexPath -Raw
        if ($c -notmatch "module-page") {
            $c = $c -replace "(?s)<style>.*?</style>\s*", ""
            $c = $c -replace "<h2>Index</h2>\s*", ""
            $c = $c -replace '(?s)<p>\s*@Html\.ActionLink\("Create New", "Create"\)\s*</p>\s*', ""
            $c = $c -replace 'class="table"', 'class="table module-table"'
            $c = $c -replace '@Html\.ActionLink\("Edit", "Edit"', '@Html.ActionLink("Editar", "Edit"'
            $c = $c -replace '@Html\.ActionLink\("Details", "Details"', '@Html.ActionLink("Detalle", "Details"'
            $c = $c -replace '@Html\.ActionLink\("Delete", "Delete"', '@Html.ActionLink("Eliminar", "Delete"'
            $table = [regex]::Match($c, '(?s)<table.*?</table>').Value
            $header = "@{ ViewBag.ModuleTitle = `"$title`"; ViewBag.ModuleSubtitle = `"$subtitle`"; ViewBag.CreateText = `"$createText`"; }`r`n`r`n@Html.Partial(`"_ModuleHeader`")`r`n`r`n<div class=`"module-card module-card-table`">`r`n    <div class=`"table-responsive`">`r`n"
            $footer = "`r`n    </div>`r`n</div>`r`n"
            $c = [regex]::Replace($c, '(?s)<table.*?</table>', "$header$table$footer", 1)
            Set-Content $indexPath $c -NoNewline
            Write-Host "Updated $indexPath"
        }
    }

    foreach ($action in @("Create","Edit","Details","Delete")) {
        $path = Join-Path $base "$folder\$action.cshtml"
        if (-not (Test-Path $path)) { continue }
        $c = Get-Content $path -Raw
        if ($c -match "module-card") { continue }
        $c = $c -replace "(?s)<style>.*?</style>\s*", ""
        $c = $c -replace "<h2>$action</h2>\s*", ""
        $c = $c -replace "<h4>[^<]+</h4>\s*", ""
        $c = $c -replace 'class="form-horizontal"', 'class="form-horizontal module-form"'
        $c = $c -replace 'value="Create" class="btn btn-default"', 'value="Guardar" class="btn btn-danger"'
        $c = $c -replace 'value="Edit" class="btn btn-default"', 'value="Guardar cambios" class="btn btn-danger"'
        $c = $c -replace 'value="Delete" class="btn btn-default"', 'value="Eliminar" class="btn btn-danger"'
        $c = $c -replace 'class="btn btn-default"', 'class="btn btn-danger"'
        $c = $c -replace '@Html\.ActionLink\("Back to List", "Index"\)', '@Html.ActionLink("Volver al listado", "Index", null, new { @class = "module-back-link" })'
        $c = $c -replace '<dl class="dl-horizontal">', '<dl class="dl-horizontal module-details">'
        $c = $c -replace '<h3>Are you sure you want to delete this\?</h3>', '<p class="module-delete-warning">Estas seguro de que queres eliminar este registro?</p>'
        $pageTitle = switch ($action) { "Create" {"Crear"} "Edit" {"Editar"} "Details" {"Detalle"} "Delete" {"Eliminar"} }
        $showCreate = if ($action -in @("Details","Delete")) { "false" } else { "true" }
        $inject = "@{ ViewBag.ModuleTitle = `"$pageTitle`"; ViewBag.ModuleSubtitle = `"$subtitle`"; ViewBag.ShowCreate = $showCreate; }`r`n`r`n@Html.Partial(`"_ModuleHeader`")`r`n`r`n<div class=`"module-card`">`r`n"
        $c = $c -replace '(@\{\s*ViewBag\.Title[^}]+\})\s*', "`$1`r`n`r`n$inject"
        $footer = "`r`n</div>`r`n<div class=`"module-footer-links`">`r`n    @Html.ActionLink(`"Volver al listado`", `"Index`", null, new { @class = `"module-back-link`" })`r`n</div>`r`n"
        if ($action -eq "Details") {
            $footer = "`r`n</div>`r`n<div class=`"module-footer-links`">`r`n    @Html.ActionLink(`"Editar`", `"Edit`", new { id = Model.ID }, new { @class = `"btn btn-outline-light btn-sm`" })`r`n    @Html.ActionLink(`"Volver al listado`", `"Index`", null, new { @class = `"module-back-link`" })`r`n</div>`r`n"
            $c = $c -replace '(?s)<p>\s*@Html\.ActionLink\("Edit".*?</p>\s*', ""
        }
        $c = $c -replace '(?s)<div>\s*@Html\.ActionLink\("Volver al listado".*?</div>\s*', ""
        $c = $c.TrimEnd() + $footer
        Set-Content $path $c -NoNewline
        Write-Host "Updated $path"
    }
}
