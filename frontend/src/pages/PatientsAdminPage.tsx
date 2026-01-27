import { useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Card } from '../components/Card'
import { Badge } from '../components/Badge'
import { Button } from '../components/Button'
import { ApiClient } from '../api/client'
import type {
  PatientDirectoryDto,
  UpdatePatientDirectoryRequest,
  PatientSex,
  PatientStatus,
  CreatePatientRequest,
} from '../api/types'
import { Input } from '../components/Input'
import { Textarea } from '../components/Textarea'
import { Modal } from '../components/Modal'

const statusTone = (status: PatientStatus) => (status === 1 ? 'success' : 'warning')

export default function PatientsAdminPage() {
  const queryClient = useQueryClient()
  const { data: patients, isLoading, error } = useQuery({
    queryKey: ['admin-patients'],
    queryFn: ApiClient.getPatientsDirectory,
  })
  const [selectedIds, setSelectedIds] = useState<string[]>([])
  const [editorOpen, setEditorOpen] = useState(false)
  const [form, setForm] = useState<PatientDirectoryDto | null>(null)
  const [saveMessage, setSaveMessage] = useState<string | null>(null)
  const [createOpen, setCreateOpen] = useState(false)
  const [createForm, setCreateForm] = useState<CreatePatientRequest & { telegramUserId: string }>({
    telegramUserId: '',
    sex: null,
    ageYears: null,
    nickname: '',
    allergies: '',
    chronicConditions: '',
    tags: '',
    notes: '',
    status: 1,
  })

  const updateMutation = useMutation({
    mutationFn: (payload: { id: string; body: UpdatePatientDirectoryRequest }) =>
      ApiClient.updatePatientDirectory(payload.id, payload.body),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin-patients'] })
      setSaveMessage('Сохранено')
      setTimeout(() => setSaveMessage(null), 1500)
    },
  })

  const createMutation = useMutation({
    mutationFn: (payload: CreatePatientRequest & { telegramUserId: string }) => {
      const { telegramUserId, ...body } = payload
      return ApiClient.createPatient(telegramUserId, body)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin-patients'] })
      setCreateOpen(false)
      setCreateForm({
        telegramUserId: '',
        sex: null,
        ageYears: null,
        nickname: '',
        allergies: '',
        chronicConditions: '',
        tags: '',
        notes: '',
        status: 1,
      })
    },
  })

  const createTestMutation = useMutation({
    mutationFn: ApiClient.createPatientDirectoryTest,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin-patients'] })
    },
  })

  const deleteMutation = useMutation({
    mutationFn: async (ids: string[]) => {
      await Promise.all(ids.map((id) => ApiClient.deletePatientDirectory(id)))
    },
    onSuccess: () => {
      setSelectedIds([])
      setEditorOpen(false)
      setForm(null)
      queryClient.invalidateQueries({ queryKey: ['admin-patients'] })
    },
  })

  const openPatient = (id: string) => {
    const p = patients?.find((x) => x.id === id)
    if (!p) return
    setSelectedIds((prev) => (prev.includes(id) ? prev : [...prev, id]))
    setForm(p)
    setEditorOpen(true)
  }

  const toggleSelect = (id: string, checked: boolean) => {
    setSelectedIds((prev) => (checked ? [...prev, id] : prev.filter((x) => x !== id)))
  }

  const handleChange = (key: keyof PatientDirectoryDto, value: any) => {
    setForm((prev) => (prev ? { ...prev, [key]: value } : prev))
  }

  const handleCreateChange = (key: keyof (CreatePatientRequest & { telegramUserId: string }), value: any) => {
    setCreateForm((prev) => ({ ...prev, [key]: value }))
  }

  return (
    <>
      <Card
        title="Пациенты"
        actions={
          <div className="flex gap-2">
            <Button variant="primary" onClick={() => setCreateOpen(true)}>
              Новый пациент
            </Button>
            <Button variant="secondary" onClick={() => createTestMutation.mutate()}>
              Тестовый пациент
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
            Не удалось загрузить пациентов. Проверьте API.
          </div>
        )}
        {!isLoading && !error && (!patients || patients.length === 0) && (
          <div className="rounded-lg border border-dashed border-border px-4 py-6 text-sm text-textSecondary">
            📭 Нет пациентов.
          </div>
        )}
        {patients && patients.length > 0 && (
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-border/70 text-sm">
              <thead className="bg-surface text-left uppercase text-xs font-semibold text-textSecondary">
                <tr>
                  <th className="px-3 py-2 w-10"></th>
                  <th className="px-3 py-2">ID</th>
                  <th className="px-3 py-2">Никнейм</th>
                  <th className="px-3 py-2">Доктор</th>
                  <th className="px-3 py-2">Пол</th>
                  <th className="px-3 py-2">Возраст</th>
                  <th className="px-3 py-2">Статус</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border/60">
                {patients.map((p: PatientDirectoryDto) => (
                  <tr
                    key={p.id}
                    className="cursor-pointer hover:bg-accentMuted"
                    onClick={() => openPatient(p.id)}
                    aria-label={`Открыть пациента ${p.id}`}
                  >
                    <td className="px-3 py-2">
                      <input
                        type="checkbox"
                        checked={selectedIds.includes(p.id)}
                        onClick={(e) => e.stopPropagation()}
                        onChange={(e) => toggleSelect(p.id, e.currentTarget.checked)}
                      />
                    </td>
                    <td className="px-3 py-2 font-medium text-textPrimary">{p.id}</td>
                    <td className="px-3 py-2 text-textSecondary">{p.nickname ?? '—'}</td>
                    <td className="px-3 py-2 text-textSecondary">{p.doctorNickname ?? '—'}</td>
                    <td className="px-3 py-2 text-textSecondary">{p.sex ?? '—'}</td>
                    <td className="px-3 py-2 text-textSecondary">{p.ageYears ?? '—'}</td>
                    <td className="px-3 py-2">
                      <Badge label={p.status === 1 ? 'Активен' : 'Неактивен'} tone={statusTone(p.status)} />
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
        onClose={() => {
          setCreateOpen(false)
        }}
        title="Новый пациент"
        footer={
          <>
            <Button variant="secondary" onClick={() => setCreateOpen(false)}>
              Отмена
            </Button>
            <Button
              variant="primary"
              disabled={createMutation.isPending || !createForm.telegramUserId}
              onClick={() => {
                createMutation.mutate(createForm)
              }}
            >
              Создать
            </Button>
          </>
        }
      >
        <div className="grid gap-3 md:grid-cols-2">
          <Input
            label="Telegram user id врача"
            value={createForm.telegramUserId}
            type="number"
            onChange={(e) => handleCreateChange('telegramUserId', e.currentTarget.value)}
          />
          <Input
            label="Пол (0=М,1=Ж)"
            type="number"
            value={createForm.sex ?? ''}
            onChange={(e) => handleCreateChange('sex', Number(e.currentTarget.value) as PatientSex)}
          />
          <Input
            label="Возраст (лет)"
            type="number"
            value={createForm.ageYears ?? ''}
            onChange={(e) => handleCreateChange('ageYears', Number(e.currentTarget.value))}
          />
          <Input
            label="Никнейм"
            value={createForm.nickname ?? ''}
            onChange={(e) => handleCreateChange('nickname', e.currentTarget.value)}
          />
          <Textarea
            label="Аллергии"
            value={createForm.allergies ?? ''}
            onChange={(e) => handleCreateChange('allergies', e.currentTarget.value)}
          />
          <Textarea
            label="Хронические состояния"
            value={createForm.chronicConditions ?? ''}
            onChange={(e) => handleCreateChange('chronicConditions', e.currentTarget.value)}
          />
          <Input
            label="Теги"
            value={createForm.tags ?? ''}
            onChange={(e) => handleCreateChange('tags', e.currentTarget.value)}
          />
          <Textarea
            label="Заметки"
            value={createForm.notes ?? ''}
            onChange={(e) => handleCreateChange('notes', e.currentTarget.value)}
          />
          <Input
            label="Статус (0=inactive,1=active)"
            type="number"
            value={createForm.status ?? 1}
            onChange={(e) => handleCreateChange('status', Number(e.currentTarget.value) as PatientStatus)}
          />
        </div>
      </Modal>

      <Modal
        open={editorOpen}
        onClose={() => {
          setEditorOpen(false)
          setSelectedIds([])
          setForm(null)
        }}
        title={form ? `Редактирование: ${form.id}` : 'Пациент'}
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
                const payload: UpdatePatientDirectoryRequest = {
                  sex: form.sex as PatientSex | null,
                  ageYears: form.ageYears ?? null,
                  nickname: form.nickname ?? null,
                  allergies: form.allergies ?? null,
                  chronicConditions: form.chronicConditions ?? null,
                  tags: form.tags ?? null,
                  status: form.status as PatientStatus,
                  notes: form.notes ?? null,
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
            <Input label="ДокторId" value={form.doctorId} readOnly />
            <Input label="Никнейм доктора" value={form.doctorNickname ?? ''} readOnly />
            <Input
              label="Пол (0=М,1=Ж)"
              type="number"
              value={form.sex ?? ''}
              onChange={(e) => handleChange('sex', Number(e.currentTarget.value) as PatientSex)}
            />
            <Input
              label="Возраст (лет)"
              type="number"
              value={form.ageYears ?? ''}
              onChange={(e) => handleChange('ageYears', Number(e.currentTarget.value))}
            />
            <Input label="Никнейм" value={form.nickname ?? ''} onChange={(e) => handleChange('nickname', e.currentTarget.value)} />
            <Textarea
              label="Аллергии"
              value={form.allergies ?? ''}
              onChange={(e) => handleChange('allergies', e.currentTarget.value)}
            />
            <Textarea
              label="Хронические состояния"
              value={form.chronicConditions ?? ''}
              onChange={(e) => handleChange('chronicConditions', e.currentTarget.value)}
            />
            <Input label="Теги" value={form.tags ?? ''} onChange={(e) => handleChange('tags', e.currentTarget.value)} />
            <Textarea label="Заметки" value={form.notes ?? ''} onChange={(e) => handleChange('notes', e.currentTarget.value)} />
            <Input
              label="Статус (0=inactive,1=active)"
              type="number"
              value={form.status}
              onChange={(e) => handleChange('status', Number(e.currentTarget.value) as PatientStatus)}
            />
            {saveMessage && <div className="text-sm text-green-600">{saveMessage}</div>}
          </div>
        ) : (
          <div className="text-sm text-textSecondary">Нет данных</div>
        )}
      </Modal>
    </>
  )
}
