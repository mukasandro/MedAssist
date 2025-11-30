import { BUILD_DATE, BUILD_VERSION } from '../version'

interface Props {
  onToggleSidebar: () => void
}

export function Topbar({ onToggleSidebar }: Props) {
  return (
    <header className="sticky top-0 z-10 flex h-16 items-center justify-between border-b border-border/70 bg-white/90 px-4 backdrop-blur">
      <button
        aria-label="Toggle sidebar"
        onClick={onToggleSidebar}
        className="rounded-lg border border-border px-3 py-2 text-sm font-medium text-textSecondary transition hover:border-accent hover:text-accent focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-500"
      >
        ☰
      </button>
      <div className="flex items-center gap-3">
        <div className="rounded-lg bg-red-100 px-3 py-1 text-sm font-semibold uppercase text-red-700">
          build {BUILD_VERSION} · {BUILD_DATE}
        </div>
        <span className="h-9 w-9 rounded-full bg-accent text-white flex items-center justify-center text-sm font-semibold">
          DR
        </span>
      </div>
    </header>
  )
}
