import { useState } from "react";
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  TextField,
  Box,
} from "@mui/material";

interface Account {
  uuid: string;
  name: string;
  currency: string;
  available_balance: {
    value: string;
    currency: string;
  };
  type: string;
}

interface TransactionDialogProps {
  open: boolean;
  onClose: () => void;
  account: Account | null;
  type: "deposit" | "withdraw" | null;
  onSubmit: (payload: { amount: string; currency: string }) => void;
}

export const TransactionDialog: React.FC<TransactionDialogProps> = ({
  open,
  onClose,
  account,
  type,
  onSubmit,
}) => {
  const [amount, setAmount] = useState("");
  const [error, setError] = useState("");

  const handleSubmit = () => {
    if (!amount || isNaN(Number(amount)) || Number(amount) <= 0) {
      setError("Please enter a valid amount");
      return;
    }
    if (!account) return;

    onSubmit({
      amount,
      currency: account.currency,
    });
    setAmount("");
    setError("");
    onClose();
  };

  return (
    <Dialog open={open} onClose={onClose}>
      <DialogTitle>
        {type === "deposit" ? "Deposit Funds" : "Withdraw Funds"}
      </DialogTitle>
      <DialogContent>
        <Box sx={{ pt: 2 }}>
          <TextField
            autoFocus
            label="Amount"
            type="number"
            fullWidth
            value={amount}
            onChange={(e) => setAmount(e.target.value)}
            error={!!error}
            helperText={error}
            InputProps={{
              endAdornment: account?.currency,
            }}
          />
        </Box>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Cancel</Button>
        <Button onClick={handleSubmit} variant="contained">
          {type === "deposit" ? "Deposit" : "Withdraw"}
        </Button>
      </DialogActions>
    </Dialog>
  );
};
