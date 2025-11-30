import { useQuery } from '@tanstack/react-query'
import { Card } from '../components/Card'
import { ApiClient } from '../api/client'
import { Badge } from '../components/Badge'

export default function ReferencePage() {
  const { data: specializations } = useQuery({
    queryKey: ['specializations'],
    queryFn: ApiClient.getSpecializations,
  })

  return (
    <Card title="Справочник специализаций">
      {!specializations || specializations.length === 0 ? (
        <div className="rounded-lg border border-dashed border-border px-4 py-6 text-sm text-textSecondary">
          📚 Нет данных
        </div>
      ) : (
        <div className="grid gap-3 md:grid-cols-2">
          {specializations.map((s) => (
            <div key={s.code} className="rounded-xl border border-border/70 bg-white px-3 py-2 shadow-sm">
              <div className="font-medium">{s.title}</div>
              <div className="mt-1 text-sm text-textSecondary flex items-center gap-2">
                Код: <Badge label={s.code} tone="neutral" />
              </div>
            </div>
          ))}
        </div>
      )}
    </Card>
  )
}
