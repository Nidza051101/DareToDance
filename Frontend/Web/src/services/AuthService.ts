import axios from 'axios';
import api from './api';
import { type AuthResultDto } from '../types/AuthResultDto';
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

    // 404 means "token is valid, no account for this email yet" - a real,
    // expected outcome for this endpoint, not a failure - so it's returned
    // as data (needsRegistration), never thrown. Any other non-2xx (e.g. a
    // bad/expired Google token, 401) is left to throw for the caller.
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
}

export default new AuthService();