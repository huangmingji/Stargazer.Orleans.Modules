"use client"

import * as React from "react"
import { useRouter } from "next/navigation"
import { toast } from "sonner"

import { UserTable } from "@/components/users/user-table"
import { UserDialog } from "@/components/users/user-dialog"
import { useAuth } from "@/hooks/use-auth"
import {
  getUsers,
  createUser,
  updateUser,
  deleteUser,
  updateUserStatus,
  type UserData,
  type CreateOrUpdateUserInput,
} from "@/lib/api"
import { de } from "zod/locales"

export default function UsersPage() {
  const router = useRouter()
  const { isAuthenticated, isLoading: authLoading } = useAuth()

  const [data, setData] = React.useState<UserData[]>([])
  const [totalCount, setTotalCount] = React.useState(0)
  const [pageIndex, setPageIndex] = React.useState(0)
  const [pageSize, setPageSize] = React.useState(10)
  const [keyword, setKeyword] = React.useState("")
  const [isLoading, setIsLoading] = React.useState(true)

  const [dialogOpen, setDialogOpen] = React.useState(false)
  const [editingUser, setEditingUser] = React.useState<UserData | null>(null)

  React.useEffect(() => {
    debugger
    if (!authLoading && !isAuthenticated) {
      router.push("/login")
    }
  }, [authLoading, isAuthenticated, router])

  const fetchUsers = React.useCallback(async () => {
    setIsLoading(true)
    try {
      const result = await getUsers({
        keyword: keyword || undefined,
        page_index: pageIndex + 1,
        page_size: pageSize,
      })
      setData(result.items)
      setTotalCount(result.total_count)
    } catch (error) {
      toast.error(error instanceof Error ? error.message : "Failed to fetch users")
    } finally {
      setIsLoading(false)
    }
  }, [pageIndex, pageSize, keyword])

  React.useEffect(() => {
    if (isAuthenticated) {
      fetchUsers()
    }
  }, [isAuthenticated, fetchUsers])

  const handlePageChange = (page: number) => {
    setPageIndex(page)
  }

  const handlePageSizeChange = (size: number) => {
    setPageSize(size)
    setPageIndex(0)
  }

  const handleSearch = (searchKeyword: string) => {
    setKeyword(searchKeyword)
    setPageIndex(0)
  }

  const handleAddUser = () => {
    setEditingUser(null)
    setDialogOpen(true)
  }

  const handleEditUser = (user: UserData) => {
    setEditingUser(user)
    setDialogOpen(true)
  }

  const handleDeleteUser = async (user: UserData) => {
    if (!confirm(`Are you sure you want to delete user "${user.account}"?`)) {
      return
    }

    try {
      await deleteUser(user.id)
      toast.success("User deleted successfully")
      fetchUsers()
    } catch (error) {
      toast.error(error instanceof Error ? error.message : "Failed to delete user")
    }
  }

  const handleToggleStatus = async (user: UserData) => {
    try {
      await updateUserStatus(user.id, !user.is_active)
      toast.success(`User ${user.is_active ? "disabled" : "enabled"} successfully`)
      fetchUsers()
    } catch (error) {
      toast.error(error instanceof Error ? error.message : "Failed to update user status")
    }
  }

  const handleSubmit = async (formData: CreateOrUpdateUserInput) => {
    try {
      if (editingUser) {
        const updateData = { ...formData }
        if (!updateData.password) {
          delete updateData.password
        }
        await updateUser(editingUser.id, updateData)
        toast.success("User updated successfully")
      } else {
        await createUser(formData)
        toast.success("User created successfully")
      }
      fetchUsers()
    } catch (error) {
      throw error
    }
  }

  if (authLoading || !isAuthenticated) {
    return (
      <div className="flex h-screen items-center justify-center">
        <div className="h-8 w-8 animate-spin rounded-full border-4 border-primary border-t-transparent" />
      </div>
    )
  }

  return (
    <div className="container mx-auto py-6">
      <div className="mb-6">
        <h1 className="text-3xl font-bold">User Management</h1>
        <p className="text-muted-foreground">Manage system users and their permissions</p>
      </div>

      <UserTable
        data={data}
        total_count={totalCount}
        page_index={pageIndex}
        page_size={pageSize}
        onPageChange={handlePageChange}
        onPageSizeChange={handlePageSizeChange}
        onSearch={handleSearch}
        onEdit={handleEditUser}
        onDelete={handleDeleteUser}
        onToggleStatus={handleToggleStatus}
        onAddUser={handleAddUser}
      />

      <UserDialog
        open={dialogOpen}
        onOpenChange={setDialogOpen}
        user={editingUser}
        onSubmit={handleSubmit}
      />
    </div>
  )
}