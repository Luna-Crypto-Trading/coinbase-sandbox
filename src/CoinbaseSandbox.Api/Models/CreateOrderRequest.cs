using System.Text.Json.Serialization;

namespace CoinbaseSandbox.Api.Models;

public class CreateOrderRequest
{
    [JsonPropertyName("client_order_id")]
    public string ClientOrderId { get; set; } = string.Empty;

    [JsonPropertyName("product_id")]
    public string ProductId { get; set; } = string.Empty;

    [JsonPropertyName("side")]
    public string Side { get; set; } = string.Empty;

    [JsonPropertyName("order_configuration")]
    public OrderConfiguration OrderConfiguration { get; set; } = new();

    [JsonPropertyName("leverage")]
    public string? Leverage { get; set; }

    [JsonPropertyName("margin_type")]
    public string? MarginType { get; set; }

    [JsonPropertyName("retail_portfolio_id")]
    public string? RetailPortfolioId { get; set; }

    [JsonPropertyName("preview_id")]
    public string? PreviewId { get; set; }

    [JsonPropertyName("attached_order_configuration")]
    public AttachedOrderConfiguration? AttachedOrderConfiguration { get; set; }
}

public class OrderConfiguration
{
    [JsonPropertyName("market_market_ioc")]
    public MarketMarketIoc? MarketMarketIoc { get; set; }

    [JsonPropertyName("sor_limit_ioc")]
    public SorLimitIoc? SorLimitIoc { get; set; }

    [JsonPropertyName("limit_limit_gtc")]
    public LimitLimitGtc? LimitLimitGtc { get; set; }

    [JsonPropertyName("limit_limit_gtd")]
    public LimitLimitGtd? LimitLimitGtd { get; set; }

    [JsonPropertyName("limit_limit_fok")]
    public LimitLimitFok? LimitLimitFok { get; set; }

    [JsonPropertyName("twap_limit_gtd")]
    public TwapLimitGtd? TwapLimitGtd { get; set; }

    [JsonPropertyName("stop_limit_stop_limit_gtc")]
    public StopLimitStopLimitGtc? StopLimitStopLimitGtc { get; set; }

    [JsonPropertyName("stop_limit_stop_limit_gtd")]
    public StopLimitStopLimitGtd? StopLimitStopLimitGtd { get; set; }

    [JsonPropertyName("trigger_bracket_gtc")]
    public TriggerBracketGtc? TriggerBracketGtc { get; set; }

    [JsonPropertyName("trigger_bracket_gtd")]
    public TriggerBracketGtd? TriggerBracketGtd { get; set; }
}

public class AttachedOrderConfiguration
{
    [JsonPropertyName("market_market_ioc")]
    public MarketMarketIoc? MarketMarketIoc { get; set; }

    [JsonPropertyName("sor_limit_ioc")]
    public SorLimitIoc? SorLimitIoc { get; set; }

    [JsonPropertyName("limit_limit_gtc")]
    public LimitLimitGtc? LimitLimitGtc { get; set; }

    [JsonPropertyName("limit_limit_gtd")]
    public LimitLimitGtd? LimitLimitGtd { get; set; }

    [JsonPropertyName("limit_limit_fok")]
    public LimitLimitFok? LimitLimitFok { get; set; }

    [JsonPropertyName("twap_limit_gtd")]
    public TwapLimitGtd? TwapLimitGtd { get; set; }

    [JsonPropertyName("stop_limit_stop_limit_gtc")]
    public StopLimitStopLimitGtc? StopLimitStopLimitGtc { get; set; }

    [JsonPropertyName("stop_limit_stop_limit_gtd")]
    public StopLimitStopLimitGtd? StopLimitStopLimitGtd { get; set; }

    [JsonPropertyName("trigger_bracket_gtc")]
    public TriggerBracketGtc? TriggerBracketGtc { get; set; }

    [JsonPropertyName("trigger_bracket_gtd")]
    public TriggerBracketGtd? TriggerBracketGtd { get; set; }
}

// Market Order - Immediate or Cancel
public class MarketMarketIoc
{
    [JsonPropertyName("quote_size")]
    public string? QuoteSize { get; set; }

    [JsonPropertyName("base_size")]
    public string? BaseSize { get; set; }
}

// Smart Order Router Limit - Immediate or Cancel
public class SorLimitIoc
{
    [JsonPropertyName("quote_size")]
    public string? QuoteSize { get; set; }

