﻿// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Security;
using Microsoft.Practices.EnterpriseLibrary.Common.Utility;

namespace Microsoft.Practices.EnterpriseLibrary.Logging
{
    /// <summary>
    /// Represents a performance tracing class to log method entry/exit and duration.    
    /// </summary>
    public class TraceManager
    {
        private readonly LogWriter logWriter;

        /// <summary>
        /// For testing purpose
        /// </summary>
        public LogWriter LogWriter
        {
            get { return this.logWriter; }
        }

        /// <summary>
        /// Create an instance of <see cref="TraceManager"/> giving the <see cref="LogWriter"/>.
        /// </summary>
        /// <param name="logWriter">The <see cref="LogWriter"/> that is used to write trace messages.</param>
        public TraceManager(LogWriter logWriter)
        {
            Guard.ArgumentNotNull(logWriter, "logWriter");

            this.logWriter = logWriter;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tracer"/> class with the given logical operation name.
        /// </summary>
        /// <param name="operation">The operation for the <see cref="Tracer"/></param>
        /// <returns></returns>
        public Tracer StartTrace(string operation)
        {
            return new Tracer(operation, this.logWriter);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tracer"/> class with the given logical operation name and activity id.
        /// </summary>
        /// <param name="operation">The operation for the <see cref="Tracer"/></param>
        /// <param name="activityId">The activity id</param>
        /// <returns></returns>
        public Tracer StartTrace(string operation, Guid activityId)
        {
            return new Tracer(operation, activityId, this.logWriter);
        }
    }
}
