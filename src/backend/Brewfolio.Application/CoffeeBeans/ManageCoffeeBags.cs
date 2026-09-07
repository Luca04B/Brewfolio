using Brewfolio.Domain.CoffeeBags;
using Brewfolio.Domain.CoffeeBeans;
using Brewfolio.Domain.Results;

namespace Brewfolio.Application.CoffeeBeans;

public sealed class CreateCoffeeBagHandler(
    ICoffeeBeanRepository repository,
    TimeProvider timeProvider)
{
    public async Task<Result<CoffeeBeanDto>> HandleAsync(
        CoffeeBeanId coffeeBeanId,
        CoffeeBagInput input,
        CancellationToken cancellationToken)
    {
        var coffeeBean = await repository.GetAsync(coffeeBeanId, cancellationToken);
        if (coffeeBean is null)
        {
            return Error.NotFound("coffeeBean.notFound", "Coffee Bean was not found.");
        }

        var now = timeProvider.GetUtcNow();
        var result = coffeeBean.AddBag(
            input.PurchasedOn,
            input.RoastedOn,
            input.OpenedOn,
            input.InitialWeightGrams,
            input.PricePaid,
            input.IsInStock,
            DateOnly.FromDateTime(now.UtcDateTime),
            now);
        if (result is Error error) return error;
        if (result is not CoffeeBag coffeeBag) throw new InvalidOperationException("Unexpected Coffee Bag result.");

        await repository.AddBagAsync(coffeeBag, cancellationToken);
        return CoffeeBeanDto.From(coffeeBean, DateOnly.FromDateTime(now.UtcDateTime));
    }
}

public sealed class ReplaceCoffeeBagHandler(
    ICoffeeBeanRepository repository,
    TimeProvider timeProvider)
{
    public async Task<Result<CoffeeBeanDto>> HandleAsync(
        CoffeeBeanId coffeeBeanId,
        CoffeeBagId coffeeBagId,
        CoffeeBagInput input,
        CancellationToken cancellationToken)
    {
        var coffeeBean = await repository.GetAsync(coffeeBeanId, cancellationToken);
        if (coffeeBean is null) return Error.NotFound("coffeeBean.notFound", "Coffee Bean was not found.");
        var now = timeProvider.GetUtcNow();
        var result = coffeeBean.ReplaceBag(
            coffeeBagId,
            input.PurchasedOn,
            input.RoastedOn,
            input.OpenedOn,
            input.InitialWeightGrams,
            input.PricePaid,
            input.IsInStock,
            DateOnly.FromDateTime(now.UtcDateTime),
            now);
        if (result is Error error) return error;
        await repository.SaveChangesAsync(cancellationToken);
        return CoffeeBeanDto.From(coffeeBean, DateOnly.FromDateTime(now.UtcDateTime));
    }
}

public sealed class OpenCoffeeBagHandler(
    ICoffeeBeanRepository repository,
    TimeProvider timeProvider)
{
    public async Task<Result<CoffeeBeanDto>> HandleAsync(
        CoffeeBeanId coffeeBeanId,
        CoffeeBagId coffeeBagId,
        CancellationToken cancellationToken)
    {
        var coffeeBean = await repository.GetAsync(coffeeBeanId, cancellationToken);
        if (coffeeBean is null) return Error.NotFound("coffeeBean.notFound", "Coffee Bean was not found.");
        var now = timeProvider.GetUtcNow();
        var result = coffeeBean.MarkBagOpened(
            coffeeBagId,
            DateOnly.FromDateTime(now.UtcDateTime),
            now);
        if (result is Error error) return error;
        await repository.SaveChangesAsync(cancellationToken);
        return CoffeeBeanDto.From(coffeeBean, DateOnly.FromDateTime(now.UtcDateTime));
    }
}

public sealed class SetCoffeeBagStockHandler(
    ICoffeeBeanRepository repository,
    TimeProvider timeProvider)
{
    public async Task<Result<CoffeeBeanDto>> HandleAsync(
        CoffeeBeanId coffeeBeanId,
        CoffeeBagId coffeeBagId,
        bool isInStock,
        CancellationToken cancellationToken)
    {
        var coffeeBean = await repository.GetAsync(coffeeBeanId, cancellationToken);
        if (coffeeBean is null) return Error.NotFound("coffeeBean.notFound", "Coffee Bean was not found.");
        var now = timeProvider.GetUtcNow();
        var result = coffeeBean.SetBagStock(coffeeBagId, isInStock, now);
        if (result is Error error) return error;
        await repository.SaveChangesAsync(cancellationToken);
        return CoffeeBeanDto.From(coffeeBean, DateOnly.FromDateTime(now.UtcDateTime));
    }
}

public sealed class DeleteCoffeeBagHandler(
    ICoffeeBeanRepository repository,
    TimeProvider timeProvider)
{
    public async Task<Result<CoffeeBeanDto>> HandleAsync(
        CoffeeBeanId coffeeBeanId,
        CoffeeBagId coffeeBagId,
        CancellationToken cancellationToken)
    {
        var coffeeBean = await repository.GetAsync(coffeeBeanId, cancellationToken);
        if (coffeeBean is null) return Error.NotFound("coffeeBean.notFound", "Coffee Bean was not found.");
        var result = coffeeBean.RemoveBag(coffeeBagId);
        if (result is Error error) return error;
        await repository.SaveChangesAsync(cancellationToken);
        return CoffeeBeanDto.From(
            coffeeBean,
            DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime));
    }
}
