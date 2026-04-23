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

interface RequestOptions extends RequestInit {
  token?: string | null
}

export function setApiUrl(url: string) {
  apiUrl = url
}

export async function apiRequest<T>(
  endpoint: string,
  options: RequestOptions = {}
): Promise<T> {
  const { token, ...fetchOptions } = options

  const headers: Record<string, string> = {
    "Content-Type": "application/json",
    ...(fetchOptions.headers as Record<string, string> || {}),
  }

  if (token) {
    headers.Authorization = `Bearer ${token}`
  }

  const response = await fetch(`${apiUrl}${endpoint}`, {
    ...fetchOptions,
    headers,
  })

  const data = await response.json()

  if (!response.ok) {
    throw new ApiError(
      data.message || "Request failed",
      data.code,
      response.status
    )
  }

  return data as T
}