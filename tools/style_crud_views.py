import os
import re

BASE = os.path.join(os.path.dirname(__file__), "..", "Views")

MODULES = {
    "usuarios": ("Usuarios", "Gestión de cuentas y accesos", "Nuevo usuario"),
    "Roles": ("Roles", "Tipos de usuario del sistema", "Nuevo rol"),
    "permisos": ("Permisos", "Permisos del sistema", "Nuevo permiso"),
    "perfiles": ("Perfiles", "Perfiles por cuenta de usuario", "Nuevo perfil"),
    "listas": ("Listas", "Listas personalizadas", "Nueva lista"),
    "listaContenidoes": ("Contenido de listas", "Películas y series en listas", "Agregar contenido"),
    "favoritos": ("Favoritos", "Contenido marcado como favorito", "Nuevo favorito"),
    "calificaciones": ("Calificaciones", "Puntajes de películas y series", "Nueva calificación"),
    "climas": ("Clima", "Registros de información climática", "Nuevo registro"),
    "cotizaciones": ("Cotizaciones", "Historial de tipos de cambio", "Nueva cotización"),
}


def strip_inline_styles(text):
    return re.sub(r"<style>.*?</style>\s*", "", text, flags=re.DOTALL)


def wrap_index(folder, title, subtitle, create_text, content):
    content = strip_inline_styles(content)
    content = re.sub(r"<h2>Index</h2>\s*", "", content)
    content = re.sub(
        r"<p>\s*@Html\.ActionLink\(\"Create New\", \"Create\"\)\s*</p>\s*",
        "",
        content,
    )
    content = content.replace('class="table"', 'class="table module-table"')
    content = re.sub(
        r'@Html\.ActionLink\("Edit", "Edit"',
        r'@Html.ActionLink("Editar", "Edit"',
        content,
    )
    content = re.sub(
        r'@Html\.ActionLink\("Details", "Details"',
        r'@Html.ActionLink("Detalle", "Details"',
        content,
    )
    content = re.sub(
        r'@Html\.ActionLink\("Delete", "Delete"',
        r'@Html.ActionLink("Eliminar", "Delete"',
        content,
    )
    content = re.sub(
        r"(<td>\s*)(@Html\.ActionLink\(\"Editar\".*?</td>)",
        r'\1<div class="module-actions">\2</div>',
        content,
        flags=re.DOTALL,
    )

    header = f"""@{{ ViewBag.ModuleTitle = "{title}"; ViewBag.ModuleSubtitle = "{subtitle}"; ViewBag.CreateText = "{create_text}"; }}

<section class="module-page">
    @Html.Partial("_ModuleHeader")

    <div class="module-card module-card-table">
        <div class="table-responsive">
"""

    footer = """
        </div>
    </div>
</section>
"""

    # Insert after first @{ ... } block
    match = re.search(r"@\{[^}]+\}\s*", content, re.DOTALL)
    if not match:
        return content
    pos = match.end()
    body = content[pos:].strip()
    body = re.sub(r"</table>\s*$", "</table>\n        </div>\n    </div>\n</section>", body.strip())
    if "module-page" in body:
        return content
    # Fix: extract table part only
    table_match = re.search(r"<table.*?</table>", body, re.DOTALL)
    if not table_match:
        return content
    return content[:pos] + "\n" + header + table_match.group(0) + "\n        </div>\n    </div>\n</section>\n"


