import { useState } from 'react'
import { useMutation } from '@tanstack/react-query'
import { AxiosError } from 'axios'
import { Card } from '../components/Card'
import { Input } from '../components/Input'
import { Button } from '../components/Button'
import { ApiClient } from '../api/client'

type ErrorResponse = {
  error?: string
  title?: string
  detail?: string
}

interface Props {
  onAuthenticated: (token: string) => void
}

export default function AdminLoginPage({ onAuthenticated }: Props) {
  const [apiKey, setApiKey] = useState('')
  const [errorText, setErrorText] = useState<string | null>(null)

  const loginMutation = useMutation({
    mutationFn: () => ApiClient.issueAdminToken(apiKey.trim()),
    onSuccess: (response) => {
      setErrorText(null)
      onAuthenticated(response.accessToken)
    },
    onError: (error) => {
      const axiosError = error as AxiosError<ErrorResponse>
      const message = axiosError.response?.data?.error
        || axiosError.response?.data?.detail
        || axiosError.response?.data?.title
        || axiosError.message
        || 'Не удалось авторизоваться.'

      setErrorText(message)
    },
  })

  const handleLogin = () => {
    if (!apiKey.trim()) {
      setErrorText('Введите API ключ администратора.')
      return
    }

    loginMutation.mutate()
  }

  return (
    <div className="mx-auto mt-16 max-w-md px-4">
      <Card title="Вход в админку">
        <div className="grid gap-3">
          <Input
            label="Admin API key"
            type="password"
            value={apiKey}
            onChange={(e) => setApiKey(e.currentTarget.value)}
            onKeyDown={(e) => {
              if (e.key === 'Enter') {
                handleLogin()
              }
            }}
          />

          {errorText && (
            <div className="rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
              {errorText}
            </div>
          )}

          <Button variant="primary" onClick={handleLogin} disabled={loginMutation.isPending} fullWidth>
            {loginMutation.isPending ? 'Проверка...' : 'Войти'}
          </Button>
        </div>
      </Card>
    </div>
  )
}
