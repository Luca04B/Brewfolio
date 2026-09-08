namespace Brewfolio.Domain.CoffeeBeans;

public readonly record struct CoffeeBeanId(Guid Value)
{
    public static CoffeeBeanId New()
    {
        return new CoffeeBeanId(Guid.NewGuid());
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}