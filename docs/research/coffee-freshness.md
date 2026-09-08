# Roasted-coffee freshness: evidence and domain implications

Research date: 2026-09-04

## Question

What can Brewfolio defensibly infer from the roast date of a purchased package of coffee, especially when roast level, brewing method, opening, and storage differ?

## Executive conclusion

Roast age is useful for **sensory guidance**, not for declaring roasted coffee safe or unsafe. Dry roasted coffee is generally shelf-stable; one coffee-storage study states that this applies below 5% moisture, while the FDA explains more generally that microbial growth is governed by water activity and uses `a_w <= 0.85` as a regulatory threshold in relevant food categories. Brewfolio will not measure moisture, water activity, packaging integrity, contamination, or storage abuse, so a roast-date calculation must not produce labels such as “edible,” “inedible,” “safe,” “unsafe,” or “expires on.” ([Smrke et al., 2022](https://doi.org/10.1016/j.fpsl.2022.100893); [FDA, Water Activity in Foods](https://www.fda.gov/inspections-compliance-enforcement-and-criminal-investigations/inspection-technical-guides/water-activity-aw-foods))

The evidence also does not support a universal “best from day X through week Y” formula. Roast degree and roast speed materially affect carbon-dioxide release, and storage after opening materially affects aroma chemistry, but neither result establishes a universal sensory optimum. A recent sensory pilot found that consumers could distinguish one-day-old from 145-day-old coffee under the tested conditions, yet found no statistically significant preference for either; its authors explicitly caution against generalizing across coffees and roasting profiles. ([Smrke et al., 2018](https://doi.org/10.1021/acs.jafc.7b03310); [Smrke et al., 2022](https://doi.org/10.1016/j.fpsl.2022.100893); [Zimmermann, Schwarz, and Lachenmeier, 2025](https://doi.org/10.1007/s00217-025-04873-0))

For Brewfolio, the robust model is therefore a reusable `Coffee Bean` product plus one or more physical `Coffee Bag` records. `RoastedOn`, `PurchasedOn`, `OpenedOn`, package quantity, and stock state belong to the bag, because repeated purchases of the same product can have different roast dates, opening histories, and storage conditions. A future freshness feature should present configurable, explicitly sensory advice for each bag rather than changing the identity of the product or asserting food safety.

## 1. Safety is not sensory freshness

Peer-reviewed coffee shelf-life work treats roasted coffee as shelf-stable against microbial spoilage when it remains sufficiently dry, while investigating staling as chemical and sensory quality loss. In the same study, the authors distinguish shelf stability from continuing changes in aroma and carbon dioxide after roasting. ([Smrke et al., 2022](https://doi.org/10.1016/j.fpsl.2022.100893))

The FDA defines water activity as the available moisture relevant to microbial growth and explains that reducing it inhibits bacteria, yeasts, and molds; the roast date alone says nothing about the water activity of a particular bag. ([FDA, Water Activity in Foods](https://www.fda.gov/inspections-compliance-enforcement-and-criminal-investigations/inspection-technical-guides/water-activity-aw-foods))

Coffee freshness studies instead measure phenomena such as carbon-dioxide loss, volatile-aroma loss, oxidation products, or sensory acceptance. For example, Cardelli and Labuza defined the end of shelf life for roasted ground coffee as the point at which 50% of consumers rejected it, and found that oxygen partial pressure, water activity, and temperature all accelerated quality deterioration. That is a sensory acceptability endpoint under specified experimental conditions, not a toxicological expiry date. ([Cardelli and Labuza, 2001](https://doi.org/10.1006/fstl.2000.0732))

**Domain implication:** Store and display a sensory-quality projection separately from stock status and any manufacturer-provided best-before date. Never derive food-safety language from `RoastedOn`.

## 2. Degassing, rest, and “peak” use

Roasting produces gases, principally carbon dioxide, that remain partly trapped in the porous bean and escape during storage, grinding, and extraction. Carbon dioxide affects extraction and crema, while its release can serve as a physical freshness indicator. ([Smrke et al., 2018](https://doi.org/10.1021/acs.jafc.7b03310))

In the gravimetric study, darker roasts and shorter roast times released more gas and released it faster; the authors needed data extending beyond 400 hours to model whole-bean release kinetics. This supports the idea that coffees can rest differently, but it does not identify a sensory peak or an “inedible after” threshold. ([Smrke et al., 2018](https://doi.org/10.1021/acs.jafc.7b03310))

The SCA's scientific explainer summarizes the same work by noting rapid whole-bean degassing during roughly the first 20 days and proposes “about three weeks” as a physical-freshness rule of thumb when carbon dioxide is the guide. The SCA also notes that grinding accelerates the process dramatically. This is a practical heuristic based on one marker, not a universal flavor optimum. ([SCA, Coffee Decoded: What is “freshly roasted” coffee?](https://sca.coffee/sca-news/coffee-decoded-9-fresh-coffee))

A 2025 exploratory sensory study compared one-day and 145-day post-roast samples of one Catuaí coffee, one 50/50 light/dark Café Crème roast approach, one packaging regime, and one automatic-machine recipe. Seventy percent of the 60 participants distinguished the samples, but the 42 successful discriminators split 22 versus 20 in preference, which was not statistically significant. The design cannot locate an optimum between those two ages, and the authors identify coffee, roast, water, and participant differences as limits on generalization. ([Zimmermann, Schwarz, and Lachenmeier, 2025](https://doi.org/10.1007/s00217-025-04873-0))

**Domain implication:** If Brewfolio later offers a “resting” or “recommended now” label, describe it as a recommendation, retain the actual roast date, show the assumed rule, and let the user override that rule.

## 3. Roast level and brewing method

There is good evidence that roast degree changes degassing kinetics: the controlled gravimetric work found more and faster gas release for darker roasts, while roast speed also independently changed the result. A rule based on only `RoastLevel` would therefore omit another experimentally important variable. ([Smrke et al., 2018](https://doi.org/10.1021/acs.jafc.7b03310))

There is also evidence that stored beans can produce different outcomes under different extraction methods, but not enough to establish universal filter-versus-espresso date windows. One 2024 study compared espresso and cold brew after opening and storage, found brewing-method-dependent sensory profiles, and found that temperature effects differed between the tested brews; it did not study filter brewing, did not know the coffees' roast profiles, and sampled only the tested one-month storage interval. ([Gantner et al., 2024](https://pmc.ncbi.nlm.nih.gov/articles/PMC11675256/))

The SCA demonstrates extraction effects rather than an optimum calendar window: coffee age and retained gas changed crema and flow in espresso, and the grind had to be adjusted to match extraction conditions across ages. The same SCA material relates retained gas to both espresso crema and filter bloom, but does not prescribe validated method-specific peak dates. ([SCA, The Science of Coffee Freshness](https://sca.coffee/sca-news/podcast/81/the-science-of-coffee-freshness-samo-smrke-expo-lectures-2019-6lrbe))

Roasters do publish useful operational guidance, but their ranges are recipes for their own coffees rather than universal evidence. Coffee Collective, for example, recommends at least 3–4 days for its French-press coffees and two weeks for its espresso, whereas Tim Wendelboe reports best results after approximately 5–10 days for its coffees. The disagreement is a reason to model configurable guidance, not to choose one brand's numbers as a domain invariant. ([Coffee Collective, 2019](https://coffeecollective.dk/blogs/stories/webshop-changes); [Tim Wendelboe, FAQ](https://timwendelboe.no/pages/faq))

**Conclusion on defaults:** Brewfolio should not ship roast-level- or brew-method-specific “good/bad” thresholds as scientific facts. A later feature may offer named presets—clearly labeled as an SCA heuristic, a roaster recommendation, or the user's preference—but the source and scope must be visible.

## 4. Opening and storage change the calculation

Opening a package replaces its protective atmosphere and can accelerate staling. In a controlled whole-bean study, an integrated screw-cap package preserved the tested chemical freshness indicators better than a clip, tape, or transfer to an airtight canister; the results tracked differences in oxygen and carbon-dioxide exchange. The authors explicitly state that sensory studies are still needed to define an acceptance threshold. ([Smrke et al., 2022](https://digitalcollection.zhaw.ch/bitstream/11475/26687/3/2022_Smrke-etal_Effects-of-different-coffee-storage-methods-on-coffee-freshness_FPSL.pdf))

Temperature, oxygen, and water activity interact with deterioration. Cardelli and Labuza found a roughly twenty-fold acceleration when oxygen partial pressure increased from 0.5 to 21.3 kPa in their roasted-ground-coffee experiment; increasing water activity and temperature also accelerated rejection. These quantified effects belong to that sample and model and should not be copied as a consumer-facing Brewfolio formula. ([Cardelli and Labuza, 2001](https://doi.org/10.1006/fstl.2000.0732))

The 2024 whole-bean study found smaller volatile-profile changes at 5 °C than at 20 °C during one month of secondary storage, but outcomes depended on coffee type and brewing method. Its packages were first opened one month after roasting, and the roaster did not provide the coffees' origin, variety, or roast-profile data, limiting extrapolation. ([Gantner et al., 2024](https://pmc.ncbi.nlm.nih.gov/articles/PMC11675256/))

**Domain implication:** `OpenedOn` is more relevant to freshness guidance than `PurchasedOn`. If storage-aware advice is later required, record storage state or storage events (for example, ambient versus frozen) rather than pretending roast age alone captures them. Purchase date remains useful for inventory history and does not substitute for an unknown roast date.

## 5. Recommended Brewfolio model

The research supports the product/package distinction already present in Brewfolio's glossary:

```text
Coffee Bean (reusable product)
  1 ─────── * Coffee Bag (physical purchased package)
                 ├─ RoastedOn          required for age-based guidance
                 ├─ PurchasedOn        inventory/provenance, not freshness origin
                 ├─ OpenedOn?          secondary-shelf-life boundary
                 ├─ InitialWeightGrams
                 ├─ RemainingWeightGrams? or explicit stock state
                 └─ Storage history?   defer until storage-aware guidance exists
```

The recommendation above is a domain-design inference from the evidence: freshness-relevant dates and conditions vary per physical package, while product identity can remain stable across repeat purchases.

For the first Coffee Bean slice:

- Keep `Coffee Bean` as the reusable product with name, roaster, origin, roast level, image, and product URL.
- Do not place `RoastedOn` or `PurchasedOn` on `Coffee Bean`; otherwise a repeat purchase would overwrite the history of an earlier bag.
- If bag tracking is not in the first slice, keep `IsInStock` as the agreed temporary manual Boolean and defer freshness calculations entirely.
- When `Coffee Bag` is added, consider deriving product-level “in stock” from its active bags instead of maintaining two independent truths.

These four bullets are modeling recommendations, not claims made by the cited scientific sources.

## 6. Language the software should avoid

Avoid:

- “Edible until” / “inedible”
- “Safe” / “unsafe” based on roast age
- “Expires on” unless displaying a date supplied by the manufacturer
- “Fresh” as a hidden binary with an unexplained cutoff
- “Dark roast lasts X weeks; light roast lasts Y weeks” presented as scientific fact
- “Best for espresso/filter” without naming the preset or recommendation source

Prefer:

- “Days since roast”
- “Resting according to: [named preset]”
- “Within / past your preferred flavor window”
- “Opened X days ago”
- “Freshness guidance unavailable: roast date unknown”
- “Sensory guidance only; storage conditions matter”

## Sources

- Anese, M., Manzocco, L., and Nicoli, M. C. (2006). “Modeling the Secondary Shelf Life of Ground Roasted Coffee.” *Journal of Agricultural and Food Chemistry*, 54(15), 5571–5576. [DOI](https://doi.org/10.1021/jf060204k)
- Cardelli, C., and Labuza, T. P. (2001). “Application of Weibull Hazard Analysis to the Determination of the Shelf Life of Roasted and Ground Coffee.” *LWT - Food Science and Technology*, 34(5), 273–278. [DOI](https://doi.org/10.1006/fstl.2000.0732)
- Gantner, M., Kostyra, E., Górska-Horczyczak, E., and Piotrowska, A. (2024). “Effect of Temperature and Storage on Coffee's Volatile Compound Profile and Sensory Characteristics.” *Foods*, 13(24), 3995. [Open full text](https://pmc.ncbi.nlm.nih.gov/articles/PMC11675256/)
- Smrke, S., Wellinger, M., Suzuki, T., Balsiger, F., Opitz, S. E. W., and Yeretzian, C. (2018). “Time-Resolved Gravimetric Method To Assess Degassing of Roasted Coffee.” *Journal of Agricultural and Food Chemistry*, 66(21), 5293–5300. [DOI](https://doi.org/10.1021/acs.jafc.7b03310), [ZHAW record](https://digitalcollection.zhaw.ch/items/4789f6f9-a182-4840-a313-54a3e8445aec)
- Smrke, S., Adam, J., Mühlemann, S., Lantz, I., and Yeretzian, C. (2022). “Effects of different coffee storage methods on coffee freshness after opening of packages.” *Food Packaging and Shelf Life*, 33, 100893. [Open full text](https://digitalcollection.zhaw.ch/bitstream/11475/26687/3/2022_Smrke-etal_Effects-of-different-coffee-storage-methods-on-coffee-freshness_FPSL.pdf)
- Zimmermann, Y. C., Schwarz, S., and Lachenmeier, D. W. (2025). “Post-roast maturation and coffee quality: a sensory perspective on freshness.” *European Food Research and Technology*. [Open full text](https://link.springer.com/article/10.1007/s00217-025-04873-0)
- Specialty Coffee Association. “Coffee Decoded: What is ‘freshly roasted’ coffee?” [SCA](https://sca.coffee/sca-news/coffee-decoded-9-fresh-coffee)
- Specialty Coffee Association. “The Science of Coffee Freshness.” [SCA](https://sca.coffee/sca-news/podcast/81/the-science-of-coffee-freshness-samo-smrke-expo-lectures-2019-6lrbe)
- U.S. Food and Drug Administration. “Water Activity (aw) in Foods.” [FDA](https://www.fda.gov/inspections-compliance-enforcement-and-criminal-investigations/inspection-technical-guides/water-activity-aw-foods)
- Coffee Collective. “Webshop Changes” (operational roaster guidance, not universal evidence). [Coffee Collective](https://coffeecollective.dk/blogs/stories/webshop-changes)
- Tim Wendelboe. “FAQ” (operational roaster guidance, not universal evidence). [Tim Wendelboe](https://timwendelboe.no/pages/faq)
