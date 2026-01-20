import { useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Card } from '../components/Card'
import { Button } from '../components/Button'
import { Input } from '../components/Input'
import { Textarea } from '../components/Textarea'
import { Modal } from '../components/Modal'
import { ApiClient } from '../api/client'
import type { CreateStaticContentRequest, StaticContentDto, UpdateStaticContentRequest } from '../api/types'

const previewValue = (value: string) => (value.length > 120 ? `${value.slice(0, 120)}...` : value)

export default function StaticContentAdminPage() {
  const queryClient = useQueryClient()
  const { data: items, isLoading, error } = useQuery({
    queryKey: ['admin-static-content'],
    queryFn: ApiClient.getStaticContent,
  })
  const [selectedIds, setSelectedIds] = useState<string[]>([])
  const [editorOpen, setEditorOpen] = useState(false)
  const [form, setForm] = useState<StaticContentDto | null>(null)
  const [saveMessage, setSaveMessage] = useState<string | null>(null)
  const [createOpen, setCreateOpen] = useState(false)
  const [createForm, setCreateForm] = useState<CreateStaticContentRequest>({
    code: '',
    title: '',
    value: '',
  })

  const updateMutation = useMutation({
    mutationFn: (payload: { id: string; body: UpdateStaticContentRequest }) =>
      ApiClient.updateStaticContent(payload.id, payload.body),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin-static-content'] })
      setSaveMessage('Сохранено')
      setTimeout(() => setSaveMessage(null), 1500)
    },
  })

  const createMutation = useMutation({
    mutationFn: (payload: CreateStaticContentRequest) => ApiClient.createStaticContent(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin-static-content'] })
      setCreateOpen(false)
      setCreateForm({ code: '', title: '', value: '' })
    },
  })

  const deleteMutation = useMutation({
    mutationFn: async (ids: string[]) => {
      await Promise.all(ids.map((id) => ApiClient.deleteStaticContent(id)))
    },
    onSuccess: () => {
      setSelectedIds([])
      setEditorOpen(false)
      setForm(null)
      queryClient.invalidateQueries({ queryKey: ['admin-static-content'] })
    },
  })

  const openItem = (id: string) => {
    const item = items?.find((x) => x.id === id)
    if (!item) return
    setSelectedIds((prev) => (prev.includes(id) ? prev : [...prev, id]))
    setForm(item)
    setEditorOpen(true)
  }

  const toggleSelect = (id: string, checked: boolean) => {
    setSelectedIds((prev) => (checked ? [...prev, id] : prev.filter((x) => x !== id)))
  }

  const handleChange = (key: keyof StaticContentDto, value: any) => {
    setForm((prev) => (prev ? { ...prev, [key]: value } : prev))
  }

  const handleCreateChange = (key: keyof CreateStaticContentRequest, value: any) => {
    setCreateForm((prev) => ({ ...prev, [key]: value }))
  }

  return (
    <>
      <Card
        title="Статика"
        actions={
          <div className="flex gap-2">
            <Button variant="primary" onClick={() => setCreateOpen(true)}>
              Новая запись
            </Button>
            <Button
              variant="danger"
              disabled={selectedIds.length === 0 || deleteMutation.isPending}
              onClick={() => deleteMutation.mutate(selectedIds)}
            >
              Удалить
            </Button>
          </div>
        }
      >
        {isLoading && <div className="text-sm text-textSecondary">⏳ Загрузка...</div>}
        {error && (
          <div className="rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
            Не удалось загрузить список статичных текстов. Проверьте API.
          </div>
        )}
        {!isLoading && !error && (!items || items.length === 0) && (
          <div className="rounded-lg border border-dashed border-border px-4 py-6 text-sm text-textSecondary">
            📭 Пока нет записей.
          </div>
        )}
        {items && items.length > 0 && (
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-border/70 text-sm">
              <thead className="bg-surface text-left uppercase text-xs font-semibold text-textSecondary">
                <tr>
                  <th className="px-3 py-2 w-10"></th>
                  <th className="px-3 py-2">Код</th>
                  <th className="px-3 py-2">Название</th>
                  <th className="px-3 py-2">Текст</th>
                  <th className="px-3 py-2">Обновлено</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border/60">
                {items.map((item) => (
                  <tr
                    key={item.id}
                    className="cursor-pointer hover:bg-accentMuted"
                    onClick={() => openItem(item.id)}
                    aria-label={`Открыть запись ${item.code}`}
                  >
                    <td className="px-3 py-2">
                      <input
                        type="checkbox"
                        checked={selectedIds.includes(item.id)}
                        onClick={(e) => e.stopPropagation()}
                        onChange={(e) => toggleSelect(item.id, e.currentTarget.checked)}
                      />
                    </td>
                    <td className="px-3 py-2 font-medium text-textPrimary">{item.code}</td>
                    <td className="px-3 py-2 text-textSecondary">{item.title ?? '—'}</td>
                    <td className="px-3 py-2 text-textSecondary">{previewValue(item.value)}</td>
                    <td className="px-3 py-2 text-textSecondary">
                      {new Date(item.updatedAt).toLocaleString()}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </Card>

      <Modal
        open={createOpen}
        onClose={() => setCreateOpen(false)}
        title="Новая запись"
        footer={
          <>
            <Button variant="secondary" onClick={() => setCreateOpen(false)}>
              Отмена
            </Button>
            <Button
              variant="primary"
              disabled={createMutation.isPending || !createForm.code || !createForm.value}
              onClick={() => createMutation.mutate(createForm)}
            >
              Создать
            </Button>
          </>
        }
      >
        <div className="grid gap-3 md:grid-cols-2">
          <Input
            label="Код"
            value={createForm.code}
            onChange={(e) => handleCreateChange('code', e.currentTarget.value)}
          />
          <Input
            label="Название"
            value={createForm.title ?? ''}
            onChange={(e) => handleCreateChange('title', e.currentTarget.value)}
          />
          <div className="md:col-span-2">
            <Textarea
              label="Текст"
              value={createForm.value}
              onChange={(e) => handleCreateChange('value', e.currentTarget.value)}
            />
          </div>
        </div>
      </Modal>

      <Modal
        open={editorOpen}
        onClose={() => {
          setEditorOpen(false)
          setSelectedIds([])
          setForm(null)
        }}
        title={form ? `Редактирование: ${form.code}` : 'Запись'}
        footer={
          <>
            <Button
              variant="secondary"
              onClick={() => {
                setEditorOpen(false)
                setSelectedIds([])
                setForm(null)
              }}
            >
              Закрыть
            </Button>
            <Button
              variant="primary"
              disabled={!form || updateMutation.isPending}
              onClick={() => {
                if (!form) return
                const payload: UpdateStaticContentRequest = {
                  code: form.code,
                  title: form.title ?? null,
                  value: form.value,
                }
                updateMutation.mutate({ id: form.id, body: payload })
              }}
            >
              Сохранить
            </Button>
          </>
        }
      >
        {form ? (
          <div className="grid gap-3 md:grid-cols-2">
            <Input label="Код" value={form.code} onChange={(e) => handleChange('code', e.currentTarget.value)} />
            <Input
              label="Название"
              value={form.title ?? ''}
              onChange={(e) => handleChange('title', e.currentTarget.value)}
            />
            <div className="md:col-span-2">
              <Textarea
                label="Текст"
                value={form.value}
                onChange={(e) => handleChange('value', e.currentTarget.value)}
              />
            </div>
            {saveMessage && <div className="text-sm text-green-600">{saveMessage}</div>}
          </div>
        ) : (
          <div className="text-sm text-textSecondary">Нет данных</div>
        )}
      </Modal>
    </>
  )
}
