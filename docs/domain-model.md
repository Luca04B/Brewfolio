# Domain model

This document captures Brewfolio's current domain shape. It is intentionally small and will evolve through concrete user scenarios rather than speculative fields.

Domain Entity identifiers use distinct StrongOf types. Names with different meanings also remain distinct inside the Domain model, so the compiler cannot exchange values such as a Coffee Bean name and a Roaster name. HTTP and JSON contracts expose their underlying `Guid` and `string` values.

## Coffee Bean

A `CoffeeBean` represents a reusable coffee product in a Member's collection. Repeated purchases of that product are represented by separate `CoffeeBag` records.

Initial information:

- `Id`
- `Name`
- `Roaster`
- `Origin`
- `RoastLevel`
- optional product URL
- optional image
- optional short description

A Coffee Bean's stock status is derived: it is in stock when at least one of its Coffee Bags is in stock. Its average price is also derived from the total price and initial weight of its Coffee Bags and is displayed as a weighted price per 100 grams.

Creating a Coffee Bean returns `Result<CoffeeBean>`. Valid input produces the Entity; expected invalid input produces a structured error. The result is implemented as a native C# 15 preview union so callers must distinguish expected outcomes explicitly.

## Coffee Bag

A `CoffeeBag` represents one physical package purchased for a Coffee Bean. A Coffee Bean can exist without a current bag and can have several bags from repeated purchases.

Initial information:

- `Id`
- `CoffeeBeanId`
- `PurchasedOn`
- optional `RoastedOn`
- optional `OpenedOn`
- `InitialWeightGrams`
- `PricePaid`, stored as a decimal amount in euros
- `IsInStock`

`PricePaid` is the price paid for the bag itself and excludes order-wide shipping. Brewfolio does not track a remaining gram quantity and does not reduce stock when a Coffee Brew is recorded. Stock is changed manually.

Roast age and time since opening can support sensory Freshness Guidance. They must not be used to claim that coffee is safe, unsafe, edible, inedible, or expired. See [the freshness research](research/coffee-freshness.md).

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

A `BrewingMethod` represents a shared, configurable preparation method such as V60, AeroPress, or French Press.

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
Coffee Bean ──> has many Coffee Bags

Coffee Bean ─────┐
                 ├──> Coffee Brew
Recipe ──────────┘

Brewing Method ──> used by many Recipes
Recipe ──> reused by many Coffee Brews
```

## Future community direction

After authentication and ownership exist, Members will be able to publish Recipes for other Members to prepare. Public Recipe Scores and rankings will be derived from ratings on the resulting Coffee Brews. Publication lifecycle, rating eligibility, weighting, and ranking rules will be designed with that vertical slice rather than added to the current entities in advance.

The first vertical slice manages Coffee Beans and their Coffee Bags. Recipes and Coffee Brews follow after that foundation is persistent and tested.
