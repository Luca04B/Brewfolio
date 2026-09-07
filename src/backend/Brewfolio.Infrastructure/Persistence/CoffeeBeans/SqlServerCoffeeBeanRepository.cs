using Brewfolio.Application.CoffeeBeans;
using Brewfolio.Domain.CoffeeBeans;
using Brewfolio.Domain.CoffeeBags;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

namespace Brewfolio.Infrastructure.Persistence.CoffeeBeans;

public sealed class SqlServerCoffeeBeanRepository(BrewfolioDbContext dbContext) : ICoffeeBeanRepository
{
    public async Task AddAsync(CoffeeBean coffeeBean, CancellationToken cancellationToken)
    {
        dbContext.CoffeeBeans.Add(coffeeBean);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<CoffeeBeanQueryPage> QueryAsync(
        CoffeeBeanQuery query,
        CancellationToken cancellationToken)
    {
        IQueryable<CoffeeBean> source = query.Search is null
            ? dbContext.CoffeeBeans
            : dbContext.CoffeeBeans.FromSql($$"""
                SELECT *
                FROM [CoffeeBeans]
                WHERE CHARINDEX({{query.Search}}, [Name]) > 0
                   OR CHARINDEX({{query.Search}}, [Roaster]) > 0
                """);
        source = source
            .Include(coffeeBean => coffeeBean.CoffeeBags)
            .AsNoTracking()
            .AsQueryable();
        if (query.IsInStock.HasValue)
        {
            source = query.IsInStock.Value
                ? source.Where(bean => bean.CoffeeBags.Any(bag => bag.IsInStock))
                : source.Where(bean => !bean.CoffeeBags.Any(bag => bag.IsInStock));
        }
        if (query.RoastLevel.HasValue)
        {
            source = source.Where(bean => bean.RoastLevel == query.RoastLevel.Value);
        }

        var matches = await source.ToArrayAsync(cancellationToken);
        var ordered = Order(matches, query.Sort);
        var cursor = DecodeCursor(query.Cursor);
        if (cursor is not null)
        {
            ordered = ordered.Where(bean => IsAfter(bean, query.Sort, cursor));
        }
        var candidates = ordered.Take(query.PageSize + 1).ToArray();
        var items = candidates.Take(query.PageSize).ToArray();
        var nextCursor = candidates.Length > query.PageSize && items.Length > 0
            ? EncodeCursor(items[^1], query.Sort)
            : null;
        return new CoffeeBeanQueryPage(items, nextCursor);
    }

    public Task<CoffeeBean?> GetAsync(CoffeeBeanId id, CancellationToken cancellationToken)
    {
        return dbContext.CoffeeBeans
            .Include(coffeeBean => coffeeBean.CoffeeBags)
            .SingleOrDefaultAsync(coffeeBean => coffeeBean.Id == id, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task AddBagAsync(CoffeeBag coffeeBag, CancellationToken cancellationToken)
    {
        dbContext.CoffeeBags.Add(coffeeBag);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(CoffeeBean coffeeBean, CancellationToken cancellationToken)
    {
        dbContext.CoffeeBeans.Remove(coffeeBean);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private sealed record PageCursor(string Primary, Guid Id);

    private static IEnumerable<CoffeeBean> Order(IEnumerable<CoffeeBean> beans, CoffeeBeanSort sort) =>
        sort switch
        {
            CoffeeBeanSort.Name => beans
                .OrderBy(bean => bean.Name.Value, StringComparer.OrdinalIgnoreCase)
                .ThenBy(bean => bean.Id.Value),
            CoffeeBeanSort.LatestRoastDate => beans
                .OrderByDescending(LatestRoastedOn)
                .ThenByDescending(bean => bean.Id.Value),
            CoffeeBeanSort.LatestActivity => beans
                .OrderByDescending(LatestActivity)
                .ThenByDescending(bean => bean.Id.Value),
            _ => beans.OrderByDescending(bean => bean.CreatedAt).ThenByDescending(bean => bean.Id.Value)
        };

    private static string EncodeCursor(CoffeeBean bean, CoffeeBeanSort sort)
    {
        var primary = Primary(bean, sort);
        return Convert.ToBase64String(
            Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new PageCursor(primary, bean.Id.Value))));
    }

    private static PageCursor? DecodeCursor(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        try
        {
            return JsonSerializer.Deserialize<PageCursor>(
                Encoding.UTF8.GetString(Convert.FromBase64String(value)));
        }
        catch (FormatException) { return null; }
        catch (JsonException) { return null; }
    }

    private static bool IsAfter(CoffeeBean bean, CoffeeBeanSort sort, PageCursor cursor)
    {
        var comparison = string.Compare(
            Primary(bean, sort),
            cursor.Primary,
            sort is CoffeeBeanSort.Name
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal);
        return sort is CoffeeBeanSort.Name
            ? comparison > 0 || comparison == 0 && bean.Id.Value.CompareTo(cursor.Id) > 0
            : comparison < 0 || comparison == 0 && bean.Id.Value.CompareTo(cursor.Id) < 0;
    }

    private static string Primary(CoffeeBean bean, CoffeeBeanSort sort) => sort switch
    {
        CoffeeBeanSort.Name => bean.Name.Value,
        CoffeeBeanSort.LatestRoastDate => LatestRoastedOn(bean)?.ToString("O") ?? string.Empty,
        CoffeeBeanSort.LatestActivity => LatestActivity(bean).ToString("O"),
        _ => bean.CreatedAt.ToString("O")
    };

    private static DateTimeOffset LatestActivity(CoffeeBean bean) =>
        bean.CoffeeBags.Select(bag => bag.UpdatedAt).Append(bean.UpdatedAt).Max();

    private static DateOnly? LatestRoastedOn(CoffeeBean bean) => bean.CoffeeBags.Max(bag => bag.RoastedOn);

    public Task<CoffeeBean?> FindExactAsync(
        CoffeeBeanName name,
        RoasterName roaster,
        CoffeeBeanId? excludingId,
        CancellationToken cancellationToken)
    {
        var source = excludingId is null
            ? dbContext.CoffeeBeans.FromSql($$"""
                SELECT *
                FROM [CoffeeBeans]
                WHERE UPPER([Name]) = UPPER({{name.Value}})
                  AND UPPER([Roaster]) = UPPER({{roaster.Value}})
                """)
            : dbContext.CoffeeBeans.FromSql($$"""
                SELECT *
                FROM [CoffeeBeans]
                WHERE UPPER([Name]) = UPPER({{name.Value}})
                  AND UPPER([Roaster]) = UPPER({{roaster.Value}})
                  AND [Id] <> {{excludingId.Value}}
                """);
        return source.AsNoTracking().FirstOrDefaultAsync(cancellationToken);
    }
}
