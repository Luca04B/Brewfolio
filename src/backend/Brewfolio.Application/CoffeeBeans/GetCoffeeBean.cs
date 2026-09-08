using Brewfolio.Domain.CoffeeBeans;

namespace Brewfolio.Application.CoffeeBeans;

public sealed class GetCoffeeBeanHandler(ICoffeeBeanRepository repository, TimeProvider timeProvider)
{
    public async Task<CoffeeBeanResult<CoffeeBeanDto>> HandleAsync(
        CoffeeBeanId id,
        CancellationToken cancellationToken)
    {
        var coffeeBean = await repository.GetAsync(id, cancellationToken);
        return coffeeBean is null
            ? new CoffeeBeanError(CoffeeBeanErrorCode.CoffeeBeanNotFound)
            : CoffeeBeanDto.From(
                coffeeBean,
                DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime));
    }
}
