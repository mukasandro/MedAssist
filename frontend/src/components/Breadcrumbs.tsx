interface Crumb {
  label: string
}

interface Props {
  path: Crumb[]
}

export function Breadcrumbs({ path }: Props) {
  return (
    <nav aria-label="Хлебные крошки" className="text-sm text-textSecondary">
      <ol className="flex items-center gap-2">
        {path.map((item, idx) => (
          <li key={idx} className="flex items-center gap-2">
            <span className="text-textSecondary">{item.label}</span>
            {idx < path.length - 1 && <span className="text-border">/</span>}
          </li>
        ))}
      </ol>
    </nav>
  )
}
