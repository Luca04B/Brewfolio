using Brewfolio.Domain.CoffeeBags;
using Brewfolio.Domain.CoffeeBeans;
using Brewfolio.Domain.Results;

namespace Brewfolio.Application.CoffeeBeans;

public sealed class CreateCoffeeBagHandler(
    ICoffeeBeanRepository repository,
    TimeProvider timeProvider)
{
    public async Task<CoffeeBeanResult<CoffeeBeanDto>> HandleAsync(
        CoffeeBeanId coffeeBeanId,
        CoffeeBagInput input,
        CancellationToken cancellationToken)
    {
        var coffeeBean = await repository.GetAsync(coffeeBeanId, cancellationToken);
        if (coffeeBean is null)
        {
            return new CoffeeBeanError(CoffeeBeanErrorCode.CoffeeBeanNotFound);
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
        if (result is ValidationFailure failure) return failure;
        if (result is not CoffeeBag coffeeBag) throw new InvalidOperationException("Unexpected Coffee Bag result.");

        await repository.AddBagAsync(coffeeBag, cancellationToken);
        return CoffeeBeanDto.From(coffeeBean, DateOnly.FromDateTime(now.UtcDateTime));
    }
}

public sealed class ReplaceCoffeeBagHandler(
    ICoffeeBeanRepository repository,
    TimeProvider timeProvider)
{
    public async Task<CoffeeBeanResult<CoffeeBeanDto>> HandleAsync(
        CoffeeBeanId coffeeBeanId,
        CoffeeBagId coffeeBagId,
        CoffeeBagInput input,
        CancellationToken cancellationToken)
    {
        var coffeeBean = await repository.GetAsync(coffeeBeanId, cancellationToken);
        if (coffeeBean is null) return CoffeeBagErrors.CoffeeBeanNotFound();
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
        if (result is CoffeeBagNotFound) return CoffeeBagErrors.CoffeeBagNotFoundError();
        if (result is ValidationFailure failure) return failure;
        await repository.SaveChangesAsync(cancellationToken);
        return CoffeeBeanDto.From(coffeeBean, DateOnly.FromDateTime(now.UtcDateTime));
    }
}

public sealed class OpenCoffeeBagHandler(
    ICoffeeBeanRepository repository,
    TimeProvider timeProvider)
{
    public async Task<CoffeeBeanResult<CoffeeBeanDto>> HandleAsync(
        CoffeeBeanId coffeeBeanId,
        CoffeeBagId coffeeBagId,
        CancellationToken cancellationToken)
    {
        var coffeeBean = await repository.GetAsync(coffeeBeanId, cancellationToken);
        if (coffeeBean is null) return CoffeeBagErrors.CoffeeBeanNotFound();
        var now = timeProvider.GetUtcNow();
        var result = coffeeBean.MarkBagOpened(
            coffeeBagId,
            DateOnly.FromDateTime(now.UtcDateTime),
            now);
        if (result is CoffeeBagNotFound) return CoffeeBagErrors.CoffeeBagNotFoundError();
        if (result is ValidationFailure failure) return failure;
        await repository.SaveChangesAsync(cancellationToken);
        return CoffeeBeanDto.From(coffeeBean, DateOnly.FromDateTime(now.UtcDateTime));
    }
}

public sealed class SetCoffeeBagStockHandler(
    ICoffeeBeanRepository repository,
    TimeProvider timeProvider)
{
    public async Task<CoffeeBeanResult<CoffeeBeanDto>> HandleAsync(
        CoffeeBeanId coffeeBeanId,
        CoffeeBagId coffeeBagId,
        bool isInStock,
        CancellationToken cancellationToken)
    {
        var coffeeBean = await repository.GetAsync(coffeeBeanId, cancellationToken);
        if (coffeeBean is null) return CoffeeBagErrors.CoffeeBeanNotFound();
        var now = timeProvider.GetUtcNow();
        var result = coffeeBean.SetBagStock(coffeeBagId, isInStock, now);
        if (result is CoffeeBagNotFound) return CoffeeBagErrors.CoffeeBagNotFoundError();
        if (result is ValidationFailure failure) return failure;
        await repository.SaveChangesAsync(cancellationToken);
        return CoffeeBeanDto.From(coffeeBean, DateOnly.FromDateTime(now.UtcDateTime));
    }
}

public sealed class DeleteCoffeeBagHandler(
    ICoffeeBeanRepository repository,
    TimeProvider timeProvider)
{
    public async Task<CoffeeBeanResult<CoffeeBeanDto>> HandleAsync(
        CoffeeBeanId coffeeBeanId,
        CoffeeBagId coffeeBagId,
        CancellationToken cancellationToken)
    {
        var coffeeBean = await repository.GetAsync(coffeeBeanId, cancellationToken);
        if (coffeeBean is null) return CoffeeBagErrors.CoffeeBeanNotFound();
        var result = coffeeBean.RemoveBag(coffeeBagId);
        if (result is CoffeeBagNotFound) return CoffeeBagErrors.CoffeeBagNotFoundError();
        if (result is ValidationFailure failure) return failure;
        await repository.SaveChangesAsync(cancellationToken);
        return CoffeeBeanDto.From(
            coffeeBean,
            DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime));
    }
}

file static class CoffeeBagErrors
{
    public static CoffeeBeanError CoffeeBeanNotFound() =>
        new(CoffeeBeanErrorCode.CoffeeBeanNotFound);

    public static CoffeeBeanError CoffeeBagNotFoundError() =>
        new(CoffeeBeanErrorCode.CoffeeBagNotFound);
}
