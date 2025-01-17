﻿using System.Runtime.Remoting.Metadata.W3cXsd2001;
using FluentNHibernate.Mapping;
using IRuettae.Persistence.Entities;

namespace IRuettae.Persistence.Mappings
{
    public class RouteCalculationMap : ClassMap<RouteCalculation>
    {
        public RouteCalculationMap()
        {
            Id(x => x.Id);
            Map(x => x.Year);
            Map(x => x.Days);
            Map(x => x.MaxNumberOfAdditionalSantas);
            Map(x => x.TimePerChildMinutes);
            Map(x => x.TimePerChildOffsetMinutes);
            Map(x => x.StarterVisitId);
            Map(x => x.TimeLimitMiliseconds);

            Map(x => x.NumberOfSantas);
            Map(x => x.NumberOfVisits);
            Map(x => x.SantaJson).CustomSqlType("LONGTEXT");
            Map(x => x.VisitsJson).CustomSqlType("LONGTEXT");

            Map(x => x.Algorithm);
            Map(x => x.AlgorithmData).CustomSqlType("LONGTEXT");

            Map(x => x.Result).CustomSqlType("LONGTEXT");
            Map(x => x.State);
            Map(x => x.Progress);
            HasMany(x => x.StateText).KeyColumn("routecalculation_id").Not.LazyLoad().Cascade.AllDeleteOrphan();
            Map(x => x.EndTime);
            Map(x => x.StartTime);
        }
    }
}
