namespace CoinbaseSandbox.Domain.Models;

public record Asset
{
    public Currency Currency { get; init; }
    public decimal Balance { get; private set; }
    public decimal Available { get; private set; }
    public decimal Held { get; private set; }
    
    public Asset(Currency currency, decimal balance = 0m)
    {
        Currency = currency ?? throw new ArgumentNullException(nameof(currency));
        
        if (balance < 0)
            throw new ArgumentException("Balance cannot be negative", nameof(balance));
            
        Balance = balance;
        Available = balance;
        Held = 0;
    }
    
    public void Deposit(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Deposit amount must be positive", nameof(amount));
            
        Balance += amount;
        Available += amount;
    }
    
    public void Withdraw(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Withdrawal amount must be positive", nameof(amount));
            
        if (amount > Available)
            throw new InvalidOperationException($"Insufficient available funds: {Available} {Currency.Symbol}");
            
        Balance -= amount;
        Available -= amount;
    }
    
    public void Hold(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Hold amount must be positive", nameof(amount));
            
        if (amount > Available)
            throw new InvalidOperationException($"Insufficient available funds: {Available} {Currency.Symbol}");
            
        Available -= amount;
        Held += amount;
    }
    
    public void Release(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Release amount must be positive", nameof(amount));
            
        if (amount > Held)
            throw new InvalidOperationException($"Insufficient held funds: {Held} {Currency.Symbol}");
            
        Available += amount;
        Held -= amount;
    }
    
    public void SettleHold(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Settle amount must be positive", nameof(amount));
            
        if (amount > Held)
            throw new InvalidOperationException($"Insufficient held funds: {Held} {Currency.Symbol}");
            
        Balance -= amount;
        Held -= amount;
    }
}