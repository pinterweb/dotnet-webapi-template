namespace BusinessApp.Data
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.Domain;
    using BusinessApp.App;
    using System.Security.Principal;

    /// <summary>
    /// Persist the requests
    /// </summary>
    public class EFMetadataStoreRequestDecorator<TRequest, TResponse> :
        IRequestHandler<TRequest, TResponse>
        where TRequest : class
    {
        private readonly BusinessAppDbContext db;
        private readonly IPrincipal user;
        private readonly IRequestHandler<TRequest, TResponse> inner;
        private readonly IEntityIdFactory<MetadataId> idFactory;

        public EFMetadataStoreRequestDecorator(IRequestHandler<TRequest, TResponse> inner,
            IPrincipal user, BusinessAppDbContext db, IEntityIdFactory<MetadataId> idFactory)
        {
            this.user = user.NotNull().Expect(nameof(user));
            this.inner = inner.NotNull().Expect(nameof(inner));
            this.db = db.NotNull().Expect(nameof(inner));
            this.idFactory = idFactory.NotNull().Expect(nameof(idFactory));
        }

        public Task<Result<TResponse, Exception>> HandleAsync(TRequest request,
            CancellationToken cancelToken)
        {
            var eventId = idFactory.Create();
            var metadata = new Metadata<TRequest>(eventId, user.Identity.Name, MetadataType.Request, request);

            db.Add(metadata);

            return inner.HandleAsync(request, cancelToken);
        }
    }
}