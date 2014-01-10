using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using NMSD.Cronus.Sample.IdentityAndAccess.Users.Commands;

namespace NMSD.Cronus.Sample.RestApi.Controllers
{
    public class UserController : ApiController
    {

        public void Post(RegisterNewUser command)
        {
            Request.CreateResponse(HttpStatusCode.Accepted);
        }

        public HttpResponseMessage Post(ChangeUserEmail command)
        {
            command.Timestamp = DateTime.Now;
            return Request.CreateResponse(HttpStatusCode.Accepted, command);
        }

    }
}