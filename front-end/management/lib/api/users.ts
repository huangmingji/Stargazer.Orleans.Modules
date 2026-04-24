import { apiRequest, setApiUrl } from "./client"
import { getAccessToken } from "./auth"
import type {
  ApiResponse,
  UserData,
  CreateOrUpdateUserInput,
  UpdateUserStatusInput,
  PageResult,
  RoleData,
  UserListParams,
} from "./types"

const BASE_URL = process.env.NEXT_PUBLIC_USERS_API_URL || "http://localhost:5000/users"

function getUsersUrl() {
  setApiUrl(BASE_URL)
  return BASE_URL
}

export async function getUsers(params: UserListParams = {}): Promise<PageResult<UserData>> {
  const url = getUsersUrl()
  const queryParams = new URLSearchParams()
  if (params.keyword) queryParams.set("keyword", params.keyword)
  if (params.page_index) queryParams.set("pageIndex", String(params.page_index))
  if (params.page_size) queryParams.set("pageSize", String(params.page_size))

  const query = queryParams.toString()
  const response = await apiRequest<ApiResponse<PageResult<UserData>>>(
    `/api/user${query ? `?${query}` : ""}`,
    { token: getAccessToken() }
  )

  if (response.code !== "success") {
    throw new Error(response.message || "Failed to fetch users")
  }

  return response.data!
}

export async function getUser(id: string): Promise<UserData> {
  const url = getUsersUrl()
  const response = await apiRequest<ApiResponse<UserData>>(
    `/api/user/${id}`,
    { token: getAccessToken() }
  )

  if (response.code !== "success") {
    throw new Error(response.message || "Failed to fetch user")
  }

  return response.data!
}

export async function createUser(data: CreateOrUpdateUserInput): Promise<void> {
  const url = getUsersUrl()
  const response = await apiRequest<ApiResponse>(
    "/api/user",
    {
      method: "POST",
      body: JSON.stringify(data),
      token: getAccessToken(),
    }
  )

  if (response.code !== "success") {
    throw new Error(response.message || "Failed to create user")
  }
}

export async function updateUser(id: string, data: CreateOrUpdateUserInput): Promise<void> {
  const url = getUsersUrl()
  const response = await apiRequest<ApiResponse>(
    `/api/user/${id}`,
    {
      method: "PUT",
      body: JSON.stringify(data),
      token: getAccessToken(),
    }
  )

  if (response.code !== "success") {
    throw new Error(response.message || "Failed to update user")
  }
}

export async function deleteUser(id: string): Promise<void> {
  const url = getUsersUrl()
  const response = await apiRequest<ApiResponse>(
    `/api/user/${id}`,
    {
      method: "DELETE",
      token: getAccessToken(),
    }
  )

  if (response.code !== "success") {
    throw new Error(response.message || "Failed to delete user")
  }
}

export async function updateUserStatus(id: string, is_enabled: boolean): Promise<void> {
  const url = getUsersUrl()
  const input: UpdateUserStatusInput = { is_enabled: is_enabled }
  const response = await apiRequest<ApiResponse>(
    `/api/user/${id}/status`,
    {
      method: "PATCH",
      body: JSON.stringify(input),
      token: getAccessToken(),
    }
  )

  if (response.code !== "success") {
    throw new Error(response.message || "Failed to update user status")
  }
}

export async function getUserRoles(userId: string): Promise<RoleData[]> {
  const url = getUsersUrl()
  const response = await apiRequest<ApiResponse<RoleData[]>>(
    `/api/user/${userId}/roles`,
    { token: getAccessToken() }
  )

  if (response.code !== "success") {
    throw new Error(response.message || "Failed to fetch user roles")
  }

  return response.data || []
}

export async function assignUserRoles(userId: string, roleIds: string[]): Promise<void> {
  const url = getUsersUrl()
  const response = await apiRequest<ApiResponse>(
    `/api/user/${userId}/roles`,
    {
      method: "POST",
      body: JSON.stringify(roleIds),
      token: getAccessToken(),
    }
  )

  if (response.code !== "success") {
    throw new Error(response.message || "Failed to assign user roles")
  }
}

export async function getUserPermissions(userId: string): Promise<string[]> {
  const url = getUsersUrl()
  const response = await apiRequest<ApiResponse<string[]>>(
    `/api/user/${userId}/permissions`,
    { token: getAccessToken() }
  )

  if (response.code !== "success") {
    throw new Error(response.message || "Failed to fetch user permissions")
  }

  return response.data || []
}