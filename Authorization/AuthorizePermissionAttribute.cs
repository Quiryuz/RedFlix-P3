using System.Linq;
using System.Web.Mvc;
using RedFlix.Helpers;

namespace RedFlix.Authorization
{
    public class AuthorizePermissionAttribute : ActionFilterAttribute
    {
        public string Entity { get; set; }
        public string ActionKey { get; set; }
        public bool AllowAnonymous { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var methodAttr = filterContext.ActionDescriptor
                .GetCustomAttributes(typeof(AuthorizePermissionAttribute), false)
                .OfType<AuthorizePermissionAttribute>()
                .FirstOrDefault();

            var classAttr = filterContext.ActionDescriptor.ControllerDescriptor
                .GetCustomAttributes(typeof(AuthorizePermissionAttribute), true)
                .OfType<AuthorizePermissionAttribute>()
                .FirstOrDefault();

            var attr = methodAttr ?? classAttr;

            if (attr != null && attr.AllowAnonymous)
            {
                return;
            }

            var controllerName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
            var actionName = filterContext.ActionDescriptor.ActionName;
            var entidad = (attr != null && !string.IsNullOrEmpty(attr.Entity))
                ? attr.Entity
                : PermissionHelper.MapControllerToEntity(controllerName);
            var accion = (attr != null && !string.IsNullOrEmpty(attr.ActionKey))
                ? attr.ActionKey
                : PermissionHelper.MapMvcActionToPermission(actionName);

            if (!PermissionHelper.TienePermiso(filterContext.HttpContext.Session, entidad, accion))
            {
                filterContext.Result = new RedirectToRouteResult(
                    new System.Web.Routing.RouteValueDictionary(new { controller = "Home", action = "AccessDenied" }));
            }
        }
    }
}
