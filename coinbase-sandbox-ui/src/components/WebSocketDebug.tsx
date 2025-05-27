import React, { useState, useEffect, useRef } from "react";
import {
  Box,
  Paper,
  Typography,
  Button,
  TextField,
  Grid,
  Chip,
  Alert,
  IconButton,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  InputAdornment,
  Card,
  CardContent,
  Switch,
  FormControlLabel,
} from "@mui/material";
import {
  PlayArrow as ConnectIcon,
  Stop as DisconnectIcon,
  Send as SendIcon,
  Delete as DeleteIcon,
  Add as AddIcon,
  Wifi as WifiIcon,
  WifiOff as WifiOffIcon,
  Settings as SettingsIcon,
  Download as DownloadIcon,
} from "@mui/icons-material";
import { PageLayout } from "./PageLayout";

interface WebSocketMessage {
  id: string;
  type: string;
  timestamp: Date;
  data: Record<string, unknown> | string;
}

interface Subscription {
  channel: string;
  productIds: string[];
}

const DEFAULT_WS_URLS = [
  "wss://ws-feed-public.sandbox.exchange.coinbase.com",
  "ws://localhost:5000/ws",
];

// Predefined subscriptions based on the image
const PREDEFINED_SUBSCRIPTIONS = [
  { channel: "ticker", productIds: ["BTC-USD", "ETH-USD"] },
  { channel: "level2", productIds: ["BTC-USD"] },
  { channel: "matches", productIds: ["BTC-USD", "ETH-USD"] },
  { channel: "heartbeat", productIds: [] },
  { channel: "status", productIds: [] },
];

