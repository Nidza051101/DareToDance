import api from './api';
import { type AuthResultDto } from '../types/AuthResultDto';
import { type IAuthService } from './IAuthService';

class AuthService implements IAuthService {

    async requestOtp(email: string): Promise<void>
    {
        await api.post('auth/otp/request',{email});
    }

    async verifyOtp(email: string, code: string): Promise<AuthResultDto> {
        const response = await api.post('auth/otp/verify', { email, code });
        localStorage.setItem('accessToken',response.data.accessToken);
        localStorage.setItem('refreshToken',response.data.refreshToken);
        return response.data;
    }

    async getMe(): Promise<{ userId: string }>
    {
        const response = await api.get('auth/me');
        return response.data;
    }

    async refresh(refreshToken: string): Promise<AuthResultDto> {
        const response = await api.post('auth/refresh', { refreshToken });
        return response.data;
    }

    async logout(refreshToken: string): Promise<void> {
        await api.post('auth/logout', { refreshToken });
        localStorage.clear();
    }
}

export default new AuthService();