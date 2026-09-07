using Brewfolio.Domain.CoffeeBeans;
using Brewfolio.Domain.Results;

namespace Brewfolio.Application.CoffeeBeans;

public sealed record UpdateCoffeeBeanCommand(
    string Name,
    string Roaster,
    string? Origin,
    RoastLevel RoastLevel,
    string? Description,
    string? ProductUrl,
    bool AllowDuplicate = false);

public sealed class UpdateCoffeeBeanHandler(ICoffeeBeanRepository repository, TimeProvider timeProvider)
{
    public async Task<Result<CoffeeBeanDto>> HandleAsync(
        CoffeeBeanId id,
        UpdateCoffeeBeanCommand command,
        CancellationToken cancellationToken)
    {
        var bean = await repository.GetAsync(id, cancellationToken);
        if (bean is null) return Error.NotFound("coffeeBean.notFound", "Coffee Bean was not found.");

        var nameResult = CoffeeBeanName.Parse(command.Name);
        if (nameResult is Error nameError) return nameError;
        if (nameResult is not CoffeeBeanName name) throw new InvalidOperationException();

        var roasterResult = RoasterName.Parse(command.Roaster);
        if (roasterResult is Error roasterError) return roasterError;
        if (roasterResult is not RoasterName roaster) throw new InvalidOperationException();

        if (!command.AllowDuplicate)
        {
            var duplicate = await repository.FindExactAsync(name, roaster, id, cancellationToken);
            if (duplicate is not null)
            {
                return Error.Conflict(
                    "coffeeBean.duplicate.possible",
                    $"A Coffee Bean with this name and roaster already exists: {duplicate.Name} / {duplicate.Roaster} ({duplicate.Id}).");
            }
        }
        var result = bean.UpdateProfile(
            name, roaster, command.Origin, command.RoastLevel,
            command.Description, command.ProductUrl, timeProvider.GetUtcNow());
        if (result is Error error) return error;
        await repository.SaveChangesAsync(cancellationToken);
        return CoffeeBeanDto.From(bean, DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime));
    }
}

public sealed class DuplicateCoffeeBeanHandler(
    ICoffeeBeanRepository repository,
    TimeProvider timeProvider)
{
    public async Task<Result<CoffeeBeanDto>> HandleAsync(
        CoffeeBeanId id,
        bool acknowledged,
        CancellationToken cancellationToken)
    {
        if (!acknowledged)
        {
            return Error.Conflict("coffeeBean.duplicate.acknowledgement", "Duplication must be acknowledged.");
        }
        var source = await repository.GetAsync(id, cancellationToken);
        if (source is null) return Error.NotFound("coffeeBean.notFound", "Coffee Bean was not found.");
        var now = timeProvider.GetUtcNow();
        var copyNameResult = CoffeeBeanName.Parse($"{source.Name.Value} (Copy)");
        if (copyNameResult is Error copyNameError) return copyNameError;
        if (copyNameResult is not CoffeeBeanName copyName) throw new InvalidOperationException();
        var result = CoffeeBean.Create(
            copyName, source.Roaster, source.Origin, source.RoastLevel,
            source.Description, source.ProductUrl, now);
        if (result is not CoffeeBean copy) throw new InvalidOperationException();
        await repository.AddAsync(copy, cancellationToken);
        return CoffeeBeanDto.From(copy, DateOnly.FromDateTime(now.UtcDateTime));
    }
}

public sealed class DeleteCoffeeBeanHandler(
    ICoffeeBeanRepository repository,
    ICoffeeBeanImageStore imageStore,
    IImageCleanupQueue cleanupQueue)
{
    public async Task<Result<bool>> HandleAsync(CoffeeBeanId id, CancellationToken cancellationToken)
    {
        var bean = await repository.GetAsync(id, cancellationToken);
        if (bean is null) return Error.NotFound("coffeeBean.notFound", "Coffee Bean was not found.");
        var imageKey = bean.ImageKey;
        await repository.DeleteAsync(bean, cancellationToken);
        if (imageKey is not null)
        {
            try { await imageStore.DeleteAsync(imageKey, cancellationToken); }
            catch { await cleanupQueue.EnqueueAsync(imageKey, cancellationToken); }
        }
        return true;
    }
}

public sealed class ReplaceCoffeeBeanImageHandler(
    ICoffeeBeanRepository repository,
    ICoffeeBeanImageStore imageStore,
    IImageCleanupQueue cleanupQueue,
    TimeProvider timeProvider)
{
    public async Task<Result<CoffeeBeanDto>> HandleAsync(
        CoffeeBeanId id,
        byte[] content,
        CancellationToken cancellationToken)
    {
        var bean = await repository.GetAsync(id, cancellationToken);
        if (bean is null) return Error.NotFound("coffeeBean.notFound", "Coffee Bean was not found.");
        var stored = await imageStore.StoreAsync(id, content, cancellationToken);
        if (stored is Error error) return error;
        if (stored is not string newKey) throw new InvalidOperationException();
        var oldKey = bean.ImageKey;
        bean.SetImage(newKey, timeProvider.GetUtcNow());
        try { await repository.SaveChangesAsync(cancellationToken); }
        catch
        {
            try { await imageStore.DeleteAsync(newKey, cancellationToken); }
            catch { await cleanupQueue.EnqueueAsync(newKey, cancellationToken); }
            throw;
        }
        if (oldKey is not null)
        {
            try { await imageStore.DeleteAsync(oldKey, cancellationToken); }
            catch { await cleanupQueue.EnqueueAsync(oldKey, cancellationToken); }
        }
        return CoffeeBeanDto.From(bean, DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime));
    }
}

public sealed class RemoveCoffeeBeanImageHandler(
    ICoffeeBeanRepository repository,
    ICoffeeBeanImageStore imageStore,
    IImageCleanupQueue cleanupQueue,
    TimeProvider timeProvider)
{
    public async Task<Result<CoffeeBeanDto>> HandleAsync(
        CoffeeBeanId id,
        CancellationToken cancellationToken)
    {
        var bean = await repository.GetAsync(id, cancellationToken);
        if (bean is null) return Error.NotFound("coffeeBean.notFound", "Coffee Bean was not found.");
        var oldKey = bean.RemoveImage(timeProvider.GetUtcNow());
        await repository.SaveChangesAsync(cancellationToken);
        if (oldKey is not null)
        {
            try { await imageStore.DeleteAsync(oldKey, cancellationToken); }
            catch { await cleanupQueue.EnqueueAsync(oldKey, cancellationToken); }
        }
        return CoffeeBeanDto.From(bean, DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime));
    }
}
