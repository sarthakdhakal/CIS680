using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using NewDotnet.Code;
using NewDotnet.Context;
using NewDotnet.DataLayer;
using NewDotnet.Models;
using System.Net;
using System.Security.Claims;

namespace NewDotnet.Code
{
   /* public class SiteAdminMethod : Attribute, IAuthorizationFilter
    {
        private OODBModel _oodbModel;
        public SiteAdminMethod(OODBModel oodbModel)
        {
            _oodbModel = oodbModel; // Dependency is injected through constructor (register OODBModel in DI container)
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {

            BearerTokenContents tc = Services.GetTokenDataFromUserPrincipal(context.HttpContext.User);
            // do not allow unauthenticated requests
            if (tc == null)
            {
                context.Result = new JsonResult(new { error = 401, message = "Authentication required." })
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized
                };
                return;
            }
               // Administrative role required
            if (_oodbModel.HasUserFlag(tc.StarId, "siteadmin"))
            {
                context.Result = new JsonResult(new { error = 403, message = "Access denied." })
                {
                    StatusCode = (int)HttpStatusCode.Forbidden
                };
                return;
            }

        }
    }
    */
    public class AdminMethod : Attribute, IAuthorizationFilter
    {

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            BearerTokenContents tc = Services.GetTokenDataFromUserPrincipal(context.HttpContext.User);
            if (tc == null)
            {
                context.Result = new JsonResult(new { status = HttpStatusCode.Unauthorized, error = 401, message = "Authentication required." })
                {
                    
                };
                return;
            }
          
          

        }

    }
    public class ApiMethod : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            BearerTokenContents tc = Services.GetTokenDataFromUserPrincipal(context.HttpContext.User);
            if (tc == null)
            {
                context.Result = new JsonResult(new { status = HttpStatusCode.Unauthorized, error = 401, message = "Authentication required." })
                {
                 
                };
                return;
            }

            // Ensure the token type is 'apiKey'
            if (tc.KeyType != "apiKey")
            {
                context.Result = new JsonResult(new { status = HttpStatusCode.Forbidden, error = 403, message = "API access denied." })
                {
                  
                };
                return;
            }
        }
    }

    public class StudentMethod : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            BearerTokenContents tc = Services.GetTokenDataFromUserPrincipal(context.HttpContext.User);
            if (tc == null)
            {
                context.Result = new JsonResult(new {status = HttpStatusCode.Unauthorized, error = 401, message = "Authentication required." })
                {
                    
                };
                return;
            }
        }
    }
    public class GlobalExceptionFilterAttribute : IExceptionFilter
    {
        private readonly OODBModelContext _context;
        public GlobalExceptionFilterAttribute()
        {
            
        }
        public GlobalExceptionFilterAttribute(OODBModelContext context)
        {
            _context = context;
        }

        public void OnException(ExceptionContext context)
        {
            string message;
            string stack;
#if DEBUG
            message = context.Exception.GetBaseException().Message;
            stack = context.Exception.StackTrace;
#else
        message = "An unhandled error occurred.";
        stack = null;
#endif

            var db = new Database(_context);
            if (context.Exception is DbUpdateException dbUpdateEx)
            {
                // Handle the db update exception or throw a custom exception
                message = "Database update failed. Please check the inner exception for details.";
                if (dbUpdateEx.InnerException != null)
                {
                   db.LogAuditEvent("global.exception", "SYSTEM", message);
                }
            }

            context.Result = new ObjectResult(new {status= HttpStatusCode.InternalServerError, error = 500, message })
            {
                
            };

            context.ExceptionHandled = true; // Marking exception as handled
        }

    }
    }

public static class TokenUtilities
{
    public static BearerTokenContents GetTokenDataFromUserPrincipal(ClaimsPrincipal user)
    {
        if (user == null || !user.Identity.IsAuthenticated)
        {
            return null;
        }

        var starIdClaim = user.FindFirst("starId")?.Value;
        if (string.IsNullOrEmpty(starIdClaim))
        {
            return null;
        }

        var expiryClaim = user.FindFirst("expiry")?.Value;
        var guestIdClaim = user.FindFirst("guestId")?.Value;
        var keyTypeClaim = user.FindFirst("keyType")?.Value;
        var playlistClaim = user.FindFirst("playlist")?.Value;

        return new BearerTokenContents
        {
            StarId = starIdClaim,
            Expiry = DateTime.TryParse(expiryClaim, out DateTime expiry) ? expiry : DateTime.Now,
            GuestId = guestIdClaim,
            KeyType = keyTypeClaim,
            Playlist = int.TryParse(playlistClaim, out int playlist) ? playlist : -1
        };
    }
}
