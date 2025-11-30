import clsx from 'clsx'

interface Item {
  key: string
  label: string
  icon?: string
}

interface Props {
  items: Item[]
  active: string
  open: boolean
  onSelect: (key: string) => void
}

export function Sidebar({ items, active, open, onSelect }: Props) {
  return (
    <nav
      aria-label="Главная навигация"
      className={clsx(
        'fixed left-0 top-0 h-screen border-r border-border/70 bg-white shadow-sm transition-all',
        open ? 'w-64' : 'w-16'
      )}
    >
      <div className="flex h-16 items-center justify-center border-b border-border/60 text-lg font-semibold text-accent">
        {open ? 'MedAssist Admin' : 'MA'}
      </div>
      <ul className="mt-4 space-y-1 px-2">
        {items.map((item) => (
          <li key={item.key}>
            <button
              className={clsx(
                'w-full rounded-xl px-3 py-2 text-left text-sm font-medium transition-colors hover:bg-accentMuted hover:text-accent',
                active === item.key && 'bg-accent text-white shadow-sm hover:text-white'
              )}
              onClick={() => onSelect(item.key)}
            >
              <span className="inline-flex items-center gap-2">
                <span aria-hidden="true">{item.icon}</span>
                {open && <span>{item.label}</span>}
              </span>
            </button>
          </li>
        ))}
      </ul>
    </nav>
  )
}
