import { Card } from '../components/Card'
import { Badge } from '../components/Badge'
import { Button } from '../components/Button'

const stats = [
  { label: 'Активные пациенты', value: '24', change: '+3' },
  { label: 'Диалоги сегодня', value: '12', change: '+2' },
  { label: 'Новых пациентов', value: '4', change: '+1' },
]

export default function DashboardPage() {
  return (
    <div className="grid gap-4">
      <div className="grid gap-4 md:grid-cols-3">
        {stats.map((s) => (
          <Card key={s.label} className="shadow-sm">
            <div className="text-sm text-textSecondary">{s.label}</div>
            <div className="mt-2 flex items-baseline gap-2">
              <div className="text-3xl font-semibold">{s.value}</div>
              <Badge label={s.change} tone={s.change.startsWith('+') ? 'success' : 'warning'} />
            </div>
          </Card>
        ))}
      </div>
      <Card title="Быстрые действия">
        <div className="flex flex-wrap gap-3">
          <Button variant="primary">Создать пациента</Button>
          <Button variant="secondary">Начать диалог</Button>
          <Button variant="ghost">Обновить справочники</Button>
        </div>
      </Card>
    </div>
  )
}
