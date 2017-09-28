// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Dispatcher.Models
{
    public class EndpointModel
    {
        public string DisplayName { get; set; }

        public Func<ApplicationModel, ResourceModel, EndpointModel, Endpoint> EndpointBuilder { get; set; }

        public IList<string> HttpMethods { get; } = new List<string>();

        public IList<object> Metadata { get; } = new List<object>();

        public object Tag { get; set; }

        public string Template { get; set; }

        public DispatcherValueCollection Values { get; } = new DispatcherValueCollection();
    }
}