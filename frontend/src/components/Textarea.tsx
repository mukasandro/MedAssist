import { TextareaHTMLAttributes } from 'react'
import clsx from 'clsx'

interface Props extends TextareaHTMLAttributes<HTMLTextAreaElement> {
  label?: string
  error?: string
}

export function Textarea({ label, error, className, ...rest }: Props) {
  return (
    <label className="flex flex-col gap-1 text-sm text-textSecondary">
      {label && <span>{label}</span>}
      <textarea
        className={clsx(
          'w-full rounded-lg border border-border bg-white px-3 py-2 text-textPrimary shadow-sm transition focus:border-accent focus:outline-none focus:ring-2 focus:ring-indigo-100',
          error && 'border-red-400 focus:ring-red-100',
          className
        )}
        {...rest}
      />
      {error && <span className="text-xs text-red-500">{error}</span>}
    </label>
  )
}
