import { createTheme } from '@mui/material/styles';

export const theme = createTheme({
  palette: {
    primary: {
      main: '#0f172a',
    },
    secondary: {
      main: '#64748b',
    },
    background: {
      default: '#f8fafc',
      paper: '#ffffff',
    },
    divider: '#e2e8f0',
  },
  typography: {
    h1: {
      fontWeight: 800,
      fontSize: '2.25rem',
      lineHeight: 1.2,
      color: '#0f172a',
    },
    body1: {
      fontWeight: 400,
      fontSize: '1rem',
    },
  },
  components: {
    MuiAppBar: {
      styleOverrides: {
        root: {
          height: 64,
        },
      },
    },
    MuiChip: {
      styleOverrides: {
        root: {
          '&.sandbox-mode': {
            backgroundColor: '#fef3c7',
            color: '#92400e',
            fontWeight: 600,
            fontSize: '0.75rem',
            padding: '8px 16px',
            border: '1px solid #fbbf24',
            borderRadius: '6px',
          },
        },
      },
    },
    MuiTab: {
      styleOverrides: {
        root: {
          textTransform: 'none',
          fontWeight: 600,
          fontSize: '0.875rem',
          minHeight: 48,
          color: '#64748b',
          '&.Mui-selected': {
            color: '#0f172a',
          },
        },
      },
    },
    MuiTabs: {
      styleOverrides: {
        indicator: {
          backgroundColor: '#0f172a',
        },
      },
    },
    MuiPaper: {
      styleOverrides: {
        root: {
          '&.dashboard-paper': {
            borderRadius: 8,
            overflow: 'hidden',
            border: '1px solid #e2e8f0',
            boxShadow: '0 1px 3px 0 rgb(0 0 0 / 0.1), 0 1px 2px -1px rgb(0 0 0 / 0.1)',
          },
        },
      },
    },
  },
}); 