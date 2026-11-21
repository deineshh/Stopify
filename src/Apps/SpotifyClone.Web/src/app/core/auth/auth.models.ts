/**
 * Request / response contracts of the Accounts API (SpotifyClone.Api/Contracts/v1/Accounts).
 * Property names follow the API's camelCase JSON.
 */

/** Values accepted by the backend `Gender` value object. */
export type GenderValue = 'male' | 'female' | 'non_binary' | 'something_else' | 'not_specified';

export interface TokenResponse {
  accessToken: string;
  /** ISO timestamp. */
  expiresAt: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  displayName: string;
  /** ISO date-time. */
  birthDate: string;
  gender: GenderValue;
}

export interface RegisterResponse extends TokenResponse {
  userId: string;
  email: string;
  displayName: string;
  birthDateUtc: string | null;
  gender: GenderValue;
}

export interface PasswordLoginRequest {
  /** Email or username. */
  identifier: string;
  password: string;
}

export interface OtpRequest {
  phoneNumber: string;
}

export interface OtpSentResponse {
  expiresInSeconds: number;
}

export interface OtpVerifyRequest {
  phoneNumber: string;
  code: string;
}

export interface VerifyEmailRequest {
  userId: string;
  code: string;
}

export interface PasswordResetRequest {
  email: string;
}

export interface PasswordResetRequestedResponse {
  expiresInSeconds: number;
  resendAvailableInSeconds: number;
}

export interface PasswordResetVerifyRequest {
  email: string;
  code: string;
}

export interface PasswordResetConfirmRequest {
  email: string;
  code: string;
  newPassword: string;
}

export interface ImageMetadata {
  imageId?: string;
  url?: string;
  width?: number;
  height?: number;
}

/** `GET /users/me` (CurrentUserDetails). */
export interface CurrentUser {
  id: string;
  username: string;
  email: string;
  phoneNumber: string | null;
  isEmailConfirmed: boolean;
  isPhoneNumberConfirmed: boolean;
  isTwoFactorEnabled: boolean;
  role: string;
  displayName: string;
  gender: GenderValue;
  birthDateUtc: string | null;
  avatar: ImageMetadata | null;
}
