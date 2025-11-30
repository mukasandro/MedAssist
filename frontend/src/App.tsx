import { useMemo, useState } from 'react'
import { Layout } from './components/Layout'
import { Breadcrumbs } from './components/Breadcrumbs'
import DialogsPage from './pages/DialogsPage'
import ReferencePage from './pages/ReferencePage'
import DashboardPage from './pages/DashboardPage'
import DoctorsAdminPage from './pages/DoctorsAdminPage'
import PatientsAdminPage from './pages/PatientsAdminPage'

const navItems = [
  { key: 'dashboard', label: 'Обзор', icon: '📊' },
  { key: 'dialogs', label: 'Диалоги', icon: '💬' },
  { key: 'reference', label: 'Справочники', icon: '📚' },
  { key: 'doctors', label: 'Врачи', icon: '🛡️' },
  { key: 'patients-admin', label: 'Пациенты (админ)', icon: '🛡️' },
]

function App() {
  const [active, setActive] = useState<string>('dashboard')

  const pageTitle = useMemo(() => {
    const item = navItems.find((n) => n.key === active)
    return item?.label ?? ''
  }, [active])

  const content = useMemo(() => {
    switch (active) {
      case 'dashboard':
        return <DashboardPage />
      case 'dialogs':
        return <DialogsPage />
      case 'reference':
        return <ReferencePage />
      case 'doctors':
        return <DoctorsAdminPage />
      case 'patients-admin':
        return <PatientsAdminPage />
      default:
        return null
    }
  }, [active])

  return (
    <Layout navItems={navItems} active={active} onSelect={setActive}>
      <div className="mb-4">
        <Breadcrumbs path={[{ label: 'Админка' }, { label: pageTitle }]} />
      </div>
      {content}
    </Layout>
  )
}

export default App
