// ECommerceMcpServer.cs

using System.ComponentModel;
using ModelContextProtocol.Server;

[McpServerToolType]
public class ECommerceMcpServer
{
    private readonly CartService _cart;

    public ECommerceMcpServer(CartService cart)
    {
        _cart = cart;
    }

    // MCP tools
    [McpServerTool(Name = "get_price", Title = "Computes the price of socks, returning a value in dollars")]
    [Description("Computes the price of socks, returning a value in dollars")]
    public float GetPrice([Description("The number of pairs of socks to calculate price for")]int count)
    {
        return _cart.GetPrice(count);
    }

    [McpServerTool(Name = "add_to_cart", Title = "Adds the specified number of pairs of socks to the cart")]
    [Description("Adds the specified number of pairs of socks to the cart")]
    public void AddSocksToCart([Description("The number of pairs to add")] int numPairs)
    {
        _cart.AddSocksToCart(numPairs);
    }

    [McpServerTool(Name = "remove_from_cart", Title = "Removes the specified number of pairs of socks from the cart")]
    [Description("Removes the specified number of pairs of socks from the cart")]
    public void RemoveSocksFromCart([Description("The number of pairs to remove")]int numPairs)
    {
        _cart.RemoveSocksFromCart(numPairs);
    }

    [McpServerTool(Name = "get_cart_status", Title = "Gets the current cart contents")]
    [Description("Gets the current cart contents")]
    public object GetCartStatus()
    {
        return new
        {
            totalItems = _cart.NumPairsOfSocks,
            totalPrice = _cart.GetPrice(_cart.NumPairsOfSocks),
            currency = "USD"
        };
    }
}