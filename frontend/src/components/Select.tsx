import { SelectHTMLAttributes } from 'react'
import clsx from 'clsx'

interface Option {
  label: string
  value: string | number
}

interface Props extends SelectHTMLAttributes<HTMLSelectElement> {
  label?: string
  options: Option[]
  error?: string
}

export function Select({ label, options, error, className, ...rest }: Props) {
  return (
    <label className="flex flex-col gap-1 text-sm text-textSecondary">
      {label && <span>{label}</span>}
      <select
        className={clsx(
          'w-full rounded-lg border border-border bg-white px-3 py-2 text-textPrimary shadow-sm transition focus:border-accent focus:outline-none focus:ring-2 focus:ring-indigo-100',
          error && 'border-red-400 focus:ring-red-100',
          className
        )}
        {...rest}
      >
        {options.map((o) => (
          <option key={o.value} value={o.value}>
            {o.label}
          </option>
        ))}
      </select>
      {error && <span className="text-xs text-red-500">{error}</span>}
    </label>
  )
}
