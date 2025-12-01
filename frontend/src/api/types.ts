export type RegistrationStatus = 'NotStarted' | 'InProgress' | 'Completed'

export type PatientSex = 0 | 1 // Male=0, Female=1
export type PatientStatus = 0 | 1 // Inactive=0, Active=1

export interface RegistrationDto {
  status: RegistrationStatus
  specialization: string | null
  humanInLoopConfirmed: boolean
  startedAt?: string | null
  telegramUsername: string
}

export interface UpsertRegistrationRequest {
  displayName: string
  specializationCode: string
  specializationTitle: string
  telegramUsername: string
  degrees?: string | null
  experienceYears?: number | null
  languages?: string | null
  bio?: string | null
  focusAreas?: string | null
  acceptingNewPatients: boolean
  location?: string | null
  contactPolicy?: string | null
  avatarUrl?: string | null
  humanInLoopConfirmed: boolean
}

export interface ProfileDto {
  doctorId: string
  displayName: string
  specializationCode: string
  specializationTitle: string
  telegramUsername: string
  degrees?: string | null
  experienceYears?: number | null
  languages?: string | null
  bio?: string | null
  focusAreas?: string | null
  acceptingNewPatients: boolean
  location?: string | null
  contactPolicy?: string | null
  avatarUrl?: string | null
  registrationStatus: RegistrationStatus
  lastSelectedPatientId?: string | null
  verified: boolean
  rating?: number | null
}

export interface UpdateProfileRequest {
  displayName?: string | null
  specializationCode?: string | null
  specializationTitle?: string | null
  degrees?: string | null
  experienceYears?: number | null
  languages?: string | null
  bio?: string | null
  focusAreas?: string | null
  acceptingNewPatients?: boolean | null
  location?: string | null
  contactPolicy?: string | null
  avatarUrl?: string | null
  lastSelectedPatientId?: string | null
}

export interface PatientDto {
  id: string
  fullName: string
  birthDate?: string | null
  sex?: PatientSex | null
  phone?: string | null
  email?: string | null
  allergies?: string | null
  chronicConditions?: string | null
  tags?: string | null
  status: PatientStatus
  notes?: string | null
  createdAt: string
  updatedAt: string
  lastDialogId?: string | null
  lastSummary?: string | null
  lastInteractionAt?: string | null
}

export interface CreatePatientRequest {
  fullName: string
  birthDate?: string | null
  sex?: PatientSex | null
  phone?: string | null
  email?: string | null
  allergies?: string | null
  chronicConditions?: string | null
  tags?: string | null
  notes?: string | null
  status?: PatientStatus | null
}

export interface DialogDto {
  id: string
  patientId?: string | null
  topic?: string | null
  status: 'Open' | 'Closed'
  createdAt: string
  closedAt?: string | null
}

export interface CreateDialogRequest {
  patientId?: string | null
  topic?: string | null
}

export interface MessageDto {
  id: string
  dialogId: string
  role: 'Doctor' | 'Assistant' | 'System'
  content: string
  createdAt: string
}

export interface CreateMessageRequest {
  role: 'doctor' | 'assistant' | 'system'
  content: string
}

export interface SpecializationDto {
  code: string
  title: string
}

export interface DoctorPublicDto {
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

export interface UpdateDoctorRequest {
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

export interface PatientDirectoryDto {
  id: string
  doctorId: string
  fullName: string
  birthDate?: string | null
  sex?: PatientSex | null
  phone?: string | null
  email?: string | null
  allergies?: string | null
  chronicConditions?: string | null
  tags?: string | null
  status: PatientStatus
  notes?: string | null
  createdAt: string
  updatedAt: string
  lastDialogId?: string | null
  lastSummary?: string | null
  lastInteractionAt?: string | null
}

export interface UpdatePatientDirectoryRequest {
  fullName: string
  birthDate?: string | null
  sex?: PatientSex | null
  phone?: string | null
  email?: string | null
  allergies?: string | null
  chronicConditions?: string | null
  tags?: string | null
  status: PatientStatus
  notes?: string | null
}

export interface CreatePatientRequest {
  fullName: string
  birthDate?: string | null
  sex?: PatientSex | null
  phone?: string | null
  email?: string | null
  allergies?: string | null
  chronicConditions?: string | null
  tags?: string | null
  notes?: string | null
  status?: PatientStatus | null
}
