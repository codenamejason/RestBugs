﻿using System;
using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;

namespace RestBugs.Services.Infrastructure
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class DebuggingExceptionFilter : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext actionExecutedContext) {
            var message = actionExecutedContext.Exception.Message;
            var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            response.Content = new StringContent(message);

            actionExecutedContext.Response = response;
        }
    }
}
