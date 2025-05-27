import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';

interface Account {
  uuid: string;
  name: string;
  currency: string;
  available_balance: {
    value: string;
    currency: string;
  };
  isDefault: boolean;
  active: boolean;
  created_at: string;
  updated_at: string;
  deleted_at: string | null;
  type: string;
}

interface TransactionPayload {
  amount: string;
  currency: string;
}

// Fetch accounts
const fetchAccounts = async (): Promise<Account[]> => {
  const response = await fetch('/api/v3/brokerage/accounts');
  const data = await response.json();
  return data.accounts || [];
};

// Transaction mutation
const performTransaction = async ({
  accountId,
  type,
  payload,
}: {
  accountId: string;
  type: 'deposit' | 'withdraw';
  payload: TransactionPayload;
}): Promise<void> => {
  const response = await fetch(
    `/api/v3/brokerage/sandbox/accounts/${accountId}/${type}`,
    {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(payload),
    }
  );

  if (!response.ok) {
    throw new Error('Transaction failed');
  }
};

// Hook for fetching accounts
export const useAccounts = () => {
  return useQuery({
    queryKey: ['accounts'],
    queryFn: fetchAccounts,
  });
};

// Hook for transactions
export const useTransaction = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: performTransaction,
    onSuccess: () => {
      // Invalidate and refetch accounts after successful transaction
      queryClient.invalidateQueries({ queryKey: ['accounts'] });
    },
  });
}; 