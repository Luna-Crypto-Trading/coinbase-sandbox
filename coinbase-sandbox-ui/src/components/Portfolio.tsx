import { useState } from "react";
import {
  Box,
  Card,
  CardContent,
  Typography,
  Grid,
  Button,
  Alert,
  Snackbar,
} from "@mui/material";
import { Add as AddIcon, Remove as RemoveIcon } from "@mui/icons-material";
import { useAccounts, useTransaction } from "../hooks/useAccounts";
import { PageLayout } from "./PageLayout";
import { TransactionDialog } from "./TransactionDialog";

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

interface TransactionPayload {
  amount: string;
  currency: string;
}

export const Portfolio = () => {
  const [selectedAccount, setSelectedAccount] = useState<Account | null>(null);
  const [transactionType, setTransactionType] = useState<
    "deposit" | "withdraw" | null
  >(null);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [snackbar, setSnackbar] = useState({
    open: false,
    message: "",
    severity: "success" as "success" | "error",
  });

  const { data: accounts } = useAccounts();
  const transaction = useTransaction();

  const handleOpenDialog = (account: Account, type: "deposit" | "withdraw") => {
    setSelectedAccount(account);
    setTransactionType(type);
    setDialogOpen(true);
  };

  const handleTransaction = async (payload: TransactionPayload) => {
    if (!selectedAccount || !transactionType) return;

    try {
      await transaction.mutateAsync({
        accountId: selectedAccount.uuid,
        type: transactionType,
        payload,
      });

      setSnackbar({
        open: true,
        message: `${
          transactionType === "deposit" ? "Deposit" : "Withdrawal"
        } successful`,
        severity: "success",
      });
    } catch {
      setSnackbar({
        open: true,
        message: "Transaction failed",
        severity: "error",
      });
    }
  };

  return (
    <PageLayout
      title="Portfolio"
      subtitle="Manage your cryptocurrency portfolio"
    >
      {/* Main Content Grid */}
      <Grid container spacing={3} sx={{ mb: 4 }}>
        {accounts?.map((account) => (
          <Grid size={{ xs: 12, sm: 6, md: 4 }} key={account.uuid}>
            <Card>
              <CardContent>
                <Typography variant="h6" gutterBottom>
                  {account.name}
                </Typography>
                <Typography variant="h4" sx={{ mb: 2 }}>
                  {account.available_balance.value} {account.currency}
                </Typography>
                <Typography
                  variant="body2"
                  color="text.secondary"
                  sx={{ mb: 2 }}
                >
                  Type: {account.type.replace("ACCOUNT_TYPE_", "")}
                </Typography>
                <Box sx={{ display: "flex", gap: 1 }}>
                  <Button
                    variant="contained"
                    startIcon={<AddIcon />}
                    onClick={() => handleOpenDialog(account, "deposit")}
                    fullWidth
                  >
                    Deposit
                  </Button>
                  <Button
                    variant="outlined"
                    startIcon={<RemoveIcon />}
                    onClick={() => handleOpenDialog(account, "withdraw")}
                    fullWidth
                  >
                    Withdraw
                  </Button>
                </Box>
              </CardContent>
            </Card>
          </Grid>
        ))}
      </Grid>

      <TransactionDialog
        open={dialogOpen}
        onClose={() => setDialogOpen(false)}
        account={selectedAccount}
        type={transactionType}
        onSubmit={handleTransaction}
      />

      <Snackbar
        open={snackbar.open}
        autoHideDuration={6000}
        onClose={() => setSnackbar({ ...snackbar, open: false })}
      >
        <Alert
          onClose={() => setSnackbar({ ...snackbar, open: false })}
          severity={snackbar.severity}
          sx={{ width: "100%" }}
        >
          {snackbar.message}
        </Alert>
      </Snackbar>
    </PageLayout>
  );
};
