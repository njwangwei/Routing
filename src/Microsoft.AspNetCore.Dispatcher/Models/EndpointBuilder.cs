// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Dispatcher.Models
{
    public abstract class EndpointBuilder
    {
        public ApplicationModel Application { get; set; }

        public EndpointModel Endpoint { get; set; }

        protected abstract Endpoint BuildCore(object tag, DispatcherValueCollection values, object[] metadata, string displayName);

        public static Endpoint CreateDefault(ApplicationModel application, EndpointModel endpoint)
        {
            return Create<Default>(application, endpoint);
        }

        public static Endpoint Create<TBuilder>(ApplicationModel application, EndpointModel endpoint)
            where TBuilder : EndpointBuilder, new()
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            if (endpoint == null)
            {
                throw new ArgumentNullException(nameof(endpoint));
            }

            var builder = new TBuilder()
            {
                Endpoint = endpoint,
                Application = application,
            };
            return builder.Build();
        }

        public virtual Endpoint Build()
        {
            var metadata = CreateMetadataCollection(Application, Endpoint);
            return BuildCore(Endpoint.Tag, Endpoint.Values, metadata, Endpoint.DisplayName);
        }

        private object[] CreateMetadataCollection(ApplicationModel application, EndpointModel endpoint)
        {
            var metadata = new object[application.Metadata.Count + endpoint.Metadata.Count];
            
            application.Metadata.CopyTo(metadata, 0);
            endpoint.Metadata.CopyTo(metadata, application.Metadata.Count);

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