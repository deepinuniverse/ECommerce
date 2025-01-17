﻿using System.Net;
using ECommerce.Domain.Exceptions;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace ECommerce.API.Middlewares;

public static class CustomExceptionHandlerMiddlewareExtensions
{
    public static IApplicationBuilder UseCustomExceptionHandler(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CustomExceptionHandlerMiddleware>();
    }
}

public class CustomExceptionHandlerMiddleware(RequestDelegate next,
    IHostingEnvironment env,
    ILogger<CustomExceptionHandlerMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        string message = null;
        var httpStatusCode = HttpStatusCode.InternalServerError;
        var apiStatusCode = ResultCode.Error;

        try
        {
            await next(context);
        }
        catch (AppException exception)
        {
            logger.LogError(exception, exception.Message);
            httpStatusCode = exception.HttpStatusCode;
            apiStatusCode = exception.ApiStatusCode;

            if (env.IsDevelopment())
            {
                var dic = new Dictionary<string, string>
                {
                    ["Exception"] = exception.Message,
                    ["StackTrace"] = exception.StackTrace
                };
                if (exception.InnerException != null)
                {
                    dic.Add("InnerException.Exception", exception.InnerException.Message);
                    dic.Add("InnerException.StackTrace", exception.InnerException.StackTrace);
                }

                if (exception.AdditionalData != null)
                    dic.Add("AdditionalData", JsonConvert.SerializeObject(exception.AdditionalData));

                message = JsonConvert.SerializeObject(dic);
            }
            else
            {
                message = exception.Message;
            }

            await WriteToResponseAsync();
        }
        catch (SecurityTokenExpiredException exception)
        {
            logger.LogError(exception, exception.Message);
            SetUnAuthorizeResponse(exception);
            await WriteToResponseAsync();
        }
        catch (UnauthorizedAccessException exception)
        {
            logger.LogError(exception, exception.Message);
            SetUnAuthorizeResponse(exception);
            await WriteToResponseAsync();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, exception.Message);

            if (env.IsDevelopment())
            {
                var dic = new Dictionary<string, string>
                {
                    ["Exception"] = exception.Message,
                    ["StackTrace"] = exception.StackTrace
                };
                message = JsonConvert.SerializeObject(dic);
            }

            await WriteToResponseAsync();
        }

        async Task WriteToResponseAsync()
        {
            if (context.Response.HasStarted)
                throw new InvalidOperationException(
                    "The response has already started, the http status code middleware will not be executed.");

            var result = new ApiResult { Code = apiStatusCode, Messages = new List<string> { message } };
            var json = JsonConvert.SerializeObject(result);

            context.Response.StatusCode = (int)httpStatusCode;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(json);
        }

        void SetUnAuthorizeResponse(Exception exception)
        {
            httpStatusCode = HttpStatusCode.Unauthorized;
            apiStatusCode = ResultCode.UnAuthorized;

            if (env.IsDevelopment())
            {
                var dic = new Dictionary<string, string>
                {
                    ["Exception"] = exception.Message,
                    ["StackTrace"] = exception.StackTrace
                };
                if (exception is SecurityTokenExpiredException tokenException)
                    dic.Add("Expires", tokenException.Expires.ToString());

                message = JsonConvert.SerializeObject(dic);
            }
        }
    }
}
