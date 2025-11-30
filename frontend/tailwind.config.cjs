/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ['./index.html', './src/**/*.{ts,tsx}'],
  theme: {
    extend: {
      colors: {
        surface: '#f7f8fb',
        card: '#ffffff',
        accent: '#4f46e5',
        accentMuted: '#eef2ff',
        textPrimary: '#111827',
        textSecondary: '#6b7280',
        border: '#e5e7eb',
      },
      boxShadow: {
        card: '0 10px 30px rgba(15, 23, 42, 0.08)',
      },
    },
  },
  plugins: [],
}
