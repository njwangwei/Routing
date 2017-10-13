// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Dispatcher;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing.Dispatcher;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace DispatcherSample
{
    public class DispatcherOptions2
    {

    }

    public static class RSCE
    {
        public static IServiceCollection AddRoutes(this IServiceCollection services, Action<IDRB> action)
        {
            return services;
        }
    }

    public class IDRB
    {
        private readonly DefaultDispatcherDataSource _data;

        public void MapMvcRoute(string v)
        {
            throw new NotImplementedException();
        }

        internal RouteEntryBuilder MapGet(string template)
        {
            return new RouteEntryBuilder(_data, template, "GET");
        }

        internal RouteEntryBuilder MapRoute(string v)
        {
            throw new NotImplementedException();
        }
    }

    public class RouteEntryBuilder
    {
        private DefaultDispatcherDataSource _data;
        private string _template;
        private string _httpMethod;

        public RouteEntryBuilder(DefaultDispatcherDataSource data, string template, string method)
        {
            _data = data;
            _template = template;
            _httpMethod = method;
        }

        public IList<object> Metadata { get; }

        internal void Executes(RequestDelegate action)
        {
            _data.Endpoints.Add(new TemplateEndpoint(_template, _httpMethod, action, Metadata.ToArray()));
        }
    }

    public static class AuthREBExtensions
    {
        public static RouteEntryBuilder WithAuthPolicy(this RouteEntryBuilder builder, string policy)
        {
            builder.Metadata.Add(new AuthorizationPolicyMetadata(policy));
            return builder;
        }
    }


    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRoutes(routes => 
            {
                routes.MapMvcRoute("{controller=Home}/{action=Index}/{id?}");

                routes
                    .MapGet("/authenticate")
                    .WithAuthPolicy("members-only-VIP")
                    .Executes(c => c.Response.WriteAsync("heyoooo"));

                routes.MapRoute("/foo")
                      .Executes(c => c.Response.WriteAsync("foo"));
            });
        }

        #region LOL
        public DefaultDispatcherDataSource ConfigureDispatcher()
        {
            return new DefaultDispatcherDataSource()
            {
                Addresses =
                {
                    new TemplateAddress("{id?}", new { controller = "Home", action = "Index", }, "Home:Index()"),
                    new TemplateAddress("Home/About/{id?}", new { controller = "Home", action = "About", }, "Home:About()"),
                    new TemplateAddress("Admin/Index/{id?}", new { controller = "Admin", action = "Index", }, "Admin:Index()"),
                    new TemplateAddress("Admin/Users/{id?}", new { controller = "Admin", action = "Users", }, "Admin:GetUsers()/Admin:EditUsers()"),
                },
                Endpoints =
                {
                    new TemplateEndpoint("{id?}", new { controller = "Home", action = "Index", }, Home_Index, "Home:Index()"),
                    new TemplateEndpoint("Home/{id?}", new { controller = "Home", action = "Index", }, Home_Index, "Home:Index()"),
                    new TemplateEndpoint("Home/Index/{id?}", new { controller = "Home", action = "Index", }, Home_Index, "Home:Index()"),
                    new TemplateEndpoint("Home/About/{id?}", new { controller = "Home", action = "About", }, Home_About, "Home:About()"),
                    new TemplateEndpoint("Admin/Index/{id?}", new { controller = "Admin", action = "Index", }, Admin_Index, "Admin:Index()"),
                    new TemplateEndpoint("Admin/Users/{id?}", new { controller = "Admin", action = "Users", }, "GET", Admin_GetUsers, "Admin:GetUsers()", new AuthorizationPolicyMetadata("Admin")),
                    new TemplateEndpoint("Admin/Users/{id?}", new { controller = "Admin", action = "Users", }, "POST", Admin_EditUsers, "Admin:EditUsers()", new AuthorizationPolicyMetadata("Admin")),
                },
            };
        }

        #endregion

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILogger<Startup> logger)
        {
            app.UseDispatcher();

            app.Use(async (context, next) =>
            {
                logger.LogInformation("Executing fake CORS middleware");

                var feature = context.Features.Get<IDispatcherFeature>();
                var policy = feature.Endpoint?.Metadata.OfType<ICorsPolicyMetadata>().LastOrDefault();
                logger.LogInformation("using CORS policy {PolicyName}", policy?.Name ?? "default");

                await next.Invoke();
            });

            app.Use(async (context, next) =>
            {
                logger.LogInformation("Executing fake AuthZ middleware");

                var feature = context.Features.Get<IDispatcherFeature>();
                var policy = feature.Endpoint?.Metadata.OfType<IAuthorizationPolicyMetadata>().LastOrDefault();
                if (policy != null)
                {
                    logger.LogInformation("using Auth policy {PolicyName}", policy.Name);
                }

                await next.Invoke();
            });
        }

        #region LOLOL

        public static Task Home_Index(HttpContext httpContext)
        {
            var url = httpContext.RequestServices.GetService<RouteTemplateUrlGenerator>();
            return httpContext.Response.WriteAsync(
                $"<html>" +
                $"<body>" +
                $"<h1>Some links you can visit</h1>" +
                $"<p><a href=\"{url.GenerateUrl(httpContext, new { controller = "Home", action = "Index", })}\">Home:Index()</a></p>" +
                $"<p><a href=\"{url.GenerateUrl(httpContext, new { controller = "Home", action = "About", })}\">Home:About()</a></p>" +
                $"<p><a href=\"{url.GenerateUrl(httpContext, new { controller = "Admin", action = "Index", })}\">Admin:Index()</a></p>" +
                $"<p><a href=\"{url.GenerateUrl(httpContext, new { controller = "Admin", action = "Users", })}\">Admin:GetUsers()/Admin:EditUsers()</a></p>" +
                $"</body>" +
                $"</html>");
        }

        public static Task Home_About(HttpContext httpContext)
        {
            return httpContext.Response.WriteAsync(
                $"<html>" +
                $"<body>" +
                $"<p>This is a dispatcher sample.</p>" +
                $"</body>" +
                $"</html>");
        }

        public static Task Admin_Index(HttpContext httpContext)
        {
            return httpContext.Response.WriteAsync(
                $"<html>" +
                $"<body>" +
                $"<p>This is the admin page.</p>" +
                $"</body>" +
                $"</html>");
        }

        public static Task Admin_GetUsers(HttpContext httpContext)
        {
            return httpContext.Response.WriteAsync(
                $"<html>" +
                $"<body>" +
                $"<p>Users: rynowak, jbagga</p>" +
                $"</body>" +
                $"</html>");
        }

        public static Task Admin_EditUsers(HttpContext httpContext)
        {
            return httpContext.Response.WriteAsync(
                $"<html>" +
                $"<body>" +
                $"<p>blerp</p>" +
                $"</body>" +
                $"</html>");
        }

#endregion
    }
}
