# Brewfolio

Brewfolio is the product and repository being built as a learning-focused portfolio application. This glossary will grow only when business terms are agreed during feature design.

## Language

**Brewfolio**:
The product represented by this monorepo.
_Avoid_: the app, the thing

**Coffee Bean**:
A specific coffee product that a Member can keep in their current collection and use for Recipes and Coffee Brews.
_Avoid_: Coffee, Bean, Coffee Beans when referring to one item

**Member**:
A person who keeps a coffee collection, creates Recipes, records Coffee Brews, and may participate in Brewfolio's future community.
_Avoid_: User, Customer

**Recipe**:
A reusable plan created by a Member for preparing coffee, including the intended Brewing Method, quantities, Water Temperature, grind, and target brew time. It is independent of a particular Coffee Bean and may later be published for other Members.
_Avoid_: Brew, Coffee Brew, Post

**Published Recipe**:
A Recipe that its creating Member has shared with other Members to prepare.
_Avoid_: Post, Coffee Brew

**Brewing Method**:
A shared, centrally managed way of preparing coffee that can be selected by Recipes, deactivated without removing historical references, and later reactivated.
_Avoid_: Recipe, Preparation Type, Member-owned Brewing Method

**Coffee Brew**:
A record of one completed coffee preparation by a Member using a Coffee Bean and a Recipe at a particular time. Its optional rating describes that specific preparation.
_Avoid_: Coffee, Recipe, Brew Log

**Recipe Score**:
An aggregate assessment derived from ratings of Coffee Brews made from a Published Recipe and used to compare Published Recipes.
_Avoid_: Coffee Brew Rating, Manually Assigned Rating

**Unknown**:
A valid saved value explicitly chosen when a Member does not know a Coffee Bean's Roast Level or a Recipe's Grind Size. The corresponding field is still required.
_Avoid_: Invalid, Missing

**Water Temperature**:
The intended temperature of the brewing water when it contacts the coffee while following a Recipe.
_Avoid_: Boiler Setpoint, Beverage Temperature
