import axios, { AxiosHeaders } from 'axios'
import {
  RegistrationDto,
  UpsertRegistrationRequest,
  ProfileDto,
  UpdateProfileRequest,
  SetActivePatientRequest,
  PatientDto,
  CreatePatientRequest,
  UpdatePatientRequest,
  SpecializationDto,
  DoctorPublicDto,
  UpdateDoctorRequest,
  TopUpDoctorTokensRequest,
  DoctorTokenBalanceDto,
  BillingTokenLedgerDto,
  BotConversationHistoryDto,
  BotChatTurnHistoryDto,
  StaticContentDto,
  StaticContentItemDto,
  CreateStaticContentRequest,
  UpdateStaticContentRequest,
  SystemSettingsDto,
  UpdateSystemSettingsRequest,
  PatientDirectoryDto,
  UpdatePatientDirectoryRequest,
  LlmGenerateRequest,
  LlmGenerateResponse,
  IssueTokenRequest,
  IssueTokenResponse,
} from './types'

const resolveBaseUrl = () => {
  const envUrl = import.meta.env.VITE_API_URL
  if (envUrl) return envUrl
  if (typeof window !== 'undefined') {
    const { origin } = window.location
    const host = origin.replace(/:\d+$/, '')
    return `${host}:8080`
  }
  return 'http://localhost:8080'
}

const api = axios.create({
  baseURL: resolveBaseUrl(),
})

const ADMIN_ACCESS_TOKEN_KEY = 'medassist.admin.access_token'

const readInitialAccessToken = () => {
  if (typeof window === 'undefined') return null
  return sessionStorage.getItem(ADMIN_ACCESS_TOKEN_KEY)
}

let adminAccessToken: string | null = readInitialAccessToken()
let unauthorizedListener: (() => void) | null = null

const setAdminAccessToken = (token: string | null) => {
  adminAccessToken = token
  if (typeof window === 'undefined') return
  if (token) {
    sessionStorage.setItem(ADMIN_ACCESS_TOKEN_KEY, token)
  } else {
    sessionStorage.removeItem(ADMIN_ACCESS_TOKEN_KEY)
  }
}

export const AdminSession = {
  getAccessToken: () => adminAccessToken,
  setAccessToken: (token: string) => setAdminAccessToken(token),
  clearAccessToken: () => setAdminAccessToken(null),
  subscribeUnauthorized: (listener: () => void) => {
    unauthorizedListener = listener
    return () => {
      if (unauthorizedListener === listener) {
        unauthorizedListener = null
      }
    }
  },
}

api.interceptors.request.use((config) => {
  if (!adminAccessToken) {
    return config
  }

  const headers = AxiosHeaders.from(config.headers)
  headers.set('Authorization', `Bearer ${adminAccessToken}`)
  config.headers = headers
  return config
})

api.interceptors.response.use(
  (response) => response,
  (error) => {
    const status = error?.response?.status
    const url: string = error?.config?.url ?? ''
    const isAuthTokenCall = url.includes('/v1/auth/token')

    if (status === 401 && !isAuthTokenCall) {
      setAdminAccessToken(null)
      unauthorizedListener?.()
    }

    return Promise.reject(error)
  }
)

const resolveLlmGatewayBaseUrl = () => {
  const envUrl = import.meta.env.VITE_LLM_GATEWAY_URL
  if (envUrl) return envUrl
  if (typeof window !== 'undefined') {
    const { origin } = window.location
    const host = origin.replace(/:\d+$/, '')
    return `${host}:8090`
  }
  return 'http://localhost:8090'
}

const llmApi = axios.create({
  baseURL: resolveLlmGatewayBaseUrl(),
})

