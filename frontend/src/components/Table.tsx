import { PropsWithChildren } from 'react'
import clsx from 'clsx'

export function Table({ children }: PropsWithChildren) {
  return (
    <div className="overflow-hidden rounded-xl border border-border/70 bg-white">
      <table className="min-w-full divide-y divide-border/70">{children}</table>
    </div>
  )
}

export function TableHeader({ children }: PropsWithChildren) {
  return <thead className="bg-surface text-left text-xs font-semibold uppercase text-textSecondary">{children}</thead>
}

export function TableRow({ children, className }: PropsWithChildren<{ className?: string }>) {
  return <tr className={clsx('divide-x divide-border/50', className)}>{children}</tr>
}

export function Th({ children }: PropsWithChildren) {
  return <th className="px-4 py-3">{children}</th>
}

export function Td({ children }: PropsWithChildren) {
  return <td className="px-4 py-3 text-sm text-textPrimary">{children}</td>
}
