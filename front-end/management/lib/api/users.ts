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
  PermissionData,
} from "./types"

const BASE_URL = process.env.NEXT_PUBLIC_USERS_API_URL || "http://localhost:5000/users"

function getUsersUrl() {
  setApiUrl(BASE_URL)
  return BASE_URL
}

export async function getUsers(params: UserListParams = {}): Promise<PageResult<UserData>> {
  setApiUrl(BASE_URL)
  const queryParams = new URLSearchParams()
  if (params.keyword) queryParams.set("keyword", params.keyword)
  if (params.page_index) queryParams.set("pageIndex", String(params.page_index))
  if (params.page_size) queryParams.set("pageSize", String(params.page_size))

  const query = queryParams.toString()
  const response = await apiRequest<PageResult<UserData>>(
    `/api/user${query ? `?${query}` : ""}`,
    { token: getAccessToken() }
  )

  return response
}

export async function getUser(id: string): Promise<UserData> {
  setApiUrl(BASE_URL)
  const response = await apiRequest<ApiResponse<UserData>>(
    `/api/user/${id}`,
    { token: getAccessToken() }
  )

  return response as unknown as UserData
}

export async function createUser(data: CreateOrUpdateUserInput): Promise<void> {
  setApiUrl(BASE_URL)
  await apiRequest<ApiResponse>(
    "/api/user",
    {
      method: "POST",
      body: data,
      token: getAccessToken(),
    }
  )
}

export async function updateUser(id: string, data: CreateOrUpdateUserInput): Promise<void> {
  setApiUrl(BASE_URL)
  await apiRequest<ApiResponse>(
    `/api/user/${id}`,
    {
      method: "PUT",
      body: data,
      token: getAccessToken(),
    }
  )
}

export async function deleteUser(id: string): Promise<void> {
  setApiUrl(BASE_URL)
  await apiRequest<ApiResponse>(
    `/api/user/${id}`,
    {
      method: "DELETE",
      token: getAccessToken(),
    }
  )
}

export async function updateUserStatus(id: string, is_enabled: boolean): Promise<void> {
  setApiUrl(BASE_URL)
  const input: UpdateUserStatusInput = { is_enabled }
  await apiRequest<ApiResponse>(
    `/api/user/${id}/status`,
    {
      method: "PATCH",
      body: input,
      token: getAccessToken(),
    }
  )
}

export async function getUserRoles(userId: string): Promise<RoleData[]> {
  setApiUrl(BASE_URL)
  const response = await apiRequest<RoleData[]>(
    `/api/user/${userId}/roles`,
    { token: getAccessToken() }
  )

  return response
}

export async function assignUserRoles(userId: string, roleIds: string[]): Promise<void> {
  setApiUrl(BASE_URL)
  await apiRequest<ApiResponse>(
    `/api/user/${userId}/roles`,
    {
      method: "POST",
      body: roleIds,
      token: getAccessToken(),
    }
  )
}

export async function getUserPermissions(userId: string): Promise<PermissionData[]> {
  setApiUrl(BASE_URL)
  const response = await apiRequest<PermissionData[]>(
    `/api/user/${userId}/permissions`,
    { token: getAccessToken() }
  )

  return response
}