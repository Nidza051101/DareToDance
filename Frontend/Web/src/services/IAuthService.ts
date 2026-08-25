import { type AuthResultDto } from '../types/AuthResultDto';
import { type GoogleAuthResultDto } from '../types/GoogleAuthResultDto';
import { type GoogleLoginResult } from '../types/GoogleLoginResult';

export interface IAuthService {
  requestOtp(email: string): Promise<void>;
  verifyOtp(email: string, code: string): Promise<AuthResultDto>;
  getMe(): Promise<{ userId: string }>;
  refresh(refreshToken: string): Promise<AuthResultDto>;
  logout(refreshToken: string): Promise<void>;
  loginWithGoogle(idToken: string): Promise<GoogleLoginResult>;
  completeRegistration(idToken: string, phone: string): Promise<GoogleAuthResultDto>;
}