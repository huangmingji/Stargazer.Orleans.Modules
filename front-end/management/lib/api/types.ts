export interface LoginRequest {
  Account: string
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
  access_token: string
  refresh_token: string
  expires_at: string
  user: UserData
}

export interface UserData {
  id: string
  account: string
  name?: string
  email?: string
  phone_number?: string
  avatar?: string
  is_active: boolean
  creator_id: string
  creation_time: string
  last_modifier_id?: string
  last_modify_time?: string
  roles?: RoleData[]
}

export interface CreateOrUpdateUserInput {
  account: string
  password?: string
  name?: string
  email?: string
  phone_number?: string
}

export interface UpdateUserStatusInput {
  is_enabled: boolean
}

export interface PageResult<T> {
  items: T[]
  total_count: number
  page_index: number
  page_size: number
}

export interface RoleData {
  id: string
  name: string
  description?: string
  is_default: boolean
  priority: number
  is_active: boolean
  permissions: PermissionData[]
  creation_time: string
}

export interface PermissionData {
  id: string
  name: string
  description?: string
  category?: string
}

export interface ApiResponse<T = unknown> {
  code: string
  message: string
  data?: T
}

export type UserListParams = {
  keyword?: string
  page_index?: number
  page_size?: number
}

export type RoleListParams = {
  keyword?: string
  page_index?: number
  page_size?: number
}