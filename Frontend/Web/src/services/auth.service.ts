import api from './api'

export interface MeResponse {
  userId: string
}

const authService = {
  getMe: () => api.get<MeResponse>('/auth/me'),
}

export default authService
