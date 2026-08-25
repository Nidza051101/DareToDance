import { type AuthResultDto } from '../types/AuthResultDto';

export interface IAuthService {
  requestOtp(email: string): Promise<void>;
  verifyOtp(email: string, code: string): Promise<AuthResultDto>;
  getMe(): Promise<{ userId: string }>;
  refresh(refreshToken: string): Promise<AuthResultDto>;
  logout(refreshToken: string): Promise<void>;
}