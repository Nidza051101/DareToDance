import api from './api'

export interface MeResponse {
  userId: string
}

const authService = {
  getMe: () => api.get<MeResponse>('/auth/me'),

  logout: async () => {
    const refreshToken = localStorage.getItem('refreshToken')
    await api.post('/auth/logout', { refreshToken })
    localStorage.removeItem('accessToken')
    localStorage.removeItem('refreshToken')
  },
}

export default authService
