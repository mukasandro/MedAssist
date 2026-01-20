export type RegistrationStatus = 'NotStarted' | 'InProgress' | 'Completed'

export type PatientSex = 0 | 1 // Male=0, Female=1
export type PatientStatus = 0 | 1 // Inactive=0, Active=1

export interface RegistrationDto {
  status: RegistrationStatus
  specializationCodes: string[]
  nickname?: string | null
  confirmed: boolean
  startedAt?: string | null
  telegramUserId?: number | null
}

export interface UpsertRegistrationRequest {
  specializationCodes: string[]
  telegramUserId: number
  nickname?: string | null
  confirmed: boolean
}

export interface ProfileDto {
  doctorId: string
  specializationCodes: string[]
  specializationTitles: string[]
  telegramUserId?: number | null
  nickname?: string | null
  lastSelectedPatientId?: string | null
  verified: boolean
}

export interface UpdateProfileRequest {
  specializationCodes?: string[] | null
  specializationTitles?: string[] | null
  nickname?: string | null
  lastSelectedPatientId?: string | null
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

export interface SpecializationDto {
  code: string
  title: string
}

export interface DoctorPublicDto {
  id: string
  specializationCodes: string[]
  specializationTitles: string[]
  telegramUserId?: number | null
  nickname?: string | null
  verified: boolean
}

export interface UpdateDoctorRequest {
  specializationCodes?: string[] | null
  specializationTitles?: string[] | null
  nickname?: string | null
  verified: boolean
}

export interface StaticContentDto {
  id: string
  code: string
  title?: string | null
  value: string
  updatedAt: string
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

export interface PatientDirectoryDto {
  id: string
  doctorId: string
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
  sex?: PatientSex | null
  ageYears?: number | null
  nickname?: string | null
  allergies?: string | null
  chronicConditions?: string | null
  tags?: string | null
  status: PatientStatus
  notes?: string | null
}
