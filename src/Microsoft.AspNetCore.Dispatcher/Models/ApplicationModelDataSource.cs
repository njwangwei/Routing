// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.Dispatcher.Models
{
    public class ApplicationModelDataSource : DispatcherDataSource
    {
        private State _state;
        private bool _initialized;
        private object _lock;
        private Func<State> _initializer;

        private readonly ApplicationModel _application;

        public ApplicationModelDataSource(ApplicationModel application)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            _application = application;
            _initializer = InitializeState;
        }

        public override IChangeToken ChangeToken => NullChangeToken.Singleton;

        protected override IReadOnlyList<Address> GetAddesses()
        {
            var state = LazyInitializer.EnsureInitialized(ref _state, ref _initialized, ref _lock, _initializer);
            return state.Addresses;
        }

        protected override IReadOnlyList<Endpoint> GetEndpoints()
        {
            var state = LazyInitializer.EnsureInitialized(ref _state, ref _initialized, ref _lock, _initializer);
            return state.Endpoints;
        }

        private State InitializeState()
        {
            var application = _application;

            var addresses = new List<Address>();
            var endpoints = new List<Endpoint>();

            for (var i = 0; i < application.Addresses.Count; i++)
            {
                var resource = application.Addresses[i];

                for (var j = 0; j < resource.Addresses.Count; j++)
                {
                    var address = resource.Addresses[j];
                    var builder = address.AddressBuilder ?? resource.AddressBuilder ?? application.AddressBuilder ?? AddressBuilder.CreateDefault;

                    addresses.Add(builder(application, resource, address));
                }

                for (var j = 0; j < resource.Endpoints.Count; j++)
                {
                    var endpoint = resource.Endpoints[j];
                    var builder = endpoint.EndpointBuilder ?? resource.EndpointBuilder ?? application.EndpointBuilder ?? EndpointBuilder.CreateDefault;

                    endpoints.Add(builder(application, resource, endpoint));
                }
            }

            return new State(addresses.ToArray(), endpoints.ToArray());
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

        private class State
        {
            public Address[] Addresses;
            public Endpoint[] Endpoints;

            public State(Address[] addresses, Endpoint[] endpoints)
            {
                Addresses = addresses;
                Endpoints = endpoints;
            }
        }
    }
}
