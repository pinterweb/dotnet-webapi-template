using System;
using BusinessApp.Kernel;

namespace BusinessApp.WebApi
{
    /// <summary>
    /// Hateoas links for <see cre="IDomainEvent" />
    /// </summary>
    /// <typeparam name="TRequest">The request type that triggered the event</typeparam>
    /// <typeparam name="TEvent">The event type</typeparam>
    public abstract class HateoasEventLink<TRequest, TEvent> : HateoasLink<TRequest, IDomainEvent>
        where TEvent : IDomainEvent
    {
        public HateoasEventLink(string rel)
            : base(rel)
        { }

        public override Func<TRequest, IDomainEvent, string> RelativeLinkFactory
            => (r, e) => EventRelativeLinkFactory(r, (TEvent)e);

        protected abstract Func<TRequest, TEvent, string> EventRelativeLinkFactory { get; }
    }
}
