import { InputHTMLAttributes } from 'react'
import clsx from 'clsx'

interface Props extends Omit<InputHTMLAttributes<HTMLInputElement>, 'type'> {
  label?: string
}

export function Toggle({ label, className, ...rest }: Props) {
  return (
    <label className="flex items-center gap-2 text-sm text-textSecondary cursor-pointer">
      <input
        type="checkbox"
        className={clsx(
          'peer relative h-5 w-10 appearance-none rounded-full border border-border bg-white transition checked:bg-accent',
          className
        )}
        {...rest}
      />
      <span className="pointer-events-none absolute ml-1 h-4 w-4 rounded-full bg-white transition peer-checked:translate-x-5 shadow-sm" />
      {label && <span className="pl-4">{label}</span>}
    </label>
  )
}
