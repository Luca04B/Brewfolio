using Brewfolio.Domain.Results;

namespace Brewfolio.Application.CoffeeBeans;

public sealed class GetCoffeeBeanHandler(ICoffeeBeanRepository repository, TimeProvider timeProvider)
{
    public async Task<Result<CoffeeBeanDto>> HandleAsync(CoffeeBeanId id, CancellationToken cancellationToken)
    {
        var coffeeBean = await repository.GetAsync(id, cancellationToken);
        return coffeeBean is null
            ? Error.NotFound("coffeeBean.notFound", "Coffee Bean was not found.")
            : CoffeeBeanDto.From(
                coffeeBean,
                DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime));
    }
}
