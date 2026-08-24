import api from './api'

export interface MeResponse {
  userId: string
}

export interface VerifyOtpResponse {
  accessToken: string
  tokenType: string
  expiresAtUtc: string
  userId: string
  refreshToken: string
  refreshTokenExpiresAtUtc: string
}

const authService = {
  getMe: () => api.get<MeResponse>('/auth/me'),

  verifyOtp: async (email: string, code: string) => {
    const response = await api.post<VerifyOtpResponse>('/auth/otp/verify', { email, code })

    localStorage.setItem('accessToken', response.data.accessToken)
    localStorage.setItem('refreshToken', response.data.refreshToken)

    console.log('accessToken:', response.data.accessToken)
    console.log('refreshToken:', response.data.refreshToken)

    return response
  },

  logout: async () => {
    const refreshToken = localStorage.getItem('refreshToken')
    await api.post('/auth/logout', { refreshToken })
    localStorage.removeItem('accessToken')
    localStorage.removeItem('refreshToken')
  },
}

export default authService
