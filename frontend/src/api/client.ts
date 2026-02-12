import axios from 'axios'
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
  StaticContentDto,
  StaticContentItemDto,
  CreateStaticContentRequest,
  UpdateStaticContentRequest,
  PatientDirectoryDto,
  UpdatePatientDirectoryRequest,
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

export const ApiClient = {
  // Registration
  upsertRegistration: (payload: UpsertRegistrationRequest) =>
    api.post<RegistrationDto>('/v1/registration', payload).then((r) => r.data),

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

  // Directory
  getDoctors: () => api.get<DoctorPublicDto[]>('/v1/doctors').then((r) => r.data),
  updateDoctor: (id: string, payload: UpdateDoctorRequest) =>
    api.put<DoctorPublicDto>(`/v1/doctors/${id}`, payload).then((r) => r.data),
  deleteDoctor: (id: string) => api.delete(`/v1/doctors/${id}`),

  // Patient directory
  getPatientsDirectory: () => api.get<PatientDirectoryDto[]>('/v1/patient-directory').then((r) => r.data),
  updatePatientDirectory: (id: string, payload: UpdatePatientDirectoryRequest) =>
    api.put<PatientDirectoryDto>(`/v1/patient-directory/${id}`, payload).then((r) => r.data),
  deletePatientDirectory: (id: string) => api.delete(`/v1/patient-directory/${id}`),
  createPatientDirectoryTest: () => api.post<PatientDirectoryDto>('/v1/patient-directory/test', {}).then((r) => r.data),
  createDoctorTest: () => api.post<DoctorPublicDto>('/v1/doctors/test', {}).then((r) => r.data),
}

export default api
