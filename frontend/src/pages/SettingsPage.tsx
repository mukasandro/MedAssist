import { useEffect, useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Card } from '../components/Card'
import { Button } from '../components/Button'
import { Input } from '../components/Input'
import { ApiClient } from '../api/client'

export default function SettingsPage() {
  const queryClient = useQueryClient()
  const { data, isLoading, error } = useQuery({
    queryKey: ['system-settings'],
    queryFn: ApiClient.getSystemSettings,
  })

  const [llmGatewayUrl, setLlmGatewayUrl] = useState('')
  const [saveMessage, setSaveMessage] = useState<string | null>(null)

  useEffect(() => {
    if (!data) return
    setLlmGatewayUrl(data.llmGatewayUrl ?? '')
  }, [data])

  const updateMutation = useMutation({
    mutationFn: ApiClient.updateSystemSettings,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['system-settings'] })
      setSaveMessage('Сохранено')
      setTimeout(() => setSaveMessage(null), 1500)
    },
  })

  return (
    <Card
      title="Настройки"
      actions={
        <Button
          variant="primary"
          disabled={isLoading || updateMutation.isPending}
          onClick={() =>
            updateMutation.mutate({
              llmGatewayUrl: llmGatewayUrl.trim(),
            })
          }
        >
          Сохранить
        </Button>
      }
    >
      {isLoading && <div className="text-sm text-textSecondary">⏳ Загрузка...</div>}
      {error && (
        <div className="rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
          Не удалось загрузить настройки. Проверьте API.
        </div>
      )}

      {!isLoading && !error && (
        <div className="grid gap-3 md:grid-cols-2">
          <Input
            label="LLM Gateway URL"
            value={llmGatewayUrl}
            placeholder="http://localhost:8090"
            onChange={(e) => setLlmGatewayUrl(e.currentTarget.value)}
          />
          <div className="flex items-end text-xs text-textSecondary">
            Последнее обновление:{' '}
            {data?.updatedAt ? new Date(data.updatedAt).toLocaleString() : '—'}
          </div>
          {saveMessage && <div className="text-sm text-green-600">{saveMessage}</div>}
        </div>
      )}
    </Card>
  )
}
