import api from './api'

export interface UserResponse {
  id: string
  email: string
  firstName: string
  lastName: string
  phone: string | null
  createdAtUtc: string
}

const userService = {
  getUserById: (id: string) => api.get<UserResponse>(`/users/${id}`),
}

export default userService