    [JsonPropertyName("base_size")]
    public string? BaseSize { get; set; }

    [JsonPropertyName("limit_price")]
    public string LimitPrice { get; set; } = string.Empty;
}

// Limit Order - Good Till Canceled
public class LimitLimitGtc
{
    [JsonPropertyName("quote_size")]
    public string? QuoteSize { get; set; }

    [JsonPropertyName("base_size")]
    public string? BaseSize { get; set; }

    [JsonPropertyName("limit_price")]
    public string LimitPrice { get; set; } = string.Empty;

    [JsonPropertyName("post_only")]
    public bool PostOnly { get; set; }
}

// Limit Order - Good Till Date
public class LimitLimitGtd
{
    [JsonPropertyName("quote_size")]
    public string? QuoteSize { get; set; }

    [JsonPropertyName("base_size")]
    public string? BaseSize { get; set; }

    [JsonPropertyName("limit_price")]
    public string LimitPrice { get; set; } = string.Empty;

    [JsonPropertyName("end_time")]
    public string EndTime { get; set; } = string.Empty;

    [JsonPropertyName("post_only")]
    public bool PostOnly { get; set; }
}

// Limit Order - Fill or Kill
public class LimitLimitFok
{
    [JsonPropertyName("quote_size")]
    public string? QuoteSize { get; set; }

    [JsonPropertyName("base_size")]
    public string? BaseSize { get; set; }

    [JsonPropertyName("limit_price")]
    public string LimitPrice { get; set; } = string.Empty;
}

// Time-Weighted Average Price
public class TwapLimitGtd
{
    [JsonPropertyName("quote_size")]
    public string? QuoteSize { get; set; }

    [JsonPropertyName("base_size")]
    public string? BaseSize { get; set; }

    [JsonPropertyName("start_time")]
    public string StartTime { get; set; } = string.Empty;

    [JsonPropertyName("end_time")]
    public string EndTime { get; set; } = string.Empty;

    [JsonPropertyName("limit_price")]
    public string LimitPrice { get; set; } = string.Empty;

    [JsonPropertyName("number_buckets")]
    public string NumberBuckets { get; set; } = string.Empty;

    [JsonPropertyName("bucket_size")]
    public string BucketSize { get; set; } = string.Empty;

    [JsonPropertyName("bucket_duration")]
    public string BucketDuration { get; set; } = string.Empty;
}

// Stop Limit - Good Till Canceled
public class StopLimitStopLimitGtc
{
    [JsonPropertyName("base_size")]
    public string BaseSize { get; set; } = string.Empty;

    [JsonPropertyName("limit_price")]
    public string LimitPrice { get; set; } = string.Empty;

    [JsonPropertyName("stop_price")]
    public string StopPrice { get; set; } = string.Empty;

    [JsonPropertyName("stop_direction")]
    public string StopDirection { get; set; } = string.Empty;
}

// Stop Limit - Good Till Date
public class StopLimitStopLimitGtd
{
    [JsonPropertyName("base_size")]
    public string BaseSize { get; set; } = string.Empty;

    [JsonPropertyName("limit_price")]
    public string LimitPrice { get; set; } = string.Empty;

    [JsonPropertyName("stop_price")]
    public string StopPrice { get; set; } = string.Empty;

    [JsonPropertyName("end_time")]
    public string EndTime { get; set; } = string.Empty;

    [JsonPropertyName("stop_direction")]
    public string StopDirection { get; set; } = string.Empty;
}

// Trigger Bracket - Good Till Canceled
public class TriggerBracketGtc
{
    [JsonPropertyName("base_size")]
    public string BaseSize { get; set; } = string.Empty;

    [JsonPropertyName("limit_price")]
    public string LimitPrice { get; set; } = string.Empty;

    [JsonPropertyName("stop_trigger_price")]
    public string StopTriggerPrice { get; set; } = string.Empty;
}

// Trigger Bracket - Good Till Date
public class TriggerBracketGtd
{
    [JsonPropertyName("base_size")]
    public string BaseSize { get; set; } = string.Empty;

    [JsonPropertyName("limit_price")]
    public string LimitPrice { get; set; } = string.Empty;

    [JsonPropertyName("stop_trigger_price")]
    public string StopTriggerPrice { get; set; } = string.Empty;

    [JsonPropertyName("end_time")]
    public string EndTime { get; set; } = string.Empty;
}