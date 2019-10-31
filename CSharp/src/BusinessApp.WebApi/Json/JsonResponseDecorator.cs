﻿namespace BusinessApp.WebApi.Json
{
    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json;
    using System;
    using System.Threading.Tasks;
    using BusinessApp.Domain;
    using System.Threading;

    /// <summary>
    /// Writes out the final response after handling the request
    /// </summary>
    public class JsonResponseDecorator<TRequest, TResponse> : IResourceHandler<TRequest, TResponse>
    {
        private readonly IResourceHandler<TRequest, TResponse> decorated;
        private readonly JsonSerializerSettings jsonSettings;

        public JsonResponseDecorator(IResourceHandler<TRequest, TResponse> decorated,
            JsonSerializerSettings jsonSettings)
        {
            this.decorated = GuardAgainst.Null(decorated, nameof(decorated));
            this.jsonSettings = GuardAgainst.Null(jsonSettings, nameof(jsonSettings));
        }

        public async Task<TResponse> HandleAsync(HttpContext context, CancellationToken cancellationToken)
        {
            try
            {
                var validContentType =
                    !string.IsNullOrWhiteSpace(context.Request.ContentType) &&
                    context.Request.ContentType.Contains("application/json");

                if (context.Request.HasBody() && !validContentType)
                {
                    context.Response.StatusCode = 415;
                    throw new NotSupportedException("Expected content-type to be application/json");
                }

                var resource = await decorated.HandleAsync(context, cancellationToken);
                await WriteResponseAsync(context, resource);

                return resource;
            }
            catch (Exception e)
            {
                await WriteResponseErrorAsync(context, e);
                throw;
            }
        }

        public virtual async Task WriteResponseAsync(HttpContext context, TResponse model)
        {
            GuardAgainst.Null(context, nameof(context));
            context.Response.ContentType = "application/json";

            // check that the response code was not already set
            if (context.Response.StatusCode == 200 &&
                (string.Compare(context.Request.Method, "put", true) == 0 ||
                string.Compare(context.Request.Method, "delete", true) == 0))
            {
                context.Response.StatusCode = StatusCodes.Status204NoContent;
            }
            else if (context.Response.StatusCode == 201 ||
                context.Response.StatusCode == 200 ||
                string.Compare(context.Request.Method, "get", true) == 0)
            {
                await context.Response.WriteAsync(JsonConvert.SerializeObject(model, jsonSettings));
            }
        }

        public virtual Task WriteResponseErrorAsync(HttpContext context, Exception exception)
        {
            GuardAgainst.Null(context, nameof(context));
            GuardAgainst.Null(exception, nameof(exception));

            var model = exception.MapToWebResponse(context);

            context.Response.ContentType = "application/problem+json";
            return context.Response.WriteAsync(JsonConvert.SerializeObject(model, jsonSettings));
        }
    }
}
