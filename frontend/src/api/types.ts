export type RegistrationStatus = 'NotStarted' | 'InProgress' | 'Completed'

export type PatientSex = 0 | 1 // Male=0, Female=1
export type PatientStatus = 0 | 1 // Inactive=0, Active=1

export interface RegistrationDto {
  status: RegistrationStatus
  specializations: SpecializationDto[]
  nickname?: string | null
  startedAt?: string | null
  telegramUserId?: number | null
}

export interface UpsertRegistrationRequest {
  specializationCodes?: string[] | null
  telegramUserId: number
  nickname?: string | null
}

export interface ProfileDto {
  doctorId: string
  specializations: SpecializationDto[]
  telegramUserId?: number | null
  tokenBalance: number
  nickname?: string | null
  lastSelectedPatientId?: string | null
  lastSelectedPatientNickname?: string | null
}

export interface UpdateProfileRequest {
  nickname?: string | null
}

export interface SetActivePatientRequest {
  patientId: string
}

export interface PatientDto {
  id: string
  sex?: PatientSex | null
  ageYears?: number | null
  nickname?: string | null
  allergies?: string | null
  chronicConditions?: string | null
  tags?: string | null
  status: PatientStatus
  notes?: string | null
  lastInteractionAt?: string | null
}

export interface CreatePatientRequest {
  sex?: PatientSex | null
  ageYears?: number | null
  nickname?: string | null
  allergies?: string | null
  chronicConditions?: string | null
  tags?: string | null
  notes?: string | null
  status?: PatientStatus | null
}

export interface UpdatePatientRequest {
  sex?: PatientSex | null
  ageYears?: number | null
  nickname?: string | null
  allergies?: string | null
  chronicConditions?: string | null
  tags?: string | null
  notes?: string | null
  status?: PatientStatus | null
}

export interface SpecializationDto {
  code: string
  title: string
}

export interface DoctorPublicDto {
  id: string
  specializations: SpecializationDto[]
  telegramUserId?: number | null
  tokenBalance: number
  nickname?: string | null
  verified: boolean
}

export interface UpdateDoctorRequest {
  specializationCodes?: string[] | null
  telegramUserId?: number | null
  nickname?: string | null
  verified: boolean
}

export interface TopUpDoctorTokensRequest {
  telegramUserId: number
  tokens: number
}

export interface DoctorTokenBalanceDto {
  doctorId: string
  telegramUserId: number
  tokenBalance: number
}

export interface BillingTokenLedgerDto {
  id: string
  doctorId: string
  telegramUserId: number
  delta: number
  balanceAfter: number
  reason: string
  conversationId?: string | null
  chatTurnId?: string | null
  requestId?: string | null
  createdAt: string
}

export interface BotConversationHistoryDto {
  conversationId: string
  telegramUserId: number
  turnsCount: number
  lastUserText?: string | null
  createdAt: string
  updatedAt: string
}

export interface BotChatTurnHistoryDto {
  turnId: string
  conversationId: string
  requestId: string
  userText: string
  assistantText: string
  createdAt: string
}

export interface StaticContentDto {
  id: string
  code: string
  title?: string | null
  value: string
  updatedAt: string
}

export interface StaticContentItemDto {
  code: string
  value: string
}

export interface CreateStaticContentRequest {
  code: string
  title?: string | null
  value: string
}

export interface UpdateStaticContentRequest {
  code: string
  title?: string | null
  value: string
}

export interface SystemSettingsDto {
  llmGatewayUrl?: string | null
  updatedAt: string
}

export interface UpdateSystemSettingsRequest {
  llmGatewayUrl: string
}

export interface PatientDirectoryDto {
  id: string
  doctorId: string
  doctorNickname?: string | null
  sex?: PatientSex | null
  ageYears?: number | null
  nickname?: string | null
  allergies?: string | null
  chronicConditions?: string | null
  tags?: string | null
  status: PatientStatus
  notes?: string | null
  createdAt: string
  updatedAt: string
  lastInteractionAt?: string | null
}

export interface UpdatePatientDirectoryRequest {
  doctorId?: string | null
  sex?: PatientSex | null
  ageYears?: number | null
  nickname?: string | null
  allergies?: string | null
  chronicConditions?: string | null
  tags?: string | null
  status: PatientStatus
  notes?: string | null
}

export interface LlmGenerateRequest {
  prompt: string
  model?: string | null
  systemPrompt?: string | null
  temperature?: number | null
  maxTokens?: number | null
}

export interface LlmGenerateResponse {
  provider: string
  model: string
  content: string
  finishReason?: string | null
  promptTokens?: number | null
  completionTokens?: number | null
  requestId?: string | null
}

export type AuthGrantType = 'api_key' | 'telegram_init_data'

export interface IssueTokenRequest {
  type: AuthGrantType
  payload: {
    initData?: string
  }
}

export interface IssueTokenResponse {
  accessToken: string
  expiresIn: number
  tokenType: string
  actorType: string
}