export const ApiClient = {
  // Registration
  upsertRegistration: (payload: UpsertRegistrationRequest) =>
    api.post<RegistrationDto>('/v1/registration', payload).then((r) => r.data),

  // Auth
  issueToken: (payload: IssueTokenRequest, apiKey?: string | null) =>
    api
      .post<IssueTokenResponse>('/v1/auth/token', payload, {
        headers: apiKey ? { Authorization: `ApiKey ${apiKey}` } : undefined,
      })
      .then((r) => r.data),
  issueAdminToken: (apiKey: string) =>
    api
      .post<IssueTokenResponse>(
        '/v1/auth/token',
        { type: 'api_key', payload: {} } satisfies IssueTokenRequest,
        { headers: { Authorization: `ApiKey ${apiKey}` } }
      )
      .then((r) => r.data),

  // Profile
  getProfile: (telegramUserId: string) =>
    api
      .get<ProfileDto>('/v1/me', { headers: { 'X-Telegram-User-Id': telegramUserId } })
      .then((r) => r.data),
  updateProfile: (telegramUserId: string, payload: UpdateProfileRequest) =>
    api
      .patch<ProfileDto>('/v1/me', payload, { headers: { 'X-Telegram-User-Id': telegramUserId } })
      .then((r) => r.data),

  // Patients
  getPatients: (telegramUserId: string) =>
    api
      .get<PatientDto[]>('/v1/patients', { headers: { 'X-Telegram-User-Id': telegramUserId } })
      .then((r) => r.data),
  createPatient: (telegramUserId: string, payload: CreatePatientRequest) =>
    api
      .post<PatientDto>('/v1/patients', payload, { headers: { 'X-Telegram-User-Id': telegramUserId } })
      .then((r) => r.data),
  updatePatient: (telegramUserId: string, id: string, payload: UpdatePatientRequest) =>
    api
      .patch<PatientDto>(`/v1/patients/${id}`, payload, { headers: { 'X-Telegram-User-Id': telegramUserId } })
      .then((r) => r.data),
  deletePatient: (telegramUserId: string, id: string) =>
    api.delete(`/v1/patients/${id}`, { headers: { 'X-Telegram-User-Id': telegramUserId } }),
  setActivePatient: (telegramUserId: string, id: string) =>
    api
      .put(
        '/v1/me/active-patient',
        { patientId: id } satisfies SetActivePatientRequest,
        { headers: { 'X-Telegram-User-Id': telegramUserId } }
      )
      .then(() => undefined),
  clearActivePatient: (telegramUserId: string) =>
    api
      .delete('/v1/me/active-patient', { headers: { 'X-Telegram-User-Id': telegramUserId } })
      .then(() => undefined),

  // Reference
  getSpecializations: () => api.get<SpecializationDto[]>('/v1/reference/specializations').then((r) => r.data),

  // Static content
  getStaticContent: () => api.get<StaticContentDto[]>('/v1/static-content').then((r) => r.data),
  createStaticContent: (payload: CreateStaticContentRequest) =>
    api.post<StaticContentDto>('/v1/static-content', payload).then((r) => r.data),
  updateStaticContent: (id: string, payload: UpdateStaticContentRequest) =>
    api.put<StaticContentDto>(`/v1/static-content/${id}`, payload).then((r) => r.data),
  deleteStaticContent: (id: string) => api.delete(`/v1/static-content/${id}`),
  getStaticContentValue: (code: string) =>
    api.get<StaticContentItemDto>(`/v1/static-content/${code}`).then((r) => r.data),

  // System settings
  getSystemSettings: () => api.get<SystemSettingsDto>('/v1/settings').then((r) => r.data),
  updateSystemSettings: (payload: UpdateSystemSettingsRequest) =>
    api.put<SystemSettingsDto>('/v1/settings', payload).then((r) => r.data),

  // Directory
  getDoctors: () => api.get<DoctorPublicDto[]>('/v1/doctors').then((r) => r.data),
  updateDoctor: (id: string, payload: UpdateDoctorRequest) =>
    api.put<DoctorPublicDto>(`/v1/doctors/${id}`, payload).then((r) => r.data),
  deleteDoctor: (id: string) => api.delete(`/v1/doctors/${id}`),
  topUpDoctorTokens: (payload: TopUpDoctorTokensRequest) =>
    api.post<DoctorTokenBalanceDto>('/v1/billing/topup', payload).then((r) => r.data),
  topUpMyTokens: (telegramUserId: string, tokens: number) =>
    api
      .post<DoctorTokenBalanceDto>(
        '/v1/me/billing/topup',
        { tokens },
        { headers: { 'X-Telegram-User-Id': telegramUserId } }
      )
      .then((r) => r.data),
  getMyBillingHistory: (telegramUserId: string, take = 100) =>
    api
      .get<BillingTokenLedgerDto[]>('/v1/me/billing/history', {
        headers: { 'X-Telegram-User-Id': telegramUserId },
        params: { take },
      })
      .then((r) => r.data),
  getBillingHistory: (telegramUserId?: number | null, take = 100) =>
    api
      .get<BillingTokenLedgerDto[]>('/v1/billing/history', {
        params: {
          telegramUserId: telegramUserId ?? undefined,
          take,
        },
      })
      .then((r) => r.data),
  getMyChatConversations: (telegramUserId: string, take = 100) =>
    api
      .get<BotConversationHistoryDto[]>('/v1/me/chat/conversations', {
        headers: { 'X-Telegram-User-Id': telegramUserId },
        params: { take },
      })
      .then((r) => r.data),
  getMyChatTurns: (telegramUserId: string, conversationId: string, take = 200) =>
    api
      .get<BotChatTurnHistoryDto[]>(`/v1/me/chat/conversations/${conversationId}/turns`, {
        headers: { 'X-Telegram-User-Id': telegramUserId },
        params: { take },
      })
      .then((r) => r.data),
  getBotChatConversations: (telegramUserId?: number | null, take = 100) =>
    api
      .get<BotConversationHistoryDto[]>('/v1/admin/chat-history/conversations', {
        params: {
          telegramUserId: telegramUserId ?? undefined,
          take,
        },
      })
      .then((r) => r.data),
  getBotChatTurns: (conversationId: string, take = 200) =>
    api
      .get<BotChatTurnHistoryDto[]>(`/v1/admin/chat-history/conversations/${conversationId}/turns`, {
        params: { take },
      })
      .then((r) => r.data),

  // Patient directory
  getPatientsDirectory: () => api.get<PatientDirectoryDto[]>('/v1/patient-directory').then((r) => r.data),
  updatePatientDirectory: (id: string, payload: UpdatePatientDirectoryRequest) =>
    api.put<PatientDirectoryDto>(`/v1/patient-directory/${id}`, payload).then((r) => r.data),
  deletePatientDirectory: (id: string) => api.delete(`/v1/patient-directory/${id}`),
  createPatientDirectoryTest: () => api.post<PatientDirectoryDto>('/v1/patient-directory/test', {}).then((r) => r.data),
  createDoctorTest: () => api.post<DoctorPublicDto>('/v1/doctors/test', {}).then((r) => r.data),

  // LLM gateway
  generateLlm: (payload: LlmGenerateRequest) =>
    llmApi.post<LlmGenerateResponse>('/v1/generate', payload).then((r) => r.data),
}

export default api
