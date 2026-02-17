import { PropsWithChildren, useState } from 'react'
import clsx from 'clsx'
import { Sidebar } from './Sidebar'
import { Topbar } from './Topbar'

interface NavItem {
  key: string
  label: string
  icon?: string
}

interface Props extends PropsWithChildren {
  navItems: NavItem[]
  active: string
  onSelect: (key: string) => void
  onLogout?: () => void
}

export function Layout({ navItems, active, onSelect, onLogout, children }: Props) {
  const [sidebarOpen, setSidebarOpen] = useState(true)

  return (
    <div className="min-h-screen bg-surface text-textPrimary">
      <Topbar onToggleSidebar={() => setSidebarOpen((v) => !v)} onLogout={onLogout} />
      <div className="flex">
        <Sidebar items={navItems} active={active} open={sidebarOpen} onSelect={onSelect} />
        <main className={clsx('flex-1 transition-all', sidebarOpen ? 'ml-64' : 'ml-16')}>
          <div className="w-full px-4 py-6">{children}</div>
        </main>
      </div>
    </div>
  )
}
