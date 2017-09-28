// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.Dispatcher.Models
{
    public abstract class AddressBuilder
    {
        public ApplicationModel Application { get; set; }
        
        public AddressModel Address { get; set; }

        protected abstract Address BuildCore(object tag, DispatcherValueCollection values, object[] metadata, string displayName);

        public static Address CreateDefault(ApplicationModel application, AddressModel address)
        {
            return Create<Default>(application, address);
        }

        public static Address Create<TBuilder>(ApplicationModel application, AddressModel address)
            where TBuilder : AddressBuilder, new()
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            if (address == null)
            {
                throw new ArgumentNullException(nameof(address));
            }

            var builder = new TBuilder()
            {
                Address = address,
                Application = application,
            };
            return builder.Build();
        }

        public virtual Address Build()
        {
            var metadata = CreateMetadataCollection(Application, Address);

            return BuildCore(Address.Tag, Address.Values, metadata, Address.DisplayName);
        }

        private object[] CreateMetadataCollection(ApplicationModel application, AddressModel address)
        {
            var metadata = new object[application.Metadata.Count + address.Metadata.Count];

            application.Metadata.CopyTo(metadata, 0);
            address.Metadata.CopyTo(metadata, application.Metadata.Count);

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
