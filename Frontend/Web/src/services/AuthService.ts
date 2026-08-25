import axios from 'axios';
import api from './api';
import { type AuthResultDto } from '../types/AuthResultDto';
import { type GoogleAuthResultDto } from '../types/GoogleAuthResultDto';
import { type GoogleIdentityDto } from '../types/GoogleIdentityDto';
import { type GoogleLoginResult } from '../types/GoogleLoginResult';
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

    async loginWithGoogle(idToken: string): Promise<GoogleLoginResult> {
        try {
            const response = await api.post('auth/google', { idToken });
            localStorage.setItem('accessToken', response.data.accessToken);
            localStorage.setItem('refreshToken', response.data.refreshToken);
            return { status: 'loggedIn', auth: response.data };
        } catch (err) {
            if (axios.isAxiosError(err) && err.response?.status === 404) {
                return {
                    status: 'needsRegistration',
                    identity: err.response.data as GoogleIdentityDto,
                };
            }

            throw err;
        }
    }

    async completeRegistration(idToken: string, phone: string): Promise<GoogleAuthResultDto> {
        const response = await api.post('auth/google/complete-registration', { idToken, phone });
        localStorage.setItem('accessToken', response.data.accessToken);
        localStorage.setItem('refreshToken', response.data.refreshToken);
        return response.data;
    }
}

export default new AuthService();