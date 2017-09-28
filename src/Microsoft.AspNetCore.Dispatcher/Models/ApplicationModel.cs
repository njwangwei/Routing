// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Dispatcher.Models
{
    public class ApplicationModel
    {
        public Func<ApplicationModel, ResourceModel, AddressModel, Address> AddressBuilder { get; set; }

        public Func<ApplicationModel, ResourceModel, EndpointModel, Endpoint> EndpointBuilder { get; set; }

        public IList<ResourceModel> Resources { get; } = new List<ResourceModel>();

        public IList<object> Metadata { get; } = new List<object>();
    }
}
