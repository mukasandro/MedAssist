import { useState } from 'react'
import { useMutation } from '@tanstack/react-query'
import { AxiosError } from 'axios'
import { Card } from '../components/Card'
import { Button } from '../components/Button'
import { Input } from '../components/Input'
import { Textarea } from '../components/Textarea'
import { Select } from '../components/Select'
import { ApiClient } from '../api/client'
import type { LlmGenerateResponse } from '../api/types'

type FormState = {
  prompt: string
  model: string
  systemPrompt: string
  temperature: string
  maxTokens: string
}

const defaultForm: FormState = {
  prompt: '',
  model: 'deepseek-chat',
  systemPrompt: '',
  temperature: '0.3',
  maxTokens: '1024',
}

export default function LlmGatewayPage() {
  const [form, setForm] = useState<FormState>(defaultForm)
  const [result, setResult] = useState<LlmGenerateResponse | null>(null)
  const [errorText, setErrorText] = useState<string | null>(null)

  const generateMutation = useMutation({
    mutationFn: ApiClient.generateLlm,
    onSuccess: (data) => {
      setResult(data)
      setErrorText(null)
    },
    onError: (error) => {
      const axiosError = error as AxiosError<{ detail?: string; title?: string }>
      const detail = axiosError.response?.data?.detail
      const title = axiosError.response?.data?.title
      setErrorText(detail || title || axiosError.message || 'Ошибка запроса к LLM Gateway')
      setResult(null)
    },
  })

  const onGenerate = () => {
    const prompt = form.prompt.trim()
    if (!prompt) {
      setErrorText('Поле prompt обязательно.')
      return
    }

    generateMutation.mutate({
      prompt,
      model: form.model.trim() || null,
      systemPrompt: form.systemPrompt.trim() || null,
      temperature: form.temperature.trim() ? Number(form.temperature) : null,
      maxTokens: form.maxTokens.trim() ? Number(form.maxTokens) : null,
    })
  }

  return (
    <Card
      title="LLM Gateway"
      actions={
        <div className="flex gap-2">
          <Button variant="secondary" onClick={() => setForm(defaultForm)}>
            Сброс
          </Button>
          <Button variant="primary" disabled={generateMutation.isPending} onClick={onGenerate}>
            {generateMutation.isPending ? 'Отправка...' : 'Отправить'}
          </Button>
        </div>
      }
    >
      <div className="grid gap-3 md:grid-cols-2">
        <div className="md:col-span-2">
          <Textarea
            label="Prompt"
            value={form.prompt}
            onChange={(e) => {
              const value = e.currentTarget.value
              setForm((prev) => ({ ...prev, prompt: value }))
            }}
          />
        </div>

        <div className="md:col-span-2">
          <Select
            label="Модель"
            value={form.model}
            options={[
              { value: 'deepseek-chat', label: 'deepseek-chat' },
              { value: 'deepseek-reasoner', label: 'deepseek-reasoner' },
            ]}
            onChange={(e) => {
              const value = e.currentTarget.value
              setForm((prev) => ({ ...prev, model: value }))
            }}
          />
        </div>

        <div className="md:col-span-2">
          <Textarea
            label="System Prompt (опционально)"
            value={form.systemPrompt}
            onChange={(e) => {
              const value = e.currentTarget.value
              setForm((prev) => ({ ...prev, systemPrompt: value }))
            }}
          />
        </div>

        <Input
          label="Temperature"
          type="number"
          step="0.1"
          min={0}
          max={2}
          value={form.temperature}
          onChange={(e) => {
            const value = e.currentTarget.value
            setForm((prev) => ({ ...prev, temperature: value }))
          }}
        />

        <Input
          label="Max Tokens"
          type="number"
          min={1}
          max={8192}
          value={form.maxTokens}
          onChange={(e) => {
            const value = e.currentTarget.value
            setForm((prev) => ({ ...prev, maxTokens: value }))
          }}
        />
      </div>

      {errorText && (
        <div className="mt-4 rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">{errorText}</div>
      )}

      {result && (
        <div className="mt-4 grid gap-3">
          <div className="rounded-lg border border-border bg-surface px-4 py-3 text-sm text-textSecondary">
            provider: {result.provider} | model: {result.model} | finish: {result.finishReason ?? 'n/a'} | promptTokens:{' '}
            {result.promptTokens ?? 'n/a'} | completionTokens: {result.completionTokens ?? 'n/a'}
          </div>
          <Textarea label="Ответ" value={result.content} readOnly />
        </div>
      )}
    </Card>
  )
}
