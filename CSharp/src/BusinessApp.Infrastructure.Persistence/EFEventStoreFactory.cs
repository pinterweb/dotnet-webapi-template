using System.Security.Principal;
using BusinessApp.Kernel;

namespace BusinessApp.Infrastructure.Persistence
{
    /// <summary>
    /// Persists events using Entity Framework
    /// </summary>
    public class EFEventStoreFactory : IEventStoreFactory
    {
        private readonly IEntityIdFactory<MetadataId> idFactory;
        private readonly IPrincipal user;
        private readonly BusinessAppDbContext db;

        public EFEventStoreFactory(BusinessAppDbContext db, IEntityIdFactory<MetadataId> idFactory,
            IPrincipal user)
        {
            this.idFactory = idFactory.NotNull().Expect(nameof(idFactory));
            this.db = db.NotNull().Expect(nameof(db));
            this.user = user.NotNull().Expect(nameof(user));
        }

        public IEventStore Create<T>(T eventTrigger) where T : class
        {
            var id = idFactory.Create();
            var metadata = new Metadata<T>(id,
                user.Identity?.Name ?? AnonymousUser.Name,
                MetadataType.EventTrigger,
                eventTrigger);

            _ = db.Add(metadata);

            return new EFEventStore(id, db, idFactory);
        }

        private class EFEventStore : IEventStore
        {
            private readonly BusinessAppDbContext db;
            private readonly IEntityIdFactory<MetadataId> idFactory;
            private readonly MetadataId correlationId;

            public EFEventStore(MetadataId correlationId, BusinessAppDbContext db,
                IEntityIdFactory<MetadataId> idFactory)
            {
                this.db = db;
                this.idFactory = idFactory;
                this.correlationId = correlationId;
            }

            public EventTrackingId Add<T>(T @event) where T : notnull, IDomainEvent
            {
                var eventId = idFactory.Create();
                var id = new EventTrackingId(eventId, correlationId);

                var metadata = new EventMetadata<T>(id, @event);

                _ = db.Add(metadata);

                return id;
            }
        }
    }
}
