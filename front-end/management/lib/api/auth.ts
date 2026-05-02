import { apiRequest, setApiUrl } from "./client"
import type {
  ApiResponse,
  LoginRequest,
  TokenResponse,
} from "./types"

const ACCOUNT_TOKEN_KEY = "account_access_token"
const REFRESH_TOKEN_KEY = "account_refresh_token"
const TOKEN_EXPIRY_KEY = "account_token_expiry"

export function getAccessToken(): string | null {
  if (typeof window === "undefined") return null
  const expiry = localStorage.getItem(TOKEN_EXPIRY_KEY)
  if (expiry && new Date(expiry).getTime() < Date.now()) {
    localStorage.removeItem(ACCOUNT_TOKEN_KEY)
    localStorage.removeItem(REFRESH_TOKEN_KEY)
    localStorage.removeItem(TOKEN_EXPIRY_KEY)
    return null
  }
  return localStorage.getItem(ACCOUNT_TOKEN_KEY)
}

export function getRefreshToken(): string | null {
  if (typeof window === "undefined") return null
  return localStorage.getItem(REFRESH_TOKEN_KEY)
}

export function setTokens(accessToken: string, refreshToken: string, expiresAt?: string): void {
  if (typeof window === "undefined") return
  localStorage.setItem(ACCOUNT_TOKEN_KEY, accessToken)
  localStorage.setItem(REFRESH_TOKEN_KEY, refreshToken)
  if (expiresAt) {
    localStorage.setItem(TOKEN_EXPIRY_KEY, expiresAt)
  }
}

export function clearTokens(): void {
  if (typeof window === "undefined") return
  localStorage.removeItem(ACCOUNT_TOKEN_KEY)
  localStorage.removeItem(REFRESH_TOKEN_KEY)
  localStorage.removeItem(TOKEN_EXPIRY_KEY)
}

export function isAuthenticated(): boolean {
  return !!getAccessToken()
}

export async function login(
  credentials: LoginRequest
): Promise<TokenResponse> {
  setApiUrl(process.env.NEXT_PUBLIC_USERS_API_URL || "http://localhost:5000/users")
  
  const response = await apiRequest<TokenResponse>(`/api/account/login`, {
    method: "POST",
    body: credentials,
  })
  setTokens(response.access_token, response.refresh_token, response.expires_at)
  return response
}

export async function refreshToken(): Promise<TokenResponse | null> {
  const refreshTokenValue = getRefreshToken()
  if (!refreshTokenValue) return null

  setApiUrl(process.env.NEXT_PUBLIC_USERS_API_URL || "http://localhost:5000/users")
  const response = await apiRequest<TokenResponse>(`/api/account/refresh`, {
    method: "POST",
    body: { RefreshToken: refreshTokenValue },
  })
  setTokens(response.access_token, response.refresh_token, response.expires_at)
  return response
}

export function logout(): void {
  clearTokens()
  window.location.href = "/login"
}