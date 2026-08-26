import { type GoogleAuthResultDto } from './GoogleAuthResultDto'
import { type GoogleIdentityDto } from './GoogleIdentityDto'

export type GoogleLoginResult =
  | { status: 'loggedIn'; auth: GoogleAuthResultDto }
  | { status: 'needsRegistration'; identity: GoogleIdentityDto }
