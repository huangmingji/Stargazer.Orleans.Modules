export interface LoginRequest {
  Name: string
  Password: string
}

export interface RegisterRequest {
  Account: string
  Password: string
  Email?: string
}

export interface RefreshTokenRequest {
  RefreshToken: string
}

export interface TokenResponse {
  AccessToken: string
  RefreshToken: string
  ExpiresAt: string
  User: UserData
}

export interface UserData {
  id: string
  account: string
  email?: string
  name?: string
  isEnabled: boolean
  creationTime: string
}

export interface ApiResponse {
  code: string
  message: string
  data?: unknown
}