import { type UserDto } from './UserDto'

export interface AuthResultDto {
  user: UserDto;
  accessToken: string;
  accessTokenExpiresAtUtc: string;
  refreshToken: string;
  refreshTokenExpiresAtUtc: string;
}