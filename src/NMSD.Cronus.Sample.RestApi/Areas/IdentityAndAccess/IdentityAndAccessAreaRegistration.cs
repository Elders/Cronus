using System.Web.Mvc;

namespace NMSD.Cronus.Sample.RestApi.Areas.IdentityAndAccess
{
    public class IdentityAndAccessAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "IdentityAndAccess";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapHttpRoute(
                "IdentityAndAccess_default",
                "IdentityAndAccess/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}