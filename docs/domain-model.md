# Domain model

This document captures Brewfolio's current domain shape. It is intentionally small and will evolve through concrete user scenarios rather than speculative fields.

## Coffee Bean

A `CoffeeBean` represents one coffee product in a Member's current collection.

Initial information:

- `Id`
- `Name`
- `Roaster`
- `Origin`
- `RoastLevel`
- `Price`, stored as a decimal amount in euros
- `IsInStock`

`Price` always means euros in the MVP; Brewfolio has no currency field or currency conversion. `IsInStock` is deliberately a Boolean. Brewfolio does not yet track quantities, individual bags, purchases, or stock history.

Creating a Coffee Bean returns `Result<CoffeeBean>`. Valid input produces the Entity; invalid input produces a structured `ValidationFailure` containing all independent validation errors. The result is implemented as a native C# 15 preview union so callers must distinguish the expected outcomes explicitly.

A roast date is not part of `CoffeeBean`. If Brewfolio later needs to distinguish repeated purchases of the same coffee, a separate concept such as `CoffeeBag` or `CoffeeStockItem` can own purchase-specific price, roast date, quantity, and availability.

## Recipe

A `Recipe` describes the intended parameters for preparing coffee.

Initial information:

- `Id`
- `Name`
- `BrewingMethodId`
- `CoffeeAmountInGrams`
- `WaterAmountInGrams`
- `WaterTemperatureInCelsius`
- `GrindSize`
- `TargetBrewTime`, represented as a duration rather than a clock time

A Recipe is independent of a Coffee Bean, references one configurable Brewing Method, and can be reused for multiple Coffee Brews. This allows different Members to follow the same Recipe with their own Coffee Beans.

## Brewing Method

A `BrewingMethod` represents a preparation method such as V60, AeroPress, or French Press from a shared, centrally managed catalog.

Initial information:

- `Id`
- `Name`
- `IsActive`

Names must be unique when Brewing Methods are stored. An unused Brewing Method may be deleted. Once referenced by a Recipe, it must be preserved; deactivation prevents selection for new Recipes while keeping existing references, and reactivation makes it selectable again.

## Coffee Brew

A `CoffeeBrew` records one completed preparation.

Initial information:

- `Id`
- `CoffeeBeanId`
- `RecipeId`
- `BrewedAt`
- `Rating`
- `Notes`

`BrewedAt` is the date and time of the preparation. `Rating` is an optional assessment of that specific preparation and ranges from 1 to 5. A future `ActualBrewTime` may record the measured duration separately from the Recipe's `TargetBrewTime`.

## Relationships

```text
Coffee Bean ─────┐
                 ├──> Coffee Brew
Recipe ──────────┘

Brewing Method ──> used by many Recipes
Recipe ──> reused by many Coffee Brews
```

## Future community direction

After authentication and ownership exist, Members will be able to publish Recipes for other Members to prepare. Public Recipe Scores and rankings will be derived from ratings on the resulting Coffee Brews. Publication lifecycle, rating eligibility, weighting, and ranking rules will be designed with that vertical slice rather than added to the current entities in advance.

The first vertical slice creates and lists Coffee Beans. Recipes and Coffee Brews follow after that foundation is persistent and tested.
