import { PropsWithChildren, ReactNode } from 'react'
import clsx from 'clsx'

interface Props extends PropsWithChildren {
  title?: string | ReactNode
  actions?: ReactNode
  className?: string
}

export function Card({ title, actions, children, className }: Props) {
  return (
    <section className={clsx('rounded-2xl bg-card p-6 shadow-card border border-border/60', className)}>
      {(title || actions) && (
        <header className="mb-4 flex items-center justify-between gap-3">
          {typeof title === 'string' ? <h2 className="text-lg font-semibold">{title}</h2> : title}
          {actions}
        </header>
      )}
      <div>{children}</div>
    </section>
  )
}
