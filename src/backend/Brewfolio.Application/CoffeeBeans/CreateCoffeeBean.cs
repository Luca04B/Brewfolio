using Brewfolio.Domain.CoffeeBeans;
using Brewfolio.Domain.Results;

namespace Brewfolio.Application.CoffeeBeans;

public sealed record CreateCoffeeBeanCommand(
    string Name,
    string Roaster,
    string? Origin,
    RoastLevel RoastLevel,
    string? Description,
    string? ProductUrl,
    CoffeeBagInput? FirstBag,
    CoffeeBeanImageInput? Image = null,
    bool AllowDuplicate = false);

public sealed record CoffeeBeanImageInput(byte[] Content);

public sealed class CreateCoffeeBeanHandler(
    ICoffeeBeanRepository repository,
    TimeProvider timeProvider,
    ICoffeeBeanImageStore? imageStore = null)
{
    public async Task<Result<CoffeeBeanDto>> HandleAsync(
        CreateCoffeeBeanCommand command,
        CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var nameResult = CoffeeBeanName.Parse(command.Name);
        if (nameResult is Error nameError) return nameError;
        if (nameResult is not CoffeeBeanName name) throw new InvalidOperationException();

        var roasterResult = RoasterName.Parse(command.Roaster);
        if (roasterResult is Error roasterError) return roasterError;
        if (roasterResult is not RoasterName roaster) throw new InvalidOperationException();

        if (!command.AllowDuplicate)
        {
            var duplicate = await repository.FindExactAsync(
                name,
                roaster,
                null,
                cancellationToken);
            if (duplicate is not null)
            {
                return Error.Conflict(
                    "coffeeBean.duplicate.possible",
                    $"A Coffee Bean with this name and roaster already exists: {duplicate.Name} / {duplicate.Roaster} ({duplicate.Id}).");
            }
        }

        var result = CoffeeBean.Create(
            name,
            roaster,
            command.Origin,
            command.RoastLevel,
            command.Description,
            command.ProductUrl,
            now);
        if (result is Error error)
        {
            return error;
        }

        if (result is not CoffeeBean coffeeBean)
        {
            throw new InvalidOperationException("Coffee Bean creation returned an unexpected result.");
        }

        if (command.FirstBag is not null)
        {
            var bag = command.FirstBag;
            var bagResult = coffeeBean.AddBag(
                bag.PurchasedOn,
                bag.RoastedOn,
                bag.OpenedOn,
                bag.InitialWeightGrams,
                bag.PricePaid,
                bag.IsInStock,
                DateOnly.FromDateTime(now.UtcDateTime),
                now);
            if (bagResult is Error bagError)
            {
                return bagError;
            }
        }

        string? storedImageKey = null;
        if (command.Image is not null)
        {
            if (imageStore is null)
            {
                return Error.Validation("coffeeBean.image.unavailable", "Image storage is unavailable.");
            }

            var imageResult = await imageStore.StoreAsync(
                coffeeBean.Id,
                command.Image.Content,
                cancellationToken);
            if (imageResult is Error imageError)
            {
                return imageError;
            }

            if (imageResult is not string imageKey)
            {
                throw new InvalidOperationException("Image storage returned an unexpected result.");
            }

            storedImageKey = imageKey;
            coffeeBean.SetImage(storedImageKey, now);
        }

        try
        {
            await repository.AddAsync(coffeeBean, cancellationToken);
        }
        catch
        {
            if (storedImageKey is not null)
            {
                await imageStore!.DeleteAsync(storedImageKey, cancellationToken);
            }

            throw;
        }

        return CoffeeBeanDto.From(coffeeBean, DateOnly.FromDateTime(now.UtcDateTime));
    }
}
