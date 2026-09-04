# Design patterns and reuse

Brewfolio uses design patterns to concentrate behavior behind small interfaces. A pattern earns its place when it removes repeated complexity, creates a useful seam, or makes important behavior easier to test. Pattern count is not a quality metric.

## Default patterns

### Vertical Slice

Organize each user capability end to end. `CreateCoffeeBean`, `ListCoffeeBeans`, and `MarkCoffeeBeanOutOfStock` each own their request, application behavior, tests, and endpoint mapping. Shared domain concepts remain in Domain; feature-specific orchestration stays local to the slice.

### Entity and Value Object

Use a Domain Entity when identity and lifecycle matter, such as `CoffeeBean`, `Recipe`, and `CoffeeBrew`. Introduce a Value Object when a value develops reusable rules or multiple parts that primitives cannot express clearly. The MVP keeps price as a decimal amount in euros.

### Factory Method

Create an Entity through a named factory when construction must enforce invariants or return an expected validation failure. Keep a normal constructor when it expresses the rules just as clearly.

### Command and Query

Represent state-changing use cases as Commands and read-only use cases as Queries. They may use separate result shapes. Introduce a mediator library only when dispatching and pipeline behavior provide more value than direct handler calls.

### Result

Return a Result for expected business failures such as invalid input, duplicates, or missing records. Reserve exceptions for unexpected failures and broken invariants.

The Domain uses the native C# 15 preview declarations `Result<T>(T, ValidationFailure)` and `Result(Success, ValidationFailure)`. Factories use the generic form to return a new Entity. A failable mutation uses the non-generic form when success has no additional payload. Infallible state changes remain `void`.

`ValidationFailure` contains at least one `ValidationError`. Each error has a stable machine-readable code, a Domain property name, and a client-safe description. Collect independent errors and return them together. An empty failure or the invalid default state of a generated union is a programming error, not an expected outcome.

Keep `NotFound`, `Conflict`, and other orchestration outcomes out of the shared Domain result. Define a concrete native union for an Application use case when it has distinct expected outcomes. The API maps those outcomes to HTTP and translates Domain property names to request-contract field names.

### Repository Adapter

Place a small persistence interface at the Application seam when a use case must load or save a Domain aggregate. Infrastructure supplies the EF Core adapter. Prefer capability-focused methods over a generic CRUD repository.

### Dependency Injection

The API composition root selects Infrastructure adapters and supplies them to Application modules. Domain objects receive the values they need and remain independent of the container.

## Patterns introduced on demand

- **Strategy**: when Brewfolio has multiple interchangeable calculations or policies, not merely multiple `if` branches.
- **Decorator or pipeline behavior**: when several use cases genuinely share validation, logging, authorization, or transaction behavior.
- **Specification**: when the same meaningful selection rule is reused across callers or must be composed.
- **Domain Event**: when one completed Domain action must trigger independent follow-up behavior without coupling the initiating module to it.
- **Adapter**: for Keycloak, external APIs, file storage, clocks, or other technology that sits behind an application-owned interface.

## Reuse rules

- Reuse domain language and behavior before reusing syntax.
- Keep the interface smaller than the complexity it hides; a pass-through abstraction has no depth.
- Extract shared code after real callers demonstrate the common concept.
- Keep feature-specific code inside its vertical slice until another feature needs the same behavior.
- Prefer composition over inheritance for application and frontend behavior.
- Test through the same interface callers use.

## Patterns to avoid by default

- `GenericRepository<TEntity>` exposing CRUD for every Entity
- `BaseService`, `BaseController`, or deep inheritance hierarchies
- one interface for every class regardless of variation or test seam
- mapping between duplicate models that carry no distinct contract
- domain events for synchronous steps that have only one caller
- shared utility folders containing unrelated helpers

These are not permanent bans. Introduce one when a concrete requirement shows that its interface hides meaningful complexity better than a direct implementation.

## Frontend reuse

Angular code is grouped by feature. Reusable UI belongs in a shared module only after multiple features need the same interaction and semantics. Keep HTTP access behind typed feature clients, keep presentation components focused on rendering and user interaction, and place business invariants in the backend Domain model.
