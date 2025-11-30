import { useEffect, useMemo, useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Card } from '../components/Card'
import { Input } from '../components/Input'
import { Textarea } from '../components/Textarea'
import { Button } from '../components/Button'
import { Badge } from '../components/Badge'
import { ApiClient } from '../api/client'
import type { CreateDialogRequest, DialogDto, MessageDto } from '../api/types'

export default function DialogsPage() {
  const queryClient = useQueryClient()
  const { data: patients } = useQuery({ queryKey: ['patients'], queryFn: ApiClient.getPatients })
  const [selectedDialogId, setSelectedDialogId] = useState<string | null>(null)
  const [newDialog, setNewDialog] = useState<CreateDialogRequest>({ topic: '', patientId: null })
  const [newMessage, setNewMessage] = useState<string>('')
  const [messageRole, setMessageRole] = useState<'doctor' | 'assistant' | 'system'>('doctor')
  const [statusMessage, setStatusMessage] = useState<string | null>(null)

  const { data: dialogs } = useQuery({
    queryKey: ['dialogs', newDialog.patientId],
    queryFn: () => ApiClient.getDialogs(newDialog.patientId),
  })

  const messagesQuery = useQuery({
    queryKey: ['messages', selectedDialogId],
    queryFn: () => ApiClient.getMessages(selectedDialogId || ''),
    enabled: !!selectedDialogId,
  })

  useEffect(() => {
    if (!selectedDialogId && dialogs && dialogs.length > 0) {
      setSelectedDialogId(dialogs[0].id)
    }
  }, [dialogs, selectedDialogId])

  const createDialog = useMutation({
    mutationFn: ApiClient.createDialog,
    onSuccess: (dialog) => {
      queryClient.invalidateQueries({ queryKey: ['dialogs'] })
      setSelectedDialogId(dialog.id)
      setStatusMessage('Диалог создан')
      setTimeout(() => setStatusMessage(null), 2000)
    },
  })

  const sendMessage = useMutation({
    mutationFn: () =>
      selectedDialogId
        ? ApiClient.addMessage(selectedDialogId, { role: messageRole, content: newMessage })
        : Promise.resolve(null),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['messages', selectedDialogId] })
      queryClient.invalidateQueries({ queryKey: ['dialogs'] })
      queryClient.invalidateQueries({ queryKey: ['patients'] })
      setNewMessage('')
    },
  })

  const closeDialog = useMutation({
    mutationFn: (id: string) => ApiClient.closeDialog(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['dialogs'] })
      setStatusMessage('Диалог закрыт')
      setTimeout(() => setStatusMessage(null), 2000)
    },
  })

  const selectedDialog = useMemo(
    () => dialogs?.find((d) => d.id === selectedDialogId) || null,
    [dialogs, selectedDialogId]
  )

  return (
    <div className="grid gap-4">
      <Card title="Новый диалог">
        <div className="grid gap-3 md:grid-cols-2">
          <Input
            label="Пациент (опционально, id)"
            value={newDialog.patientId ?? ''}
            onChange={(e) => setNewDialog((prev) => ({ ...prev, patientId: e.currentTarget.value || null }))}
            placeholder="Введите id пациента или оставьте пустым"
            list="patients-list"
          />
          <datalist id="patients-list">
            {patients?.map((p) => (
              <option key={p.id} value={p.id}>
                {p.fullName}
              </option>
            ))}
          </datalist>
          <Input
            label="Тема"
            value={newDialog.topic ?? ''}
            onChange={(e) => setNewDialog((prev) => ({ ...prev, topic: e.currentTarget.value }))}
          />
        </div>
        <div className="mt-3 flex items-center gap-3">
          <Button onClick={() => createDialog.mutate(newDialog)}>Создать</Button>
          <Button variant="secondary" onClick={() => queryClient.invalidateQueries({ queryKey: ['dialogs'] })}>
            Обновить
          </Button>
          {statusMessage && <span className="text-sm text-green-600">{statusMessage}</span>}
        </div>
      </Card>

      <Card title="Диалоги">
        {!dialogs || dialogs.length === 0 ? (
          <div className="rounded-lg border border-dashed border-border px-4 py-6 text-sm text-textSecondary">
            📭 Нет диалогов — создайте новый.
          </div>
        ) : (
          <div className="flex flex-wrap gap-2">
            {dialogs.map((d: DialogDto) => (
              <Button
                key={d.id}
                variant={d.id === selectedDialogId ? 'primary' : 'secondary'}
                onClick={() => setSelectedDialogId(d.id)}
              >
                {d.topic || 'Без темы'} · {d.status}
              </Button>
            ))}
          </div>
        )}
      </Card>

      {selectedDialog && (
        <Card
          title="Сообщения"
          actions={
            <Button variant="secondary" onClick={() => closeDialog.mutate(selectedDialog.id)}>
              Закрыть диалог
            </Button>
          }
        >
          <div className="mb-3 flex flex-wrap gap-2 text-sm text-textSecondary">
            <Badge label={selectedDialog.status} tone={selectedDialog.status === 'Open' ? 'success' : 'warning'} />
            {selectedDialog.patientId && <Badge label={`Пациент: ${selectedDialog.patientId}`} tone="neutral" />}
          </div>
          <div className="space-y-2 rounded-xl border border-border/70 bg-white p-3">
            {messagesQuery.data?.map((m: MessageDto) => (
              <div
                key={m.id}
                className="rounded-lg border border-border/60 bg-surface px-3 py-2 text-sm shadow-sm"
                aria-label={`Сообщение от ${m.role}`}
              >
                <div className="flex items-center justify-between text-xs text-textSecondary">
                  <span>{m.role}</span>
                  <span>{new Date(m.createdAt).toLocaleString()}</span>
                </div>
                <div className="mt-1 text-textPrimary">{m.content}</div>
              </div>
            ))}
            {(!messagesQuery.data || messagesQuery.data.length === 0) && (
              <div className="text-sm text-textSecondary">Пока нет сообщений.</div>
            )}
          </div>

          <div className="mt-4 grid gap-3 md:grid-cols-[200px_1fr]">
            <Input
              label="Роль (doctor/assistant/system)"
              value={messageRole}
              onChange={(e) => setMessageRole(e.currentTarget.value as any)}
            />
            <Textarea
              label="Сообщение"
              value={newMessage}
              onChange={(e) => setNewMessage(e.currentTarget.value)}
              placeholder="Введите текст"
            />
          </div>
          <div className="mt-3">
            <Button onClick={() => sendMessage.mutate()} disabled={!selectedDialogId || sendMessage.isPending}>
              Отправить
            </Button>
          </div>
        </Card>
      )}
    </div>
  )
}
