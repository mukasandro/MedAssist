import { useEffect, useMemo, useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { Card } from '../components/Card'
import { Button } from '../components/Button'
import { Input } from '../components/Input'
import { Select } from '../components/Select'
import { ApiClient } from '../api/client'
import type { BotConversationHistoryDto, BotChatTurnHistoryDto, DoctorPublicDto } from '../api/types'

const normalizeTake = (value: string, fallback: number, max: number) => {
  const parsed = Number(value)
  if (!Number.isFinite(parsed)) return fallback
  return Math.min(Math.max(Math.trunc(parsed), 1), max)
}

const formatDateTime = (value: string) => new Date(value).toLocaleString()

export default function ChatHistoryAdminPage() {
  const [telegramUserIdFilterInput, setTelegramUserIdFilterInput] = useState('')
  const [takeConversationsInput, setTakeConversationsInput] = useState('100')
  const [takeTurnsInput, setTakeTurnsInput] = useState('200')
  const [selectedConversationId, setSelectedConversationId] = useState<string | null>(null)

  const telegramUserIdFilter = useMemo(() => {
    const trimmed = telegramUserIdFilterInput.trim()
    if (!trimmed) return null
    const parsed = Number(trimmed)
    if (!Number.isFinite(parsed)) return null
    return Math.trunc(parsed)
  }, [telegramUserIdFilterInput])

  const takeConversations = useMemo(() => normalizeTake(takeConversationsInput, 100, 200), [takeConversationsInput])
  const takeTurns = useMemo(() => normalizeTake(takeTurnsInput, 200, 500), [takeTurnsInput])

  const { data: doctors } = useQuery({
    queryKey: ['admin-doctors-chat-history'],
    queryFn: ApiClient.getDoctors,
  })

  const {
    data: conversations,
    isLoading: isConversationsLoading,
    isFetching: isConversationsFetching,
    error: conversationsError,
    refetch: refetchConversations,
  } = useQuery({
    queryKey: ['admin-chat-conversations', telegramUserIdFilter, takeConversations],
    queryFn: () => ApiClient.getBotChatConversations(telegramUserIdFilter, takeConversations),
  })

  useEffect(() => {
    if (!selectedConversationId || !conversations) return
    if (!conversations.some((x) => x.conversationId === selectedConversationId)) {
      setSelectedConversationId(null)
    }
  }, [conversations, selectedConversationId])

  const {
    data: turns,
    isLoading: isTurnsLoading,
    isFetching: isTurnsFetching,
    error: turnsError,
  } = useQuery({
    queryKey: ['admin-chat-turns', selectedConversationId, takeTurns],
    queryFn: () => ApiClient.getBotChatTurns(selectedConversationId!, takeTurns),
    enabled: Boolean(selectedConversationId),
  })

  const doctorNicknameByTelegramUserId = useMemo(() => {
    const map = new Map<number, string>()
    ;(doctors ?? []).forEach((doctor: DoctorPublicDto) => {
      if (!doctor.telegramUserId) return
      map.set(doctor.telegramUserId, doctor.nickname?.trim() || 'Без никнейма')
    })
    return map
  }, [doctors])

  const doctorFilterOptions = useMemo(
    () => [
      { value: '', label: 'Все врачи' },
      ...(doctors ?? [])
        .filter((doctor: DoctorPublicDto) => Boolean(doctor.telegramUserId))
        .map((doctor: DoctorPublicDto) => ({
          value: String(doctor.telegramUserId!),
          label: `${doctor.nickname?.trim() || 'Без никнейма'} (tg: ${doctor.telegramUserId})`,
        })),
    ],
    [doctors]
  )

  return (
    <div className="grid gap-4">
      <Card
        title="История общения врачей"
        actions={
          <Button variant="secondary" onClick={() => refetchConversations()} disabled={isConversationsFetching}>
            {isConversationsFetching ? 'Обновление...' : 'Обновить'}
          </Button>
        }
      >
        <div className="grid gap-3 md:grid-cols-2 lg:grid-cols-4">
          <Select
            label="Врач"
            value={telegramUserIdFilterInput}
            options={doctorFilterOptions}
            onChange={(e) => setTelegramUserIdFilterInput(e.currentTarget.value)}
          />
          <Input
            label="Фильтр по Telegram user id"
            type="number"
            value={telegramUserIdFilterInput}
            placeholder="Все врачи"
            onChange={(e) => setTelegramUserIdFilterInput(e.currentTarget.value)}
          />
          <Input
            label="Лимит диалогов (1..200)"
            type="number"
            value={takeConversationsInput}
            onChange={(e) => setTakeConversationsInput(e.currentTarget.value)}
          />
          <Input
            label="Лимит сообщений (1..500)"
            type="number"
            value={takeTurnsInput}
            onChange={(e) => setTakeTurnsInput(e.currentTarget.value)}
          />
        </div>
      </Card>

      <div className="grid gap-4 lg:grid-cols-2">
        <Card title="Диалоги">
          {isConversationsLoading && <div className="text-sm text-textSecondary">⏳ Загрузка...</div>}
          {conversationsError && (
            <div className="rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
              Не удалось загрузить диалоги.
            </div>
          )}
          {!isConversationsLoading && !conversationsError && (!conversations || conversations.length === 0) && (
            <div className="rounded-lg border border-dashed border-border px-4 py-6 text-sm text-textSecondary">
              Диалоги не найдены.
            </div>
          )}
          {conversations && conversations.length > 0 && (
            <div className="overflow-x-auto">
              <table className="min-w-full divide-y divide-border/70 text-sm">
                <thead className="bg-surface text-left uppercase text-xs font-semibold text-textSecondary">
                  <tr>
                    <th className="px-3 py-2">Conversation</th>
                    <th className="px-3 py-2">Врач</th>
                    <th className="px-3 py-2">Сообщений</th>
                    <th className="px-3 py-2">Обновлен</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-border/60">
                  {conversations.map((conversation: BotConversationHistoryDto) => (
                    <tr
                      key={conversation.conversationId}
                      className={`cursor-pointer hover:bg-accentMuted ${
                        selectedConversationId === conversation.conversationId ? 'bg-accentMuted' : ''
                      }`}
                      onClick={() => setSelectedConversationId(conversation.conversationId)}
                    >
                      <td className="px-3 py-2 align-top">
                        <div className="font-medium text-textPrimary">{conversation.conversationId}</div>
                        <div className="mt-1 text-xs text-textSecondary line-clamp-2">
                          {conversation.lastUserText?.trim() || '—'}
                        </div>
                      </td>
                      <td className="px-3 py-2 align-top text-textSecondary">
                        <div>tg: {conversation.telegramUserId}</div>
                        <div className="text-xs">
                          {doctorNicknameByTelegramUserId.get(conversation.telegramUserId) || '—'}
                        </div>
                      </td>
                      <td className="px-3 py-2 align-top text-textSecondary">{conversation.turnsCount}</td>
                      <td className="px-3 py-2 align-top text-textSecondary">{formatDateTime(conversation.updatedAt)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </Card>

        <Card title="Сообщения диалога">
          {!selectedConversationId && (
            <div className="rounded-lg border border-dashed border-border px-4 py-6 text-sm text-textSecondary">
              Выберите диалог слева.
            </div>
          )}

          {selectedConversationId && (
            <div className="grid gap-3">
              <div className="rounded-lg border border-border bg-surface px-4 py-3 text-xs text-textSecondary">
                ConversationId: {selectedConversationId}
              </div>
              {isTurnsLoading && <div className="text-sm text-textSecondary">⏳ Загрузка...</div>}
              {isTurnsFetching && !isTurnsLoading && (
                <div className="text-xs text-textSecondary">Обновляю список сообщений...</div>
              )}
              {turnsError && (
                <div className="rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
                  Не удалось загрузить сообщения диалога.
                </div>
              )}
              {!isTurnsLoading && !turnsError && (!turns || turns.length === 0) && (
                <div className="rounded-lg border border-dashed border-border px-4 py-6 text-sm text-textSecondary">
                  В выбранном диалоге пока нет сообщений.
                </div>
              )}
              {turns && turns.length > 0 && (
                <div className="grid gap-3">
                  {[...turns]
                    .sort((a, b) => new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime())
                    .map((turn: BotChatTurnHistoryDto) => (
                      <div key={turn.turnId} className="rounded-lg border border-border bg-white px-4 py-3">
                        <div className="mb-2 text-xs text-textSecondary">
                          {formatDateTime(turn.createdAt)} | requestId: {turn.requestId}
                        </div>
                        <div className="grid gap-2 text-sm">
                          <div>
                            <div className="mb-1 text-xs uppercase text-textSecondary">Вопрос</div>
                            <div className="whitespace-pre-wrap text-textPrimary">{turn.userText}</div>
                          </div>
                          <div>
                            <div className="mb-1 text-xs uppercase text-textSecondary">Ответ</div>
                            <div className="whitespace-pre-wrap text-textPrimary">{turn.assistantText}</div>
                          </div>
                        </div>
                      </div>
                    ))}
                </div>
              )}
            </div>
          )}
        </Card>
      </div>
    </div>
  )
}