export const WebSocketDebug: React.FC = () => {
  const [isConnected, setIsConnected] = useState(false);
  const [messages, setMessages] = useState<WebSocketMessage[]>([]);
  const [customMessage, setCustomMessage] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [subscriptions, setSubscriptions] = useState<Subscription[]>([]);
  const [newChannel, setNewChannel] = useState("");
  const [newProductIds, setNewProductIds] = useState("");
  const [wsUrls, setWsUrls] = useState<string[]>(() => {
    const saved = localStorage.getItem("wsUrls");
    return saved ? JSON.parse(saved) : DEFAULT_WS_URLS;
  });
  const [selectedUrl, setSelectedUrl] = useState(wsUrls[0]);
  const [newUrl, setNewUrl] = useState("");
  const [showNewUrlInput, setShowNewUrlInput] = useState(false);
  const [autoReconnect, setAutoReconnect] = useState(true);
  const [messageFilter, setMessageFilter] = useState("");
  const [showOnlyErrors, setShowOnlyErrors] = useState(false);
  const wsRef = useRef<WebSocket | null>(null);

  // Save URLs to localStorage when they change
  useEffect(() => {
    localStorage.setItem("wsUrls", JSON.stringify(wsUrls));
  }, [wsUrls]);

  // Connect to WebSocket
  const connect = () => {
    try {
      const ws = new WebSocket(selectedUrl);
      wsRef.current = ws;

      ws.onopen = () => {
        setIsConnected(true);
        setError(null);
        addMessage({
          id: generateMessageId(),
          type: "system",
          timestamp: new Date(),
          data: "Connected to WebSocket",
        });
        console.log("WebSocket connection established");
      };

      ws.onclose = () => {
        setIsConnected(false);
        addMessage({
          id: generateMessageId(),
          type: "system",
          timestamp: new Date(),
          data: "Disconnected from WebSocket",
        });
        console.log("WebSocket connection closed");

        // Auto reconnect if enabled
        if (autoReconnect) {
          setTimeout(connect, 3000);
        }
      };

      ws.onerror = (event) => {
        setError("WebSocket error occurred");
        console.error("WebSocket error:", event);
        addMessage({
          id: generateMessageId(),
          type: "error",
          timestamp: new Date(),
          data: "WebSocket error occurred",
        });
        console.log("Failed to connect to WebSocket");
      };

      ws.onmessage = (event) => {
        try {
          const data = JSON.parse(event.data);
          addMessage({
            id: generateMessageId(),
            type: "received",
            timestamp: new Date(),
            data,
          });
        } catch {
          addMessage({
            id: generateMessageId(),
            type: "error",
            timestamp: new Date(),
            data: "Failed to parse message",
          });
        }
      };
    } catch (err) {
      setError("Failed to connect to WebSocket");
      console.error("Connection error:", err);
      console.log("Unable to establish WebSocket connection");
    }
  };

  // Generate unique message ID
  const generateMessageId = () => {
    return Date.now().toString() + Math.random().toString(36).substr(2, 9);
  };

  // Add new WebSocket URL
  const handleAddUrl = () => {
    if (newUrl && !wsUrls.includes(newUrl)) {
      setWsUrls([...wsUrls, newUrl]);
      setSelectedUrl(newUrl);
      setNewUrl("");
      setShowNewUrlInput(false);
    }
  };

  // Remove WebSocket URL
  const handleRemoveUrl = (url: string) => {
    if (wsUrls.length > 1) {
      const newUrls = wsUrls.filter((u) => u !== url);
      setWsUrls(newUrls);
      if (selectedUrl === url) {
        setSelectedUrl(newUrls[0]);
      }
    }
  };

  // Disconnect from WebSocket
  const disconnect = () => {
    if (wsRef.current) {
      wsRef.current.close();
      wsRef.current = null;
    }
  };

  // Add message to the list
  const addMessage = (message: WebSocketMessage) => {
    setMessages((prev) => [...prev, message].slice(-100)); // Keep last 100 messages
  };

  // Send custom message
  const sendMessage = () => {
    if (!wsRef.current || !customMessage) return;

    try {
      wsRef.current.send(customMessage);
      addMessage({
        id: generateMessageId(),
        type: "sent",
        timestamp: new Date(),
        data: JSON.parse(customMessage),
      });
      setCustomMessage("");
    } catch (err) {
      setError("Failed to send message");
      console.error("Send error:", err);
      console.log("Failed to send message");
    }
  };

  // Subscribe to channel
  const subscribe = (channel: string, productIds: string[]) => {
    if (!wsRef.current || !channel || productIds.length === 0) {
      console.log(
        "Cannot subscribe: WebSocket not connected, channel missing, or no product IDs specified."
      );
      return;
    }

    const subscription: Subscription = {
      channel,
      productIds,
    };

    // Prevent adding duplicate subscriptions to the displayed list
    if (
      subscriptions.some(
        (sub) =>
          sub.channel === channel &&
          sub.productIds.every((id, index) => id === productIds[index])
      )
    ) {
      console.log(
        `Already subscribed to ${channel} with product IDs ${productIds.join(
          ", "
        )}`
      );
      return;
    }

    const message = {
      type: "subscribe",
      channels: [channel],
      product_ids: productIds,
    };

    try {
      wsRef.current.send(JSON.stringify(message));
      setSubscriptions((prev) => [...prev, subscription]);
      console.log(`Successfully subscribed to ${channel}`);
    } catch (err) {
      setError("Failed to subscribe");
      console.error("Subscribe error:", err);
      console.log("Failed to subscribe to channel");
    }
  };

  // Unsubscribe from channel
  const unsubscribe = (subscription: Subscription) => {
    if (!wsRef.current) return;

    const message = {
      type: "unsubscribe",
      channels: [subscription.channel],
      product_ids: subscription.productIds,
    };

    try {
      wsRef.current.send(JSON.stringify(message));
      setSubscriptions((prev) => prev.filter((s) => s !== subscription));
      console.log(`Successfully unsubscribed from ${subscription.channel}`);
    } catch (err) {
      setError("Failed to unsubscribe");
      console.error("Unsubscribe error:", err);
      console.log("Failed to unsubscribe from channel");
    }
  };

  // Clear messages
  const clearMessages = () => {
    setMessages([]);
    console.log("Message log has been cleared");
  };

  // Filter messages based on current filter settings
  const filteredMessages = messages.filter((message) => {
    if (showOnlyErrors && message.type !== "error") return false;
    if (messageFilter) {
      const messageStr = JSON.stringify(message.data).toLowerCase();
      return messageStr.includes(messageFilter.toLowerCase());
    }
    return true;
  });

  // Cleanup on unmount
  useEffect(() => {
    return () => {
      if (wsRef.current) {
        wsRef.current.close();
      }
    };
  }, []);

  return (
    <PageLayout
      title="WebSocket Debug"
      subtitle="Monitor and test WebSocket connections"
    >
      {/* Connection Status and URL Selection - Matches Image Header */}
      <Box sx={{ mb: 3, p: 2, border: "1px solid #ccc", borderRadius: "4px" }}>
        {" "}
        {/* Added border/padding for visual separation */}
        <Grid container spacing={2} alignItems="center">
          <Grid size={{ xs: 12, md: 6 }}>
            {/* Connection Status */}
            <Box
              sx={{
                display: "flex",
                alignItems: "center",
                mb: { xs: 2, md: 0 },
              }}
            >
              {" "}
              {/* Adjusted margin for mobile */}
              <Typography variant="h6" sx={{ mr: 1 }}>
                WebSocket Connection
              </Typography>
              <Chip
                icon={isConnected ? <WifiIcon /> : <WifiOffIcon />}
                label={isConnected ? "Connected" : "Disconnected"}
                color={isConnected ? "success" : "error"}
                size="small" // Smaller chip
              />
              <IconButton size="small" sx={{ ml: 1 }}>
                {" "}
                {/* Gear icon placeholder */}
                <SettingsIcon fontSize="small" />
              </IconButton>
            </Box>
            <Typography
              variant="subtitle2"
              color="text.secondary"
              sx={{ mb: 1 }}
            >
              {" "}
              {/* Subtitle */}
              Connect to WebSocket feeds and manage subscriptions
            </Typography>
            <FormControl fullWidth>
              <InputLabel>WebSocket URL</InputLabel>
              <Select
                value={selectedUrl}
                onChange={(e) => setSelectedUrl(e.target.value)}
                label="WebSocket URL"
                disabled={isConnected}
                endAdornment={
                  <InputAdornment position="end">
                    <IconButton
                      onClick={() => setShowNewUrlInput(!showNewUrlInput)}
                      edge="end"
                    >
                      <AddIcon />
                    </IconButton>
                  </InputAdornment>
                }
              >
                {wsUrls.map((url) => (
                  <MenuItem
                    key={url}
                    value={url}
                    sx={{
                      display: "flex",
                      justifyContent: "space-between",
                      alignItems: "center",
                    }}
                  >
                    <Typography variant="body2" sx={{ flex: 1 }}>
                      {url}
                    </Typography>
                    {wsUrls.length > 1 && (
                      <IconButton
                        size="small"
                        onClick={(e) => {
                          e.stopPropagation();
                          handleRemoveUrl(url);
                        }}
                      >
                        <DeleteIcon fontSize="small" />
                      </IconButton>
                    )}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
          </Grid>
          <Grid size={{ xs: 12, md: 6 }}>
            <Box
              sx={{
                display: "flex",
                gap: 2,
                alignItems: "center",
                justifyContent: "flex-end",
              }}
            >
              {" "}
              {/* Align to right */}
              <Button
                variant="contained"
                startIcon={isConnected ? <DisconnectIcon /> : <ConnectIcon />}
                onClick={isConnected ? disconnect : connect}
                sx={{ px: 4 }}
              >
                {isConnected ? "Disconnect" : "Connect"}
              </Button>
              {/* Removed Auto Reconnect Switch as it's not in the image */}
            </Box>
          </Grid>
        </Grid>
        {showNewUrlInput && (
          <Box sx={{ mt: 2, display: "flex", gap: 1 }}>
            <TextField
              fullWidth
              label="New WebSocket URL"
              value={newUrl}
              onChange={(e) => setNewUrl(e.target.value)}
              placeholder="wss://example.com/ws"
            />
            <Button
              variant="contained"
              onClick={handleAddUrl}
              disabled={!newUrl}
            >
              Add
            </Button>
            <Button
              variant="outlined"
              onClick={() => {
                setShowNewUrlInput(false);
                setNewUrl("");
              }}
            >
              Cancel
            </Button>
          </Box>
        )}
        {error && (
          <Alert severity="error" sx={{ mt: 2 }}>
            {error}
          </Alert>
        )}
      </Box>

      {/* Two-column layout below header */}
      <Grid container spacing={3}>
        {" "}
        {/* Added spacing between columns */}
        {/* Left Column: Channel Subscriptions and Send Custom Message */}
        <Grid size={{ xs: 12, md: 4 }}>
          {" "}
          {/* md=4 for left column */}
          {/* Channel Subscriptions - Matches Image */}
          <Card sx={{ mb: 3 }}>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Channel Subscriptions
              </Typography>
              <Typography
                variant="subtitle2"
                color="text.secondary"
                sx={{ mb: 2 }}
              >
                {" "}
                {/* Subtitle */}
                Manage WebSocket channel subscriptions
              </Typography>
              {PREDEFINED_SUBSCRIPTIONS.map((sub, index) => (
                <Box
                  key={index}
                  sx={{
                    display: "flex",
                    alignItems: "center",
                    justifyContent: "space-between",
                    mb: 1,
                    p: 1,
                    border: "1px solid #eee",
                    borderRadius: "4px",
                  }}
                >
                  {" "}
                  {/* Styled box for each subscription */}
                  <Box>
                    <Typography variant="body1">{sub.channel}</Typography>
                    <Typography variant="body2" color="text.secondary">
                      {sub.productIds.length > 0
                        ? sub.productIds.join(", ")
                        : "No products"}
                    </Typography>
                  </Box>
                  <IconButton
                    size="small"
                    onClick={() => subscribe(sub.channel, sub.productIds)}
                    disabled={isConnected}
                  >
                    {" "}
                    {/* Add button, disabled when connected */}
                    <AddIcon fontSize="small" />
                  </IconButton>
                </Box>
              ))}
              {/* Removed manual subscription input fields */}
              {subscriptions.length > 0 && (
                <Box sx={{ mt: 2 }}>
                  <Typography variant="subtitle2" gutterBottom>
                    Active Subscriptions:
                  </Typography>
                  <Box sx={{ display: "flex", flexWrap: "wrap", gap: 1 }}>
                    {subscriptions.map((sub, index) => (
                      <Chip
                        key={index}
                        label={`${sub.channel}: ${sub.productIds.join(", ")}`}
                        onDelete={() => unsubscribe(sub)}
                        color="primary"
                        variant="outlined"
                        size="small" // Smaller chip
                      />
                    ))}
                  </Box>
                </Box>
              )}
            </CardContent>
          </Card>
          {/* Send Custom Message - Matches Image Placement */}
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Send Custom Message
              </Typography>
              <Typography
                variant="subtitle2"
                color="text.secondary"
                sx={{ mb: 2 }}
              >
                {" "}
                {/* Subtitle */}
                Send a custom JSON message
              </Typography>
              <TextField
                fullWidth
                multiline
                rows={4}
                label="Message"
                value={customMessage}
                onChange={(e) => setCustomMessage(e.target.value)}
                placeholder='{"type": "subscribe", "channels": ["ticker"], "product_ids": ["BTC-USD"]}'
                sx={{ mb: 2 }} // Add margin below input
              />
              <Button
                variant="contained"
                startIcon={<SendIcon />}
                onClick={sendMessage}
                disabled={!isConnected || !customMessage}
                fullWidth // Make button full width
              >
                Send Message
              </Button>
            </CardContent>
          </Card>
        </Grid>
        {/* Right Column: Message Log */}
        <Grid size={{ xs: 12, md: 8 }}>
          {" "}
          {/* md=8 for right column */}
          {/* Message Log - Matches Image */}
          <Card>
            <CardContent>
              <Box
                sx={{
                  display: "flex",
                  justifyContent: "space-between",
                  alignItems: "center",
                  mb: 2,
                }}
              >
                <Box sx={{ display: "flex", alignItems: "center" }}>
                  {" "}
                  {/* Container for title and count */}
                  <Typography variant="h6" sx={{ mr: 1 }}>
                    Message Log
                  </Typography>
                  <Typography variant="h6" color="text.secondary">
                    ({messages.length})
                  </Typography>{" "}
                  {/* Message count */}
                </Box>
                <Box sx={{ display: "flex", gap: 1, alignItems: "center" }}>
                  {" "}
                  {/* Container for icons and filter */}
                  <IconButton size="small">
                    {" "}
                    {/* Download Icon Placeholder */}
                    <DownloadIcon fontSize="small" />
                  </IconButton>
                  <IconButton size="small" onClick={clearMessages}>
                    {" "}
                    {/* Trash Icon */}
                    <DeleteIcon fontSize="small" />
                  </IconButton>
                  <TextField
                    size="small"
                    label="Filter Messages"
                    value={messageFilter}
                    onChange={(e) => setMessageFilter(e.target.value)}
                    placeholder="Filter messages..." // Updated placeholder
                    sx={{ minWidth: 150 }} // Give filter some width
                  />
                  {/* Channel Filter Dropdown - To be implemented */}
                  <FormControl size="small" sx={{ minWidth: 120 }}>
                    <InputLabel>Channel</InputLabel>
                    <Select
                      value="All Channels" // Placeholder value
                      label="Channel"
                      onChange={() => {}} // Placeholder handler
                    >
                      <MenuItem value="All Channels">All Channels</MenuItem>
                      {/* Map through unique channels in messages to create menu items */}
                      {/* Example: <MenuItem value="ticker">ticker</MenuItem> */}
                    </Select>
                  </FormControl>
                </Box>
              </Box>

              <Box sx={{ maxHeight: 400, overflow: "auto" }}>
                {filteredMessages.map((message) => (
                  <Paper
                    key={message.id}
                    sx={{
                      p: 1,
                      mb: 1,
                      bgcolor:
                        message.type === "error"
                          ? "#fee2e2"
                          : message.type === "sent"
                          ? "#f0fdf4"
                          : message.type === "received"
                          ? "#f8fafc"
                          : "#f1f5f9",
                    }}
                  >
                    <Box
                      sx={{
                        display: "flex",
                        justifyContent: "space-between",
                        mb: 0.5,
                      }}
                    >
                      <Typography variant="caption" color="text.secondary">
                        {message.timestamp.toLocaleTimeString()}
                      </Typography>
                      <Chip
                        label={message.type}
                        size="small"
                        color={
                          message.type === "error"
                            ? "error"
                            : message.type === "sent"
                            ? "success"
                            : message.type === "received"
                            ? "primary"
                            : "default"
                        }
                      />
                    </Box>
                    <Typography
                      variant="body2"
                      component="pre"
                      sx={{
                        whiteSpace: "pre-wrap",
                        wordBreak: "break-word",
                        fontFamily: "monospace",
                        fontSize: "0.875rem",
                      }}
                    >
                      {JSON.stringify(message.data, null, 2)}
                    </Typography>
                  </Paper>
                ))}
              </Box>
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </PageLayout>
  );
};