def wrap_form(action, folder, title, subtitle, content):
    content = strip_inline_styles(content)
    action_titles = {
        "Create": ("Crear", "Guardar"),
        "Edit": ("Editar", "Guardar cambios"),
        "Details": ("Detalle", None),
        "Delete": ("Eliminar", "Confirmar eliminación"),
    }
    page_title, submit_text = action_titles[action]

    content = re.sub(r"<h2>" + action + r"</h2>\s*", "", content)
    content = re.sub(r"<h4>[^<]+</h4>\s*", "", content)
    content = content.replace('class="form-horizontal"', 'class="form-horizontal module-form"')
    content = content.replace('value="Create"', 'value="Guardar" class="btn btn-danger"')
    content = content.replace('value="Edit"', 'value="Guardar cambios" class="btn btn-danger"')
    content = content.replace('value="Delete"', 'value="Eliminar" class="btn btn-danger"')
    content = re.sub(
        r'class="btn btn-default"',
        'class="btn btn-danger"',
        content,
    )
    content = re.sub(
        r'@Html\.ActionLink\("Back to List", "Index"\)',
        r'@Html.ActionLink("Volver al listado", "Index", null, new { @class = "btn btn-outline-light btn-sm" })',
        content,
    )
    content = re.sub(
        r'@Html\.ActionLink\("Edit", "Edit", new \{ id = Model\.ID \}\)',
        r'@Html.ActionLink("Editar", "Edit", new { id = Model.ID }, new { @class = "btn btn-outline-light btn-sm" })',
        content,
    )
    content = re.sub(
        r'@Html\.ActionLink\("Back to List", "Index"\) \|',
        r'',
        content,
    )

    show_create = "false" if action in ("Details", "Delete") else "true"
    extra = ""
    if action == "Delete":
        content = content.replace(
            "<h3>Are you sure you want to delete this?</h3>",
            '<p class="module-delete-warning">¿Estás seguro de que querés eliminar este registro?</p>',
        )
        extra = ' ViewBag.ShowCreate = false;'

    if action == "Details":
        content = content.replace('<dl class="dl-horizontal">', '<dl class="dl-horizontal module-details">')
        extra = ' ViewBag.ShowCreate = false;'

    header = f"""@{{ ViewBag.ModuleTitle = "{page_title}"; ViewBag.ModuleSubtitle = "{subtitle}";{extra} }}

<section class="module-page">
    @Html.Partial("_ModuleHeader")

    <div class="module-card">
"""

    footer = """
    </div>
    <div class="module-footer-links">
"""
    if action == "Details":
        footer += """        @Html.ActionLink("Editar", "Edit", new { id = Model.ID }, new { @class = "btn btn-outline-light btn-sm" })
"""
    footer += """        @Html.ActionLink("Volver al listado", "Index", null, new { @class = "module-back-link" })
    </div>
</section>
"""

    match = re.search(r"@\{[^}]+\}\s*", content, re.DOTALL)
    if not match or "module-page" in content:
        return content
    pos = match.end()
    body = content[pos:].strip()
    # Remove trailing back link div for forms (we add footer)
    body = re.sub(r"<div>\s*@Html\.ActionLink\(\"Volver al listado\".*?</div>\s*$", "", body, flags=re.DOTALL)
    body = re.sub(r"<p>\s*@Html\.ActionLink\(\"Editar\".*?</p>\s*$", "", body, flags=re.DOTALL)
    return content[:pos] + "\n" + header + body + footer


def main():
    for folder, (title, subtitle, create_text) in MODULES.items():
        index_path = os.path.join(BASE, folder, "Index.cshtml")
        if os.path.exists(index_path):
            with open(index_path, "r", encoding="utf-8") as f:
                text = f.read()
            new_text = wrap_index(folder, title, subtitle, create_text, text)
            with open(index_path, "w", encoding="utf-8", newline="\r\n") as f:
                f.write(new_text)
            print("Updated", index_path)

        for action in ("Create", "Edit", "Details", "Delete"):
            path = os.path.join(BASE, folder, f"{action}.cshtml")
            if os.path.exists(path):
                with open(path, "r", encoding="utf-8") as f:
                    text = f.read()
                new_text = wrap_form(action, folder, title, subtitle, text)
                with open(path, "w", encoding="utf-8", newline="\r\n") as f:
                    f.write(new_text)
                print("Updated", path)


if __name__ == "__main__":
    main()
