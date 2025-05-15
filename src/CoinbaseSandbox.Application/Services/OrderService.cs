namespace CoinbaseSandbox.Application.Services;

using Domain.Events;
using Domain.Models;
using Domain.Repositories;
using CoinbaseSandbox.Domain.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IPriceService _priceService;
    private readonly IWalletService _walletService;
    private readonly IEventPublisher _eventPublisher;

    // Default fee percentage for trades
    private const decimal DefaultFeePercentage = 0.006m; // 0.6%

    public OrderService(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IPriceService priceService,
        IWalletService walletService,
        IEventPublisher eventPublisher)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _priceService = priceService;
        _walletService = walletService;
        _eventPublisher = eventPublisher;
    }

    public async Task<Order> PlaceOrderAsync(
        string productId,
        OrderSide side,
        OrderType type,
        decimal size,
        decimal? limitPrice = null,
        CancellationToken cancellationToken = default)
    {
        // Validate product exists
        var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
        if (product == null)
            throw new ArgumentException($"Product {productId} not found", nameof(productId));

        // Validate order size
        if (size < product.MinimumOrderSize)
            throw new ArgumentException($"Order size must be at least {product.MinimumOrderSize}", nameof(size));

        // Create the order
        var order = new Order(productId, side, type, size, limitPrice);

        // For market orders, execute immediately
        if (type == OrderType.Market)
        {
            await ExecuteMarketOrderAsync(order, product, cancellationToken);
        }
        else
        {
            // For limit orders, just store them for now
            order.Open();
        }

        // Save the order
        await _orderRepository.AddAsync(order, cancellationToken);

        return order;
    }

    private async Task ExecuteMarketOrderAsync(Order order, Product product, CancellationToken cancellationToken)
    {
        // Get current price
        var currentPrice = await _priceService.GetCurrentPriceAsync(order.ProductId, cancellationToken);

        // Calculate order details
        var executedPrice = currentPrice;
        var quoteAmount = order.Size * executedPrice;
        var fee = quoteAmount * DefaultFeePercentage;

        // Default wallet ID - in a real system, we'd get this from the user
        const string walletId = "default";

        // Adjust balances based on order side
        if (order.Side == OrderSide.Buy)
        {
            // Withdraw quote currency (e.g., USD)
            await _walletService.WithdrawAsync(walletId, product.QuoteCurrency.Symbol, quoteAmount + fee, cancellationToken);

            // Deposit base currency (e.g., BTC)
            await _walletService.DepositAsync(walletId, product.BaseCurrency.Symbol, order.Size, cancellationToken);
        }
        else // Sell
        {
            // Withdraw base currency (e.g., BTC)
            await _walletService.WithdrawAsync(walletId, product.BaseCurrency.Symbol, order.Size, cancellationToken);

            // Deposit quote currency (e.g., USD) minus fees
            await _walletService.DepositAsync(walletId, product.QuoteCurrency.Symbol, quoteAmount - fee, cancellationToken);
        }

        // Mark order as filled
        order.Fill(executedPrice, fee);

        // Publish order filled event
        await _eventPublisher.PublishAsync(new OrderFilledEvent(order), cancellationToken);
    }

    public async Task<Order> GetOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
        if (order == null)
            throw new KeyNotFoundException($"Order {orderId} not found");

        return order;
    }

    public async Task<IEnumerable<Order>> GetOrdersAsync(int limit = 100, CancellationToken cancellationToken = default)
    {
        return await _orderRepository.GetAllAsync(limit, cancellationToken);
    }

    public async Task<Order> CancelOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await GetOrderAsync(orderId, cancellationToken);

        // Cancel the order
        order.Cancel();

        // Update the order
        await _orderRepository.UpdateAsync(order, cancellationToken);

        return order;
    }
}