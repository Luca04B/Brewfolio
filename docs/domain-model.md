# Domain model

This document captures Brewfolio's current domain shape. It is intentionally small and will evolve through concrete user scenarios rather than speculative fields.

## Coffee Bean

A `CoffeeBean` represents one coffee product in the user's current collection.

Initial information:

- `Id`
- `Name`
- `Roaster`
- `Origin`
- `RoastLevel`
- `Price`, stored as a decimal amount in euros
- `IsInStock`

`Price` always means euros in the MVP; Brewfolio has no currency field or currency conversion. `IsInStock` is deliberately a Boolean. Brewfolio does not yet track quantities, individual bags, purchases, or stock history.

Creating a Coffee Bean returns `Result<CoffeeBean>`. Valid input produces the Entity; missing required text or a negative price produces a structured validation `Error`. The result is implemented as a native C# 15 preview union so callers must distinguish the expected outcomes explicitly.

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

A Recipe is independent of a Coffee Bean, references one configurable Brewing Method, and can be reused for multiple Coffee Brews.

## Brewing Method

A `BrewingMethod` represents a configurable preparation method such as V60, AeroPress, or French Press.

Initial information:

- `Id`
- `Name`
- `IsActive`

Names must be unique when Brewing Methods are stored. Deactivation prevents selection for new Recipes while preserving existing Recipe references.

## Coffee Brew

A `CoffeeBrew` records one completed preparation.

Initial information:

- `Id`
- `CoffeeBeanId`
- `RecipeId`
- `BrewedAt`
- `Rating`
- `Notes`

`BrewedAt` is the date and time of the preparation. `Rating` is optional and ranges from 1 to 5. A future `ActualBrewTime` may record the measured duration separately from the Recipe's `TargetBrewTime`.

## Relationships

```text
Coffee Bean ─────┐
                 ├──> Coffee Brew
Recipe ──────────┘

Brewing Method ──> used by many Recipes
Recipe ──> reused by many Coffee Brews
```

The first vertical slice creates and lists Coffee Beans. Recipes and Coffee Brews follow after that foundation is persistent and tested.
