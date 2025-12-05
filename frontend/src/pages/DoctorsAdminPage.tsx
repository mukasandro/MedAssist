import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { Card } from '../components/Card'
import { Badge } from '../components/Badge'
import { Button } from '../components/Button'
import { ApiClient } from '../api/client'
import type { DoctorPublicDto, UpdateDoctorRequest } from '../api/types'
import { Input } from '../components/Input'
import { Textarea } from '../components/Textarea'
import { Toggle } from '../components/Toggle'
import { Modal } from '../components/Modal'

type DoctorForm = {
  id: string
  displayName: string
  specializationCode: string
  specializationTitle: string
  degrees?: string | null
  experienceYears?: number | null
  languages?: string | null
  bio?: string | null
  focusAreas?: string | null
  acceptingNewPatients: boolean
  location?: string | null
  contactPolicy?: string | null
  avatarUrl?: string | null
  verified: boolean
  rating?: number | null
}

export default function DoctorsAdminPage() {
  const queryClient = useQueryClient()
  const { data: doctors, isLoading, error } = useQuery({ queryKey: ['admin-doctors'], queryFn: ApiClient.getDoctors })
  const [selectedIds, setSelectedIds] = useState<string[]>([])
  const [editorOpen, setEditorOpen] = useState(false)
  const [form, setForm] = useState<DoctorForm | null>(null)
  const [saveMessage, setSaveMessage] = useState<string | null>(null)
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
      queryClient.invalidateQueries({ queryKey: ['admin-doctors'] })
      setSaveMessage('Сохранено')
      setTimeout(() => setSaveMessage(null), 1500)
    },
  })

  const openNew = () => {
    setSelectedIds([])
    setForm({
      id: 'new',
      displayName: '',
      specializationCode: '',
      specializationTitle: '',
      degrees: '',
      experienceYears: 0,
      languages: '',
      bio: '',
      focusAreas: '',
      acceptingNewPatients: true,
      location: '',
      contactPolicy: '',
      avatarUrl: '',
      verified: false,
      rating: null,
    })
    setEditorOpen(true)
  }

  const openDoctor = (id: string) => {
    const doc = doctors?.find((d) => d.id === id)
    if (!doc) return
    setSelectedIds((prev) => (prev.includes(id) ? prev : [...prev, id]))
    setForm({
      id: doc.id,
      displayName: doc.displayName,
      specializationCode: doc.specializationCodes?.[0] ?? '',
      specializationTitle: doc.specializationTitles?.[0] ?? '',
      degrees: doc.degrees,
      experienceYears: doc.experienceYears,
      languages: doc.languages,
      bio: doc.bio,
      focusAreas: doc.focusAreas,
      acceptingNewPatients: doc.acceptingNewPatients,
      location: doc.location,
      contactPolicy: doc.contactPolicy,
      avatarUrl: doc.avatarUrl,
      verified: doc.verified,
      rating: doc.rating,
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
                  <th className="px-3 py-2">Имя</th>
                  <th className="px-3 py-2">Специализация</th>
                  <th className="px-3 py-2">Стаж</th>
                  <th className="px-3 py-2">Языки</th>
                  <th className="px-3 py-2">Статус</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border/60">
                {doctors.map((d: DoctorPublicDto) => (
                  <tr
                    key={d.id}
                    className="cursor-pointer hover:bg-accentMuted"
                    onClick={() => openDoctor(d.id)}
                    aria-label={`Открыть врача ${d.displayName}`}
                  >
                    <td className="px-3 py-2">
                      <input
                        type="checkbox"
                        checked={selectedIds.includes(d.id)}
                        onClick={(e) => e.stopPropagation()}
                        onChange={(e) => toggleSelect(d.id, e.currentTarget.checked)}
                      />
                    </td>
                    <td className="px-3 py-2 font-medium text-textPrimary">{d.displayName}</td>
                    <td className="px-3 py-2 text-textSecondary">{d.specializationTitles?.[0] ?? '—'}</td>
                    <td className="px-3 py-2 text-textSecondary">{d.experienceYears ?? '—'}</td>
                    <td className="px-3 py-2 text-textSecondary">{d.languages ?? '—'}</td>
                    <td className="px-3 py-2">
                      <Badge
                        label={d.acceptingNewPatients ? 'Принимает новых' : 'Закрыт'}
                        tone={d.acceptingNewPatients ? 'success' : 'warning'}
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
        title={form?.id === 'new' ? 'Новый врач' : `Редактирование: ${form?.displayName ?? ''}`}
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
                const title = form.specializationTitle.trim()
                const payload: UpdateDoctorRequest = {
                  displayName: form.displayName,
                  specializationCodes: code && title ? [code] : [],
                  specializationTitles: code && title ? [title] : [],
                  degrees: form.degrees,
                  experienceYears: form.experienceYears,
                  languages: form.languages,
                  bio: form.bio,
                  focusAreas: form.focusAreas,
                  acceptingNewPatients: form.acceptingNewPatients,
                  location: form.location,
                  contactPolicy: form.contactPolicy,
                  avatarUrl: form.avatarUrl,
                  verified: form.verified,
                  rating: form.rating,
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
            <Input label="Имя" value={form.displayName} onChange={(e) => handleChange('displayName', e.currentTarget.value)} />
            <Input
              label="Специализация"
              value={form.specializationTitle}
              onChange={(e) => handleChange('specializationTitle', e.currentTarget.value)}
            />
            <Input
              label="Код специализации"
              value={form.specializationCode}
              onChange={(e) => handleChange('specializationCode', e.currentTarget.value)}
            />
            <Input
              label="Стаж"
              type="number"
              value={form.experienceYears ?? ''}
              onChange={(e) => handleChange('experienceYears', Number(e.currentTarget.value))}
            />
            <Input label="Языки" value={form.languages ?? ''} onChange={(e) => handleChange('languages', e.currentTarget.value)} />
            <Textarea label="Био" value={form.bio ?? ''} onChange={(e) => handleChange('bio', e.currentTarget.value)} />
            <Input label="Фокусы" value={form.focusAreas ?? ''} onChange={(e) => handleChange('focusAreas', e.currentTarget.value)} />
            <Input label="Локация" value={form.location ?? ''} onChange={(e) => handleChange('location', e.currentTarget.value)} />
            <Input
              label="Политика контакта"
              value={form.contactPolicy ?? ''}
              onChange={(e) => handleChange('contactPolicy', e.currentTarget.value)}
            />
            <Input
              label="Аватар URL"
              value={form.avatarUrl ?? ''}
              onChange={(e) => handleChange('avatarUrl', e.currentTarget.value)}
            />
            <div className="flex items-center gap-3">
              <Toggle
                label="Принимает новых"
                checked={form.acceptingNewPatients}
                onChange={(e) => handleChange('acceptingNewPatients', e.currentTarget.checked)}
              />
              <Toggle label="Верифицирован" checked={form.verified} onChange={(e) => handleChange('verified', e.currentTarget.checked)} />
            </div>
            <Input
              label="Рейтинг"
              type="number"
              value={form.rating ?? ''}
              onChange={(e) => handleChange('rating', Number(e.currentTarget.value))}
            />
            {saveMessage && <div className="text-sm text-green-600">{saveMessage}</div>}
          </div>
        ) : (
          <div className="text-sm text-textSecondary">Нет данных для отображения.</div>
        )}
      </Modal>
    </>
  )
}
