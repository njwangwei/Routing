// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Dispatcher.Models
{
    public class ApplicationModel
    {
        public Func<ApplicationModel, AddressModel, Address> AddressBuilder { get; set; }

        public Func<ApplicationModel, EndpointModel, Endpoint> EndpointBuilder { get; set; }

        public IList<AddressModel> Addresses { get; } = new List<AddressModel>();

        public IList<EndpointModel> Endpoints { get; } = new List<EndpointModel>();

        public IList<object> Metadata { get; } = new List<object>();
    }
}
