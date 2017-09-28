// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Dispatcher;
using Microsoft.AspNetCore.Dispatcher.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace DispatcherSample
{
    public class DataSourceBuilder : DispatcherDataSource
    {
        private List<Address> _addresses;
        private List<Endpoint> _endpoints;

        public DataSourceBuilder()
        {
            _addresses = new List<Address>();
            _endpoints = new List<Endpoint>();
        }

        public ApplicationModel Model { get; } = new ApplicationModel();

        public override IChangeToken ChangeToken => NullChangeToken.Singleton;

        protected override IReadOnlyList<Address> GetAddesses() => _addresses;

        protected override IReadOnlyList<Endpoint> GetEndpoints() => _endpoints;

        public TemplateBuilder ForTemplate(string template)
        {
            return new TemplateBuilder(this, template);
        }

        public DataSourceBuilder AddAddress(AddressModel address)
        {
            Model.Addresses.Add(address);
            return this;
        }

        public DataSourceBuilder AddEndpoint(EndpointModel endpoint)
        {
            Model.Endpoints.Add(endpoint);
            return this;
        }

        public DispatcherDataSource Build()
        {
            // TODO build
            return this;
        }
    }

    public class TemplateBuilder
    {
        public TemplateBuilder(DataSourceBuilder parent, string template)
        {
            Parent = parent;
            Template = template;
        }

        public ApplicationModel Model { get; } = new ApplicationModel();

        protected DataSourceBuilder Parent { get; }

        protected string Template { get; }

        public ValuesBuilder ForValues(object values)
        {
            return new ValuesBuilder(this, new DispatcherValueCollection(values));
        }

        public TemplateBuilder AddAddress(AddressModel address)
        {
            if (address.Template == null)
            {
                address.Template = Template;
            }

            Parent.AddAddress(address);
            return this;
        }

        public TemplateBuilder AddEndpoint(EndpointModel endpoint)
        {
            if (endpoint.Template == null)
            {
                endpoint.Template = Template;
            }

            Parent.AddEndpoint(endpoint);
            return this;
        }

        public DispatcherDataSource Build()
        {
            return Parent.Build();
        }
    }

    public class ValuesBuilder
    {
        public ValuesBuilder(TemplateBuilder parent, DispatcherValueCollection values)
        {
            Parent = parent;
            Values = values;
        }

        public ApplicationModel Model { get; } = new ApplicationModel();

        protected TemplateBuilder Parent { get; }

        protected DispatcherValueCollection Values { get; }

        public DispatcherDataSource Build()
        {
            return Parent.Build();
        }

        public ValuesBuilder AddAddress(AddressModel address)
        {
            foreach (var kvp in Values)
            {
                if (!address.Values.ContainsKey(kvp.Key))
                {
                    address.Values.Add(kvp.Key, kvp.Value);
                }
            }

            Model.Addresses.Add(address);
            return this;
        }

        public ValuesBuilder AddEndpoint(EndpointModel endpoint)
        {
            foreach (var kvp in Values)
            {
                if (!endpoint.Values.ContainsKey(kvp.Key))
                {
                    endpoint.Values.Add(kvp.Key, kvp.Value);
                }
            }

            Model.Endpoints.Add(endpoint);
            return this;
        }
    }

    public static class TemplateBuilderExtensions
    {
        public static TemplateBuilder AddResource(this TemplateBuilder builder, object values, RequestDelegate handler)
        {
            builder.ForValues(values).AddResource(handler, null);
            return builder;
        }

        public static TemplateBuilder AddResource(this TemplateBuilder builder, object values, RequestDelegate handler, string displayName)
        {
            builder.ForValues(values).AddResource(handler, displayName);
            return builder;
        }

        public static TemplateBuilder AddResource(this TemplateBuilder builder, object values, string httpMethod, RequestDelegate handler, string displayName)
        {
            builder.ForValues(values).AddResource(httpMethod, handler, displayName, Array.Empty<object>());
            return builder;
        }

        public static TemplateBuilder AddResource(this TemplateBuilder builder, object values, string httpMethod, RequestDelegate handler, string displayName, params object[] metadata)
        {
            builder.ForValues(values).AddResource(httpMethod, handler, displayName, metadata);
            return builder;
        }
    }

    public static class ValuesBuilderExtensions
    {
        public static ValuesBuilder AddResource(this ValuesBuilder builder, RequestDelegate handler)
        {
            return AddResource(builder, null, handler, null, Array.Empty<object>());
        }

        public static ValuesBuilder AddResource(this ValuesBuilder builder, RequestDelegate handler, string displayName)
        {
            return AddResource(builder, null, handler, displayName, Array.Empty<object>());
        }

        public static ValuesBuilder AddResource(this ValuesBuilder builder, string httpMethod, RequestDelegate handler, string displayName)
        {
            return AddResource(builder, httpMethod, handler, displayName, Array.Empty<object>());
        }

        public static ValuesBuilder AddResource(this ValuesBuilder builder, string httpMethod, RequestDelegate handler, string displayName, params object[] metadata)
        {
            builder.AddAddress(new AddressModel()
            {
                AddressBuilder = AddressBuilder.CreateDefault,
                DisplayName = displayName,
                Tag = handler,
            });

            var endpoint = new EndpointModel()
            {
                DisplayName = displayName,
                EndpointBuilder = EndpointBuilder.CreateDefault,
                Tag = handler,
            };

            if (httpMethod == null)
            {
                endpoint.HttpMethods.Add(httpMethod);
            }

            if (metadata != null)
            {
                for (var i = 0; i < metadata.Length; i++)
                {
                    endpoint.Metadata.Add(metadata[i]);
                }
            }

            builder.AddEndpoint(endpoint);

            return builder;
        }
    }
}