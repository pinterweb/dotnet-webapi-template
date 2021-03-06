﻿using System.Linq;
using BusinessApp.Kernel;

namespace BusinessApp.Infrastructure.Persistence
{
    /// <summary>
    /// Filters the query
    /// </summary>
    public class LinqSpecificationQueryVisitor<T> : IQueryVisitor<T>
    {
        private readonly LinqSpecification<T> spec;

        public LinqSpecificationQueryVisitor(LinqSpecification<T> spec)
            => this.spec = spec.NotNull().Expect(nameof(spec));

        public IQueryable<T> Visit(IQueryable<T> queryable) => queryable.Where(spec.Predicate);
    }
}
