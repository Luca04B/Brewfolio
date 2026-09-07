using Brewfolio.Domain.Results;

namespace Brewfolio.Domain.CoffeeBags;

public readonly record struct CoffeeBagNotFound;

public union CoffeeBagMutationResult(CoffeeBag, ValidationFailure, CoffeeBagNotFound);
