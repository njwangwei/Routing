// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.Dispatcher.Models
{
    public abstract class AddressBuilder
    {
        public ApplicationModel Application { get; set; }
        
        public ResourceModel Resource { get; set; }

        public AddressModel Address { get; set; }

        protected abstract Address BuildCore(object tag, DispatcherValueCollection values, object[] metadata, string displayName);

        public static Address CreateDefault(ApplicationModel application, ResourceModel resource, AddressModel address)
        {
            return Create<Default>(application, resource, address);
        }

        public static Address Create<TBuilder>(ApplicationModel application, ResourceModel resource, AddressModel address)
            where TBuilder : AddressBuilder, new()
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            if (resource == null)
            {
                throw new ArgumentNullException(nameof(resource));
            }

            if (address == null)
            {
                throw new ArgumentNullException(nameof(address));
            }

            var builder = new TBuilder()
            {
                Address = address,
                Resource = resource,
                Application = application,
            };
            return builder.Build();
        }

        public virtual Address Build()
        {
            var displayName = Address.DisplayName ?? Resource.DisplayName;
            var metadata = CreateMetadataCollection(Application, Resource, Address);
            var tag = Address.Tag ?? Resource.Tag;

            var values = new DispatcherValueCollection(Resource.Values);
            foreach (var kvp in Address.Values)
            {
                values[kvp.Key] = kvp.Value;
            }

            return BuildCore(tag, values, metadata, displayName);
        }

        private object[] CreateMetadataCollection(ApplicationModel model, ResourceModel item, AddressModel address)
        {
            var metadata = new object[model.Metadata.Count + model.Metadata.Count + address.Metadata.Count];

            var index = 0;
            model.Metadata.CopyTo(metadata, index);

            index += model.Metadata.Count;
            item.Metadata.CopyTo(metadata, index);

            index += item.Metadata.Count;
            address.Metadata.CopyTo(metadata, index);

            return metadata;
        }

        private class Default : AddressBuilder
        {
            protected override Address BuildCore(object tag, DispatcherValueCollection values, object[] metadata, string displayName)
            {
                return new DispatcherValueAddress(values, metadata, displayName);
            }
        }
    }
}
