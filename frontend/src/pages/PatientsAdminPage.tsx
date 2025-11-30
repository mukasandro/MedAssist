import { useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Card } from '../components/Card'
import { Badge } from '../components/Badge'
import { Button } from '../components/Button'
import { ApiClient } from '../api/client'
import type { AdminPatientDto, UpdatePatientAdminRequest, PatientSex, PatientStatus, CreatePatientRequest } from '../api/types'
import { Input } from '../components/Input'
import { Textarea } from '../components/Textarea'
import { Modal } from '../components/Modal'

const statusTone = (status: PatientStatus) => (status === 1 ? 'success' : 'warning')

export default function PatientsAdminPage() {
  const queryClient = useQueryClient()
  const { data: patients, isLoading, error } = useQuery({ queryKey: ['admin-patients'], queryFn: ApiClient.getPatientsAdmin })
  const [selectedIds, setSelectedIds] = useState<string[]>([])
  const [editorOpen, setEditorOpen] = useState(false)
  const [form, setForm] = useState<AdminPatientDto | null>(null)
  const [saveMessage, setSaveMessage] = useState<string | null>(null)
  const [createOpen, setCreateOpen] = useState(false)
  const [createForm, setCreateForm] = useState<CreatePatientRequest>({
    fullName: '',
    birthDate: null,
    sex: null,
    phone: '',
    email: '',
    allergies: '',
    chronicConditions: '',
    tags: '',
    notes: '',
    status: 1,
  })

  const updateMutation = useMutation({
    mutationFn: (payload: { id: string; body: UpdatePatientAdminRequest }) =>
      ApiClient.updatePatientAdmin(payload.id, payload.body),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin-patients'] })
      setSaveMessage('Сохранено')
      setTimeout(() => setSaveMessage(null), 1500)
    },
  })

  const createMutation = useMutation({
    mutationFn: (payload: CreatePatientRequest) => ApiClient.createPatient(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin-patients'] })
      setCreateOpen(false)
      setCreateForm({
        fullName: '',
        birthDate: null,
        sex: null,
        phone: '',
        email: '',
        allergies: '',
        chronicConditions: '',
        tags: '',
        notes: '',
        status: 1,
      })
    },
  })

  const createTestMutation = useMutation({
    mutationFn: ApiClient.createPatientAdminTest,
    onSuccess: () => {
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

  const handleChange = (key: keyof AdminPatientDto, value: any) => {
    setForm((prev) => (prev ? { ...prev, [key]: value } : prev))
  }

  const handleCreateChange = (key: keyof CreatePatientRequest, value: any) => {
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
            <Button variant="danger" disabled={selectedIds.length === 0}>
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
                  <th className="px-3 py-2">Имя</th>
                  <th className="px-3 py-2">Пол</th>
                  <th className="px-3 py-2">Телефон</th>
                  <th className="px-3 py-2">Email</th>
                  <th className="px-3 py-2">Статус</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border/60">
                {patients.map((p: AdminPatientDto) => (
                  <tr
                    key={p.id}
                    className="cursor-pointer hover:bg-accentMuted"
                    onClick={() => openPatient(p.id)}
                    aria-label={`Открыть пациента ${p.fullName}`}
                  >
                    <td className="px-3 py-2">
                      <input
                        type="checkbox"
                        checked={selectedIds.includes(p.id)}
                        onClick={(e) => e.stopPropagation()}
                        onChange={(e) => toggleSelect(p.id, e.currentTarget.checked)}
                      />
                    </td>
                    <td className="px-3 py-2 font-medium text-textPrimary">{p.fullName}</td>
                    <td className="px-3 py-2 text-textSecondary">{p.sex ?? '—'}</td>
                    <td className="px-3 py-2 text-textSecondary">{p.phone ?? '—'}</td>
                    <td className="px-3 py-2 text-textSecondary">{p.email ?? '—'}</td>
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
              disabled={createMutation.isPending || !createForm.fullName}
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
            label="Имя"
            value={createForm.fullName}
            onChange={(e) => handleCreateChange('fullName', e.currentTarget.value)}
          />
          <Input
            label="Дата рождения"
            type="date"
            value={createForm.birthDate ? createForm.birthDate.split('T')[0] : ''}
            onChange={(e) =>
              handleCreateChange('birthDate', e.currentTarget.value ? `${e.currentTarget.value}T00:00:00Z` : null)
            }
          />
          <Input
            label="Пол (0=М,1=Ж)"
            type="number"
            value={createForm.sex ?? ''}
            onChange={(e) => handleCreateChange('sex', Number(e.currentTarget.value) as PatientSex)}
          />
          <Input
            label="Телефон"
            value={createForm.phone ?? ''}
            onChange={(e) => handleCreateChange('phone', e.currentTarget.value)}
          />
          <Input
            label="Email"
            value={createForm.email ?? ''}
            onChange={(e) => handleCreateChange('email', e.currentTarget.value)}
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
        title={form ? `Редактирование: ${form.fullName}` : 'Пациент'}
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
                const payload: UpdatePatientAdminRequest = {
                  fullName: form.fullName,
                  birthDate: form.birthDate ?? null,
                  sex: form.sex as PatientSex | null,
                  phone: form.phone ?? null,
                  email: form.email ?? null,
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
            <Input label="Имя" value={form.fullName} onChange={(e) => handleChange('fullName', e.currentTarget.value)} />
            <Input label="ДокторId" value={form.doctorId} readOnly />
            <Input
              label="Дата рождения"
              type="date"
              value={form.birthDate ? form.birthDate.split('T')[0] : ''}
              onChange={(e) => handleChange('birthDate', e.currentTarget.value ? `${e.currentTarget.value}T00:00:00Z` : null)}
            />
            <Input
              label="Пол (0=М,1=Ж)"
              type="number"
              value={form.sex ?? ''}
              onChange={(e) => handleChange('sex', Number(e.currentTarget.value) as PatientSex)}
            />
            <Input label="Телефон" value={form.phone ?? ''} onChange={(e) => handleChange('phone', e.currentTarget.value)} />
            <Input label="Email" value={form.email ?? ''} onChange={(e) => handleChange('email', e.currentTarget.value)} />
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
