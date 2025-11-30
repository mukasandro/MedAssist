import { ButtonHTMLAttributes } from 'react'
import clsx from 'clsx'

type Variant = 'primary' | 'secondary' | 'ghost' | 'danger'

interface Props extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: Variant
  fullWidth?: boolean
}

const base =
  'inline-flex items-center justify-center rounded-lg px-4 py-2 text-sm font-medium transition-colors focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2'

const variants: Record<Variant, string> = {
  primary:
    'bg-accent text-white hover:bg-indigo-600 focus-visible:outline-indigo-500 disabled:bg-indigo-300 disabled:cursor-not-allowed',
  secondary:
    'bg-card text-textPrimary border border-border hover:border-indigo-200 hover:text-indigo-700 focus-visible:outline-indigo-500 disabled:opacity-60 disabled:cursor-not-allowed',
  ghost: 'text-textSecondary hover:text-textPrimary hover:bg-accentMuted focus-visible:outline-indigo-500',
  danger:
    'bg-red-500 text-white hover:bg-red-600 focus-visible:outline-red-500 disabled:bg-red-300 disabled:cursor-not-allowed',
}

export function Button({ variant = 'primary', fullWidth, className, ...rest }: Props) {
  return (
    <button
      className={clsx(base, variants[variant], fullWidth && 'w-full', className)}
      {...rest}
      aria-pressed={rest['aria-pressed']}
    />
  )
}
