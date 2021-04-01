// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     TODO: add comments
    /// </summary>
    public class SqlServerTemporalConvention : IEntityTypeAnnotationChangedConvention, IPropertyAnnotationChangedConvention
    {
        private const string PeriodStartDefaultName = "PeriodStart";
        private const string PeriodEndDefaultName = "PeriodEnd";

        /// <summary>
        ///     TODO: add comments
        /// </summary>
        public virtual void ProcessEntityTypeAnnotationChanged(
            IConventionEntityTypeBuilder entityTypeBuilder,
            string name,
            IConventionAnnotation? annotation,
            IConventionAnnotation? oldAnnotation,
            IConventionContext<IConventionAnnotation> context)
        {
            if (name == SqlServerAnnotationNames.IsTemporal)
            {
                if (annotation?.Value as bool? == true)
                {
                    var periodStartPropertyBuilder = entityTypeBuilder.Property(
                        typeof(DateTime),
                        PeriodStartDefaultName,
                        setTypeConfigurationSource: true, // TODO: true or false here?
                        fromDataAnnotation: false);

                    // should we just ignore here and throw from the validator instead?
                    if (periodStartPropertyBuilder == null)
                    {
                        throw new InvalidOperationException($"Could not configure period start property '{PeriodStartDefaultName}' for entity '{entityTypeBuilder.Metadata.ShortName()}' mapped to temporal table.");
                    }

                    periodStartPropertyBuilder.Metadata.SetAnnotation(SqlServerAnnotationNames.TemporalIsPeriodStart, true);

                    var periodEndPropertyBuilder = entityTypeBuilder.Property(
                        typeof(DateTime),
                        PeriodEndDefaultName,
                        setTypeConfigurationSource: true, // TODO: true or false here?
                        fromDataAnnotation: false);

                    // should we just ignore here and throw from the validator instead?
                    if (periodEndPropertyBuilder == null)
                    {
                        throw new InvalidOperationException($"Could not configure period end property '{PeriodEndDefaultName}' for entity '{entityTypeBuilder.Metadata.ShortName()}' mapped to temporal table.");
                    }

                    periodEndPropertyBuilder.Metadata.SetAnnotation(SqlServerAnnotationNames.TemporalIsPeriodEnd, true);
                }
            }
            else if (oldAnnotation?.Value as bool? == true)
            {
                // TODO: is this correct/necessary? to remove period properties here?
                var periodStartProperty = entityTypeBuilder.Metadata.GetProperties().Where(p => p[SqlServerAnnotationNames.TemporalIsPeriodStart] as bool? == true).SingleOrDefault();
                if (periodStartProperty != null)
                {
                    entityTypeBuilder.Metadata.RemoveProperty(periodStartProperty);
                }

                var periodEndProperty = entityTypeBuilder.Metadata.GetProperties().Where(p => p[SqlServerAnnotationNames.TemporalIsPeriodEnd] as bool? == true).SingleOrDefault();
                if (periodEndProperty != null)
                {
                    entityTypeBuilder.Metadata.RemoveProperty(periodEndProperty);
                }
            }
        }

        /// <summary>
        ///     TODO: add comments
        /// </summary>
        public virtual void ProcessPropertyAnnotationChanged(
            IConventionPropertyBuilder propertyBuilder,
            string name,
            IConventionAnnotation? annotation,
            IConventionAnnotation? oldAnnotation,
            IConventionContext<IConventionAnnotation> context)
        {
            if (name == SqlServerAnnotationNames.TemporalIsPeriodStart
                && annotation?.Value as bool? == true)
            {
                var previousPeriodStartProperty = propertyBuilder.Metadata.DeclaringEntityType.GetProperties()
                    .Where(p => p != propertyBuilder.Metadata && p[SqlServerAnnotationNames.TemporalIsPeriodStart] as bool? == true).SingleOrDefault();

                if (previousPeriodStartProperty != null)
                {
                    propertyBuilder.Metadata.DeclaringEntityType.RemoveProperty(previousPeriodStartProperty);
                }

                // TODO: is this right place?
                //propertyBuilder.ValueGenerated(ValueGenerated.OnAddOrUpdate);
            }

            if (name == SqlServerAnnotationNames.TemporalIsPeriodEnd
                && annotation?.Value as bool? == true)
            {
                var previousPeriodEndProperty = propertyBuilder.Metadata.DeclaringEntityType.GetProperties()
                    .Where(p => p != propertyBuilder.Metadata && p[SqlServerAnnotationNames.TemporalIsPeriodEnd] as bool? == true).SingleOrDefault();

                if (previousPeriodEndProperty != null)
                {
                    propertyBuilder.Metadata.DeclaringEntityType.RemoveProperty(previousPeriodEndProperty);
                }

                //propertyBuilder.ValueGenerated(ValueGenerated.OnAddOrUpdate);
            }
        }
    }
}
