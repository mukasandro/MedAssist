import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { Card } from '../components/Card'
import { Badge } from '../components/Badge'
import { Button } from '../components/Button'
import { ApiClient } from '../api/client'
import type { DoctorPublicDto, UpdateDoctorRequest } from '../api/types'
import { Input } from '../components/Input'
import { Toggle } from '../components/Toggle'
import { Modal } from '../components/Modal'

type DoctorForm = {
  id: string
  specializationCode: string
  telegramUserId?: number | null
  tokenBalance: number
  nickname?: string | null
  lastSelectedPatientId: string
  verified: boolean
}

export default function DoctorsAdminPage() {
  const queryClient = useQueryClient()
  const { data: doctors, isLoading, error } = useQuery({ queryKey: ['admin-doctors'], queryFn: ApiClient.getDoctors })
  const [selectedIds, setSelectedIds] = useState<string[]>([])
  const [editorOpen, setEditorOpen] = useState(false)
  const [form, setForm] = useState<DoctorForm | null>(null)
  const [saveMessage, setSaveMessage] = useState<string | null>(null)
  const [saveError, setSaveError] = useState<string | null>(null)
  const [topUpTokens, setTopUpTokens] = useState<string>('1000')
  const testMutation = useMutation({
    mutationFn: ApiClient.createDoctorTest,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin-doctors'] })
    },
  })

  const deleteMutation = useMutation({
    mutationFn: async (ids: string[]) => {
      await Promise.all(ids.map((id) => ApiClient.deleteDoctor(id)))
    },
    onSuccess: () => {
      setSelectedIds([])
      setEditorOpen(false)
      setForm(null)
      queryClient.invalidateQueries({ queryKey: ['admin-doctors'] })
    },
  })

  const updateMutation = useMutation({
    mutationFn: (payload: { id: string; body: UpdateDoctorRequest }) =>
      ApiClient.updateDoctor(payload.id, payload.body),
    onSuccess: () => {
      setSaveError(null)
      queryClient.invalidateQueries({ queryKey: ['admin-doctors'] })
      setSaveMessage('Сохранено')
      setTimeout(() => setSaveMessage(null), 1500)
    },
    onError: (err: any) => {
      const detail = err?.response?.data?.detail
      setSaveError(typeof detail === 'string' && detail.length > 0 ? detail : 'Не удалось сохранить врача.')
    },
  })

  const topUpMutation = useMutation({
    mutationFn: ApiClient.topUpDoctorTokens,
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['admin-doctors'] })
      setForm((prev) => (prev ? { ...prev, tokenBalance: data.tokenBalance } : prev))
      setSaveMessage(`Баланс пополнен. Текущий остаток: ${data.tokenBalance}`)
      setTimeout(() => setSaveMessage(null), 1800)
    },
  })

  const openNew = () => {
    setSelectedIds([])
    setTopUpTokens('1000')
    setSaveError(null)
    setForm({
      id: 'new',
      specializationCode: '',
      telegramUserId: null,
      tokenBalance: 0,
      nickname: '',
      lastSelectedPatientId: '',
      verified: false,
    })
    setEditorOpen(true)
  }

  const openDoctor = (id: string) => {
    const doc = doctors?.find((d) => d.id === id)
    if (!doc) return
    setTopUpTokens('1000')
    setSaveError(null)
    setSelectedIds((prev) => (prev.includes(id) ? prev : [...prev, id]))
    setForm({
      id: doc.id,
      specializationCode: doc.specializations?.[0]?.code ?? '',
      telegramUserId: doc.telegramUserId ?? null,
      tokenBalance: doc.tokenBalance ?? 0,
      nickname: doc.nickname ?? '',
      lastSelectedPatientId: doc.lastSelectedPatientId ?? '',
      verified: doc.verified,
    })
    setEditorOpen(true)
  }

  const toggleSelect = (id: string, checked: boolean) => {
    setSelectedIds((prev) => (checked ? [...prev, id] : prev.filter((x) => x !== id)))
  }

  const handleChange = (key: keyof DoctorForm, value: any) => {
    setForm((prev) => (prev ? { ...prev, [key]: value } : prev))
  }

  return (
    <>
      <Card
        title="Врачи"
        actions={
          <div className="flex gap-2">
            <Button variant="primary" onClick={openNew}>
              Новая
            </Button>
            <Button variant="secondary" onClick={() => testMutation.mutate()}>
              Тестовый врач
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
            Не удалось загрузить список врачей. Проверьте API.
          </div>
        )}
        {!isLoading && !error && (!doctors || doctors.length === 0) && (
          <div className="rounded-lg border border-dashed border-border px-4 py-6 text-sm text-textSecondary">
            📭 Нет врачей. Создайте врача через регистрацию/профиль.
          </div>
        )}
        {doctors && doctors.length > 0 && (
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-border/70 text-sm">
              <thead className="bg-surface text-left uppercase text-xs font-semibold text-textSecondary">
                <tr>
                  <th className="px-3 py-2 w-10"></th>
                  <th className="px-3 py-2">ID</th>
                  <th className="px-3 py-2">Telegram ID</th>
                  <th className="px-3 py-2">Баланс</th>
                  <th className="px-3 py-2">Никнейм</th>
                  <th className="px-3 py-2">Активный пациент</th>
                  <th className="px-3 py-2">Специализация</th>
                  <th className="px-3 py-2">Верификация</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border/60">
                {doctors.map((d: DoctorPublicDto) => (
                  <tr
                    key={d.id}
                    className="cursor-pointer hover:bg-accentMuted"
                    onClick={() => openDoctor(d.id)}
                    aria-label={`Открыть врача ${d.id}`}
                  >
                    <td className="px-3 py-2">
                      <input
                        type="checkbox"
                        checked={selectedIds.includes(d.id)}
                        onClick={(e) => e.stopPropagation()}
                        onChange={(e) => toggleSelect(d.id, e.currentTarget.checked)}
                      />
                    </td>
                    <td className="px-3 py-2 font-medium text-textPrimary">{d.id}</td>
                    <td className="px-3 py-2 text-textSecondary">{d.telegramUserId ?? '—'}</td>
                    <td className="px-3 py-2 text-textSecondary">{d.tokenBalance ?? 0}</td>
                    <td className="px-3 py-2 text-textSecondary">{d.nickname ?? '—'}</td>
                    <td className="px-3 py-2 text-textSecondary">{d.lastSelectedPatientId ?? '—'}</td>
                    <td className="px-3 py-2 text-textSecondary">{d.specializations?.[0]?.title ?? '—'}</td>
                    <td className="px-3 py-2">
                      <Badge
                        label={d.verified ? 'Верифицирован' : 'Не верифицирован'}
                        tone={d.verified ? 'success' : 'warning'}
                      />
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </Card>

      <Modal
        open={editorOpen}
        onClose={() => {
          setEditorOpen(false)
          setSelectedIds([])
        }}
        title={form?.id === 'new' ? 'Новый врач' : `Редактирование: ${form?.id ?? ''}`}
        footer={
          <>
            <Button variant="secondary" onClick={() => setEditorOpen(false)}>
              Отмена
            </Button>
            <Button
              variant="primary"
              disabled={!form || form.id === 'new' || updateMutation.isPending}
              onClick={() => {
                if (!form || form.id === 'new') return
                const code = form.specializationCode.trim()
                const lastSelectedPatientId = form.lastSelectedPatientId.trim()
                const payload: UpdateDoctorRequest = {
                  specializationCodes: code ? [code] : [],
                  telegramUserId: form.telegramUserId ?? null,
                  nickname: form.nickname ?? null,
                  lastSelectedPatientId,
                  verified: form.verified,
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
            <Input
              label="Код специализации"
              value={form.specializationCode}
              onChange={(e) => handleChange('specializationCode', e.currentTarget.value)}
            />
            <Input
              label="Telegram ID"
              type="number"
              value={form.telegramUserId ?? ''}
              onChange={(e) => handleChange('telegramUserId', e.currentTarget.value === '' ? null : Number(e.currentTarget.value))}
            />
            <Input label="Баланс токенов" value={form.tokenBalance} readOnly />
            <Input label="Никнейм" value={form.nickname ?? ''} onChange={(e) => handleChange('nickname', e.currentTarget.value)} />
            <Input
              label="Активный пациент (ID)"
              placeholder="GUID пациента или пусто для очистки"
              value={form.lastSelectedPatientId}
              onChange={(e) => handleChange('lastSelectedPatientId', e.currentTarget.value)}
            />
            <div className="flex items-center gap-3">
              <Toggle label="Верифицирован" checked={form.verified} onChange={(e) => handleChange('verified', e.currentTarget.checked)} />
            </div>
            <Input
              label="Пополнить на (токенов)"
              type="number"
              min={1}
              value={topUpTokens}
              onChange={(e) => setTopUpTokens(e.currentTarget.value)}
            />
            <div className="flex items-end">
              <Button
                variant="secondary"
                disabled={
                  !form.telegramUserId ||
                  topUpMutation.isPending ||
                  !Number.isFinite(Number(topUpTokens)) ||
                  Number(topUpTokens) <= 0
                }
                onClick={() => {
                  if (!form.telegramUserId) return
                  topUpMutation.mutate({
                    telegramUserId: form.telegramUserId,
                    tokens: Number(topUpTokens),
                  })
                }}
              >
                Пополнить баланс
              </Button>
            </div>
            {saveMessage && <div className="text-sm text-green-600">{saveMessage}</div>}
            {saveError && <div className="text-sm text-red-600">{saveError}</div>}
          </div>
        ) : (
          <div className="text-sm text-textSecondary">Нет данных для отображения.</div>
        )}
      </Modal>
    </>
  )
}
