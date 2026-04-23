import { apiRequest, setApiUrl } from "./client"
import type {
  ApiResponse,
  LoginRequest,
  TokenResponse,
} from "./types"

const ACCOUNT_TOKEN_KEY = "account_access_token"
const REFRESH_TOKEN_KEY = "account_refresh_token"

export function getAccessToken(): string | null {
  if (typeof window === "undefined") return null
  return localStorage.getItem(ACCOUNT_TOKEN_KEY)
}

export function getRefreshToken(): string | null {
  if (typeof window === "undefined") return null
  return localStorage.getItem(REFRESH_TOKEN_KEY)
}

export function setTokens(accessToken: string, refreshToken: string): void {
  if (typeof window === "undefined") return
  localStorage.setItem(ACCOUNT_TOKEN_KEY, accessToken)
  localStorage.setItem(REFRESH_TOKEN_KEY, refreshToken)
}

export function clearTokens(): void {
  if (typeof window === "undefined") return
  localStorage.removeItem(ACCOUNT_TOKEN_KEY)
  localStorage.removeItem(REFRESH_TOKEN_KEY)
}

export function isAuthenticated(): boolean {
  return !!getAccessToken()
}

export async function login(
  credentials: LoginRequest
): Promise<TokenResponse> {
  setApiUrl(process.env.NEXT_PUBLIC_USERS_API_URL || "http://localhost:5000/users")
  const response = await apiRequest<ApiResponse>(`/api/account/login`, {
    method: "POST",
    body: JSON.stringify(credentials),
  })

  if (response.code !== "success") {
    throw new Error(response.message || "Login failed")
  }

  const tokenData = response.data as TokenResponse
  setTokens(tokenData.AccessToken, tokenData.RefreshToken)

  return tokenData
}

export async function refreshToken(): Promise<TokenResponse | null> {
  const refreshTokenValue = getRefreshToken()
  if (!refreshTokenValue) return null

  setApiUrl(process.env.NEXT_PUBLIC_USERS_API_URL || "http://localhost:5000/users")
  try {
    const response = await apiRequest<ApiResponse>(`/api/account/refresh`, {
      method: "POST",
      body: JSON.stringify({ RefreshToken: refreshTokenValue }),
    })

    if (response.code !== "success") {
      clearTokens()
      return null
    }

    const tokenData = response.data as TokenResponse
    setTokens(tokenData.AccessToken, tokenData.RefreshToken)
    return tokenData
  } catch {
    clearTokens()
    return null
  }
}

export function logout(): void {
  clearTokens()
  window.location.href = "/login"
}