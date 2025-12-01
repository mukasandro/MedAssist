import axios from 'axios'
import {
  RegistrationDto,
  UpsertRegistrationRequest,
  ProfileDto,
  UpdateProfileRequest,
  PatientDto,
  CreatePatientRequest,
  DialogDto,
  CreateDialogRequest,
  MessageDto,
  CreateMessageRequest,
  SpecializationDto,
  DoctorPublicDto,
  UpdateDoctorRequest,
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
  getRegistration: () => api.get<RegistrationDto>('/v1/registration').then((r) => r.data),
  upsertRegistration: (payload: UpsertRegistrationRequest) =>
    api.post<RegistrationDto>('/v1/registration', payload).then((r) => r.data),

  // Profile
  getProfile: () => api.get<ProfileDto>('/v1/me').then((r) => r.data),
  updateProfile: (payload: UpdateProfileRequest) =>
    api.patch<ProfileDto>('/v1/me', payload).then((r) => r.data),

  // Patients
  getPatients: () => api.get<PatientDto[]>('/v1/patients').then((r) => r.data),
  createPatient: (payload: CreatePatientRequest) =>
    api.post<PatientDto>('/v1/patients', payload).then((r) => r.data),
  deletePatient: (id: string) => api.delete(`/v1/patients/${id}`),
  selectPatient: (id: string) => api.post('/v1/patients/' + id + '/select'),

  // Dialogs
  getDialogs: (patientId?: string | null) =>
    api
      .get<DialogDto[]>('/v1/dialogs', { params: patientId ? { patientId } : undefined })
      .then((r) => r.data),
  createDialog: (payload: CreateDialogRequest) =>
    api.post<DialogDto>('/v1/dialogs', payload).then((r) => r.data),
  closeDialog: (id: string) => api.post<DialogDto>(`/v1/dialogs/${id}/close`).then((r) => r.data),

  // Messages
  getMessages: (dialogId: string) =>
    api.get<MessageDto[]>(`/v1/dialogs/${dialogId}/messages`).then((r) => r.data),
  addMessage: (dialogId: string, payload: CreateMessageRequest) =>
    api.post<MessageDto>(`/v1/dialogs/${dialogId}/messages`, payload).then((r) => r.data),

  // Reference
  getSpecializations: () => api.get<SpecializationDto[]>('/v1/reference/specializations').then((r) => r.data),

  // Directory
  getDoctors: () => api.get<DoctorPublicDto[]>('/v1/doctors').then((r) => r.data),
  updateDoctor: (id: string, payload: UpdateDoctorRequest) =>
    api.put<DoctorPublicDto>(`/v1/doctors/${id}`, payload).then((r) => r.data),

  // Patient directory
  getPatientsDirectory: () => api.get<PatientDirectoryDto[]>('/v1/patient-directory').then((r) => r.data),
  updatePatientDirectory: (id: string, payload: UpdatePatientDirectoryRequest) =>
    api.put<PatientDirectoryDto>(`/v1/patient-directory/${id}`, payload).then((r) => r.data),
  createPatientDirectoryTest: () => api.post<PatientDirectoryDto>('/v1/patient-directory/test', {}).then((r) => r.data),
  createDoctorTest: () => api.post<DoctorPublicDto>('/v1/doctors/test', {}).then((r) => r.data),
}

export default api
