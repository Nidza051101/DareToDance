export interface GoogleAuthResultDto {
  accessToken: string;
  tokenType: string;
  expiresAtUtc: string;
  userId: string;
  refreshToken: string;
  refreshTokenExpiresAtUtc: string;
}
