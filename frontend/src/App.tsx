import { useEffect, useMemo, useState } from 'react'
import { Layout } from './components/Layout'
import { Breadcrumbs } from './components/Breadcrumbs'
import ReferencePage from './pages/ReferencePage'
import DashboardPage from './pages/DashboardPage'
import DoctorsAdminPage from './pages/DoctorsAdminPage'
import PatientsAdminPage from './pages/PatientsAdminPage'
import StaticContentAdminPage from './pages/StaticContentAdminPage'
import LlmGatewayPage from './pages/LlmGatewayPage'
import SettingsPage from './pages/SettingsPage'
import AdminLoginPage from './pages/AdminLoginPage'
import ChatHistoryAdminPage from './pages/ChatHistoryAdminPage'
import { AdminSession } from './api/client'

const navItems = [
  { key: 'dashboard', label: 'Обзор', icon: '📊' },
  { key: 'reference', label: 'Справочники', icon: '📚' },
  { key: 'static-content', label: 'Статика', icon: '🧩' },
  { key: 'settings', label: 'Настройки', icon: '⚙️' },
  { key: 'llm-gateway', label: 'LLM Gateway', icon: '🧠' },
  { key: 'chat-history', label: 'История чатов', icon: '💬' },
  { key: 'doctors', label: 'Врачи', icon: '🛡️' },
  { key: 'patients-admin', label: 'Пациенты (админ)', icon: '🛡️' },
]

function App() {
  const [accessToken, setAccessToken] = useState<string | null>(() => AdminSession.getAccessToken())
  const [active, setActive] = useState<string>('dashboard')

  useEffect(() => {
    const unsubscribe = AdminSession.subscribeUnauthorized(() => {
      setAccessToken(null)
    })

    return unsubscribe
  }, [])

  const pageTitle = useMemo(() => {
    const item = navItems.find((n) => n.key === active)
    return item?.label ?? ''
  }, [active])

  const content = useMemo(() => {
    switch (active) {
      case 'dashboard':
        return <DashboardPage />
      case 'reference':
        return <ReferencePage />
      case 'static-content':
        return <StaticContentAdminPage />
      case 'settings':
        return <SettingsPage />
      case 'llm-gateway':
        return <LlmGatewayPage />
      case 'chat-history':
        return <ChatHistoryAdminPage />
      case 'doctors':
        return <DoctorsAdminPage />
      case 'patients-admin':
        return <PatientsAdminPage />
      default:
        return null
    }
  }, [active])

  if (!accessToken) {
    return (
      <AdminLoginPage
        onAuthenticated={(token) => {
          AdminSession.setAccessToken(token)
          setAccessToken(token)
        }}
      />
    )
  }

  return (
    <Layout
      navItems={navItems}
      active={active}
      onSelect={setActive}
      onLogout={() => {
        AdminSession.clearAccessToken()
        setAccessToken(null)
      }}
    >
      <div className="mb-4">
        <Breadcrumbs path={[{ label: 'Админка' }, { label: pageTitle }]} />
      </div>
      {content}
    </Layout>
  )
}

export default App
