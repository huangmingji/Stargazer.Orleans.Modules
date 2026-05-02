import axios, { AxiosInstance, AxiosRequestConfig, AxiosError, InternalAxiosRequestConfig } from "axios"

let apiUrl = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5000"

export class ApiError extends Error {
  constructor(
    message: string,
    public code?: string,
    public status?: number
  ) {
    super(message)
    this.name = "ApiError"
  }
}

function createApiClient(): AxiosInstance {
  const client = axios.create({
    baseURL: apiUrl,
    timeout: 30000,
    headers: {
      "Content-Type": "application/json",
    },
  })

  client.interceptors.request.use(
    (config: InternalAxiosRequestConfig) => {
      return config
    },
    (error) => Promise.reject(error)
  )

  client.interceptors.response.use(
    (response) => response,
    (error: AxiosError) => {
      if (error.response) {
        const data = error.response.data as { message?: string; code?: string }
        throw new ApiError(
          data?.message || error.message,
          data?.code,
          error.response.status
        )
      }
      if (error.request) {
        throw new ApiError(
          "Network error: No response received",
          "network_error"
        )
      }
      throw new ApiError(
        error.message,
        "unknown_error"
      )
    }
  )

  return client
}

let apiClient = createApiClient()

export function setApiUrl(url: string) {
  apiUrl = url
  apiClient = createApiClient()
}

export function resetApiClient() {
  apiClient = createApiClient()
}

export function getApiClient(): AxiosInstance {
  return apiClient
}

export interface RequestOptions {
  method?: "GET" | "POST" | "PUT" | "PATCH" | "DELETE"
  body?: unknown
  token?: string | null
  params?: Record<string, string | number>
}

export async function apiRequest<T>(
  endpoint: string,
  options: RequestOptions = {}
): Promise<T> {
  const { method = "GET", body, token, params } = options

  const config: AxiosRequestConfig = {
    url: endpoint,
    method,
    params,
  }

  if (body) {
    config.data = body
  }

  if (token) {
    config.headers = {
      Authorization: `Bearer ${token}`,
    }
  }

  const response = await apiClient.request<T>(config)
  return response.data
}