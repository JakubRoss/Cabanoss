﻿using Cabanoss.Core.Exceptions;
using Microsoft.AspNetCore.Http;

namespace Cabanoss.Core.MIddleware
{
    public class ErrorHandlingMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next.Invoke(context);
            }
            catch (ResourceNotFoundException ex)
            {
                context.Response.StatusCode = 404;
                await context.Response.WriteAsync(ex.Message);
            }
            catch (UnauthorizedException  ex) 
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync(ex.Message);
            } 
            catch(ConflictExceptions ex)
            {
                context.Response.StatusCode = 409;
                await context.Response.WriteAsync(ex.Message);
            }

        }
    }
}
