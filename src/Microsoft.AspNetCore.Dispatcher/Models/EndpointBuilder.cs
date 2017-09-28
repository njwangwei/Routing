// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Dispatcher.Models
{
    public abstract class EndpointBuilder
    {
        public ApplicationModel Application { get; set; }

        public ResourceModel Resource { get; set; }

        public EndpointModel Endpoint { get; set; }

        protected abstract Endpoint BuildCore(object tag, DispatcherValueCollection values, object[] metadata, string displayName);

        public static Endpoint CreateDefault(ApplicationModel application, ResourceModel resource, EndpointModel endpoint)
        {
            return Create<Default>(application, resource, endpoint);
        }

        public static Endpoint Create<TBuilder>(ApplicationModel application, ResourceModel resource, EndpointModel endpoint)
            where TBuilder : EndpointBuilder, new()
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            if (resource == null)
            {
                throw new ArgumentNullException(nameof(resource));
            }

            if (endpoint == null)
            {
                throw new ArgumentNullException(nameof(endpoint));
            }

            var builder = new TBuilder()
            {
                Endpoint = endpoint,
                Resource = resource,
                Application = application,
            };
            return builder.Build();
        }

        public virtual Endpoint Build()
        {
            var displayName = Endpoint.DisplayName ?? Resource.DisplayName;
            var metadata = CreateMetadataCollection(Application, Resource, Endpoint);
            var tag = Endpoint.Tag ?? Resource.Tag;

            var values = new DispatcherValueCollection(Resource.Values);
            foreach (var kvp in Endpoint.Values)
            {
                values[kvp.Key] = kvp.Value;
            }

            return BuildCore(tag, values, metadata, displayName);
        }

        private object[] CreateMetadataCollection(ApplicationModel model, ResourceModel item, EndpointModel endpoint)
        {
            var metadata = new object[model.Metadata.Count + model.Metadata.Count + endpoint.Metadata.Count];

            var index = 0;
            model.Metadata.CopyTo(metadata, index);

            index += model.Metadata.Count;
            item.Metadata.CopyTo(metadata, index);

            index += item.Metadata.Count;
            endpoint.Metadata.CopyTo(metadata, index);

            return metadata;
        }

        private class Default : EndpointBuilder
        {
            protected override Endpoint BuildCore(object tag, DispatcherValueCollection values, object[] metadata, string displayName)
            {
                if (tag is RequestDelegate appFunc)
                {
                    return new SimpleEndpoint(appFunc, metadata, values, displayName);
                }
                else if (tag is Func<RequestDelegate, RequestDelegate> middleware)
                {
                    return new SimpleEndpoint(middleware, metadata, values, displayName);
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
        }
    }
}