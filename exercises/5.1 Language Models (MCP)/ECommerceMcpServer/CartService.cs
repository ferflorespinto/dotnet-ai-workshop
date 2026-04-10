// CartService.cs

using System.ComponentModel;

public class CartService
{
    public int NumPairsOfSocks { get; set; }
    
    [Description("Adds the specified number of pairs of socks to the cart")]
    public void AddSocksToCart(int numPairs)
    {
        NumPairsOfSocks += numPairs;
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("*****");
        Console.WriteLine($"Added {numPairs} pairs to your cart. Total: {NumPairsOfSocks} pairs.");
        Console.WriteLine("*****");
        Console.ForegroundColor = ConsoleColor.White;
    }

    [Description("Removes the specified number of pairs of socks from the cart, or the items remaining, whichever is lower.")]
    public void RemoveSocksFromCart(int numPairs)
    {
        int numPairsToRemove = numPairs > NumPairsOfSocks ? NumPairsOfSocks : numPairs;
        NumPairsOfSocks -= numPairsToRemove;

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("*****");
        Console.WriteLine($"Removed {numPairsToRemove} pairs from your cart. Total: {NumPairsOfSocks} pairs.");
        Console.WriteLine("*****");
        Console.ForegroundColor = ConsoleColor.White;
    }
    
    [Description("Removes all pairs of socks from the cart.")]
    public void EmptyCart()
    {
        string message = NumPairsOfSocks == 0 ? "Cart is already empty" : "Emptied your cart";
        NumPairsOfSocks = 0;
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("*****");
        Console.WriteLine($"{message}. Total: {NumPairsOfSocks} pairs.");
        Console.WriteLine("*****");
        Console.ForegroundColor = ConsoleColor.White;
    }

    [Description("Computes the price of socks, returning a value in dollars.")]
    public float GetPrice(
        [Description("The number of pairs of socks to calculate price for")] int count)
        => count * 15.99f;
}