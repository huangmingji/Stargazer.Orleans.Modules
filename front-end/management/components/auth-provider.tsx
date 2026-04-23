"use client"

import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useState,
  type ReactNode,
} from "react"
import { login as loginApi, logout as logoutApi, refreshToken, type UserData } from "@/lib/api"

interface AuthContextType {
  user: UserData | null
  isLoading: boolean
  isAuthenticated: boolean
  login: (account: string, password: string) => Promise<{ success: boolean; message?: string }>
  logout: () => void
}

const AuthContext = createContext<AuthContextType | undefined>(undefined)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<UserData | null>(null)
  const [isLoading, setIsLoading] = useState(true)

  useEffect(() => {
    refreshToken()
      .then((res) => {
        if (res) {
          setUser(res.User)
        }
      })
      .finally(() => {
        setIsLoading(false)
      })
  }, [])

  const handleLogin = useCallback(
    async (account: string, password: string) => {
      try {
        const res = await loginApi({ Name: account, Password: password })
        setUser(res.User)
        return { success: true }
      } catch (err) {
        const message = err instanceof Error ? err.message : "Login failed"
        return { success: false, message }
      }
    },
    []
  )

  const handleLogout = useCallback(() => {
    logoutApi()
    setUser(null)
  }, [])

  return (
    <AuthContext.Provider
      value={{
        user,
        isLoading,
        isAuthenticated: !!user,
        login: handleLogin,
        logout: handleLogout,
      }}
    >
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth() {
  const context = useContext(AuthContext)
  if (context === undefined) {
    throw new Error("useAuth must be used within an AuthProvider")
  }
  return context
}