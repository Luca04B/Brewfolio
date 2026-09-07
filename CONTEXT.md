# Brewfolio

Brewfolio is the product and repository being built as a learning-focused portfolio application. This glossary will grow only when business terms are agreed during feature design.

## Language

**Brewfolio**:
The product represented by this monorepo.
_Avoid_: the app, the thing

**Coffee Bean**:
A specific coffee product that a Member can keep in their collection, purchase repeatedly as Coffee Bags, and use for Recipes and Coffee Brews. It represents the reusable product definition, not an individual physical package.
_Avoid_: Coffee, Bean, Coffee Beans when referring to one item, Coffee Bag

**Coffee Bag**:
A particular purchased package of a Coffee Bean. Separate Coffee Bags distinguish repeated purchases or roasts of the same product and provide the source records for later price and spending analysis.
_Avoid_: Coffee Bean, Purchase, Batch

**Roaster**:
The person or organization that roasts and offers a Coffee Bean product; Brewfolio currently records its name as part of the product identity.
_Avoid_: Rooster, Retailer

**Stock Status**:
The manually recorded availability of a Coffee Bag. A Coffee Bean is considered in stock when at least one of its Coffee Bags is in stock.
_Avoid_: Remaining Quantity, Freshness

**Freshness Guidance**:
Sensory-oriented information or recommendations based on a Coffee Bag's age and known handling. It is not a statement about food safety, edibility, or expiry.
_Avoid_: Expiry Status, Safe to Drink, Edible

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
