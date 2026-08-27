import axios from 'axios';
import api from './api';
import { type AuthResultDto } from '../types/AuthResultDto';
import { type GoogleAuthResultDto } from '../types/GoogleAuthResultDto';
import { type GoogleIdentityDto } from '../types/GoogleIdentityDto';
import { type GoogleLoginResult } from '../types/GoogleLoginResult';
import { type IAuthService } from './IAuthService';
import { clearAccessToken, setAccessToken } from './authToken';

class AuthService implements IAuthService {

    async requestOtp(email: string): Promise<void>
    {
        await api.post('auth/otp/request',{email});
    }

    async verifyOtp(email: string, code: string): Promise<AuthResultDto> {
        const response = await api.post('auth/otp/verify', { email, code });
        setAccessToken(response.data.accessToken);
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

    async logout(): Promise<void> {
        // Bez tela zahteva — backend čita refreshToken iz HttpOnly kolačića.
        await api.post('auth/logout');
        clearAccessToken();
    }

    async loginWithGoogle(idToken: string): Promise<GoogleLoginResult> {
        try {
            const response = await api.post('auth/google', { idToken });
            setAccessToken(response.data.accessToken);
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
        setAccessToken(response.data.accessToken);
        return response.data;
    }
}

export default new AuthService();