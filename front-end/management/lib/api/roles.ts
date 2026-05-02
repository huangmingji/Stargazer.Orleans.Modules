import { apiRequest, setApiUrl } from "./client"
import { getAccessToken } from "./auth"
import type {
  ApiResponse,
  RoleData,
  PageResult,
  RoleListParams,
} from "./types"

const BASE_URL = process.env.NEXT_PUBLIC_USERS_API_URL || "http://localhost:5000/users"

function getRolesUrl() {
  setApiUrl(BASE_URL)
  return BASE_URL
}

export async function getRoles(params: RoleListParams = {}): Promise<PageResult<RoleData>> {
  const url = getRolesUrl()
  const queryParams = new URLSearchParams()
  if (params.keyword) queryParams.set("keyword", params.keyword)
  if (params.page_index) queryParams.set("pageIndex", String(params.page_index))
  if (params.page_size) queryParams.set("pageSize", String(params.page_size))

  const query = queryParams.toString()
  const response = await apiRequest<ApiResponse<PageResult<RoleData>>>(
    `/api/role${query ? `?${query}` : ""}`,
    { token: getAccessToken() }
  )

  if (response.code !== "success") {
    throw new Error(response.message || "Failed to fetch roles")
  }

  return response.data!
}

export async function getRole(id: string): Promise<RoleData> {
  const url = getRolesUrl()
  const response = await apiRequest<ApiResponse<RoleData>>(
    `/api/role/${id}`,
    { token: getAccessToken() }
  )

  if (response.code !== "success") {
    throw new Error(response.message || "Failed to fetch role")
  }

  return response.data!
}

export async function createRole(data: { name: string; description?: string; permissionIds?: string[] }): Promise<void> {
  const url = getRolesUrl()
  const response = await apiRequest<ApiResponse>(
    "/api/role",
    {
      method: "POST",
      body: data,
      token: getAccessToken(),
    }
  )

  if (response.code !== "success") {
    throw new Error(response.message || "Failed to create role")
  }
}

export async function updateRole(id: string, data: { name?: string; description?: string; permissionIds?: string[] }): Promise<void> {
  const url = getRolesUrl()
  const response = await apiRequest<ApiResponse>(
    `/api/role/${id}`,
    {
      method: "PUT",
      body: data,
      token: getAccessToken(),
    }
  )

  if (response.code !== "success") {
    throw new Error(response.message || "Failed to update role")
  }
}

export async function deleteRole(id: string): Promise<void> {
  const url = getRolesUrl()
  const response = await apiRequest<ApiResponse>(
    `/api/role/${id}`,
    {
      method: "DELETE",
      token: getAccessToken(),
    }
  )

  if (response.code !== "success") {
    throw new Error(response.message || "Failed to delete role")
  }
}