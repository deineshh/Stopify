import { CurrentUser } from '@core/auth/auth.models';

export const TEST_USER: CurrentUser = {
  id: 'u1',
  username: 'aqua',
  email: 'aqua@studios.com',
  phoneNumber: null,
  isEmailConfirmed: true,
  isPhoneNumberConfirmed: false,
  isTwoFactorEnabled: false,
  role: 'Listener',
  displayName: 'Aqua Studios',
  gender: 'male',
  birthDateUtc: null,
  avatar: null,
};
