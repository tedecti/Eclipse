using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Eclipse.Data;

public static class ModelBuilderExtensions
{
    public static void ConfigureHasManyWithOne<TEntity, TRelatedEntity>(
        this ModelBuilder modelBuilder,
        Expression<Func<TEntity, IEnumerable<TRelatedEntity>>> collectionExpression,
        Expression<Func<TRelatedEntity, TEntity>> referenceExpression,
        Expression<Func<TRelatedEntity, object>> foreignKeyExpression,
        DeleteBehavior deleteBehavior = DeleteBehavior.NoAction)
        where TEntity : class
        where TRelatedEntity : class
    {
        modelBuilder.Entity<TEntity>()
            .HasMany(collectionExpression)
            .WithOne(referenceExpression)
            .HasForeignKey(foreignKeyExpression)
            .OnDelete(deleteBehavior);
    }

    public static void ConfigureHasOneWithMany<TEntity, TRelatedEntity>(
        this ModelBuilder modelBuilder,
        Expression<Func<TRelatedEntity, TEntity>> referenceExpression,
        Expression<Func<TEntity, IEnumerable<TRelatedEntity>>> collectionExpression,
        Expression<Func<TRelatedEntity, object>> foreignKeyExpression,
        DeleteBehavior deleteBehavior = DeleteBehavior.NoAction)
        where TEntity : class
        where TRelatedEntity : class
    {
        modelBuilder.Entity<TRelatedEntity>()
            .HasOne(referenceExpression)
            .WithMany(collectionExpression)
            .HasForeignKey(foreignKeyExpression)
            .OnDelete(deleteBehavior);
    }
}