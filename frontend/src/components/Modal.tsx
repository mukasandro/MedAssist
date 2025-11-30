import { PropsWithChildren, ReactNode, MouseEvent } from 'react'
import clsx from 'clsx'

interface Props extends PropsWithChildren {
  open: boolean
  title?: string | ReactNode
  onClose: () => void
  footer?: ReactNode
  className?: string
}

export function Modal({ open, title, onClose, children, footer, className }: Props) {
  if (!open) return null

  const handleBackdrop = (e: MouseEvent<HTMLDivElement>) => {
    if (e.target === e.currentTarget) onClose()
  }

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/30 backdrop-blur-sm"
      role="dialog"
      aria-modal="true"
      onClick={handleBackdrop}
    >
      <div className={clsx('w-full max-w-3xl rounded-2xl bg-white p-6 shadow-2xl', className)}>
        <div className="mb-4 flex items-center justify-between">
          {typeof title === 'string' ? <h3 className="text-lg font-semibold">{title}</h3> : title}
          <button
            aria-label="Close"
            onClick={onClose}
            className="rounded-lg border border-border px-2 py-1 text-sm text-textSecondary hover:text-textPrimary focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-500"
          >
            ✕
          </button>
        </div>
        <div className="max-h-[70vh] overflow-y-auto pr-1">{children}</div>
        {footer && <div className="mt-4 flex items-center justify-end gap-2">{footer}</div>}
      </div>
    </div>
  )
}
