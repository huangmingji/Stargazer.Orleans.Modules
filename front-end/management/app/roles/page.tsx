"use client"

import * as React from "react"
import { useRouter } from "next/navigation"
import { toast } from "sonner"
import {
  IconChevronLeft,
  IconChevronRight,
  IconDotsVertical,
  IconPlus,
  IconSearch,
  IconShield,
} from "@tabler/icons-react"
import {
  getCoreRowModel,
  getPaginationRowModel,
  useReactTable,
  type ColumnDef,
} from "@tanstack/react-table"

import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import { Checkbox } from "@/components/ui/checkbox"
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu"
import { Input } from "@/components/ui/input"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select"
import {
  Sheet,
  SheetContent,
  SheetDescription,
  SheetFooter,
  SheetHeader,
  SheetTitle,
} from "@/components/ui/sheet"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table"
import { useAuth } from "@/hooks/use-auth"
import { getRoles, createRole, updateRole, deleteRole, type RoleData, type ApiResponse, type PageResult } from "@/lib/api"
import { setApiUrl, apiRequest } from "@/lib/api/client"
import { getAccessToken } from "@/lib/api/auth"

const BASE_URL = process.env.NEXT_PUBLIC_USERS_API_URL || "http://localhost:5000/users"

export default function RolesPage() {
  const router = useRouter()
  const { isAuthenticated, isLoading: authLoading } = useAuth()

  const [data, setData] = React.useState<RoleData[]>([])
  const [totalCount, setTotalCount] = React.useState(0)
  const [pageIndex, setPageIndex] = React.useState(0)
  const [pageSize, setPageSize] = React.useState(10)
  const [keyword, setKeyword] = React.useState("")
  const [isLoading, setIsLoading] = React.useState(true)

  const [dialogOpen, setDialogOpen] = React.useState(false)
  const [editingRole, setEditingRole] = React.useState<RoleData | null>(null)
  const [searchKeyword, setSearchKeyword] = React.useState("")

  React.useEffect(() => {
    if (!authLoading && !isAuthenticated) {
      router.push("/login")
    }
  }, [authLoading, isAuthenticated, router])

  const fetchRoles = React.useCallback(async () => {
    setIsLoading(true)
    setApiUrl(BASE_URL)
    try {
      const queryParams = new URLSearchParams()
      if (keyword) queryParams.set("keyword", keyword)
      queryParams.set("pageIndex", String(pageIndex + 1))
      queryParams.set("pageSize", String(pageSize))
      const query = queryParams.toString()

      const response = await apiRequest<ApiResponse<PageResult<RoleData>>>(
        `/api/role${query ? `?${query}` : ""}`,
        { token: getAccessToken() }
      )

      if (response.code !== "success") {
        throw new Error(response.message || "Failed to fetch roles")
      }

      setData(response.data!.items)
      setTotalCount(response.data!.total_count)
    } catch (error) {
      toast.error(error instanceof Error ? error.message : "Failed to fetch roles")
    } finally {
      setIsLoading(false)
    }
  }, [pageIndex, pageSize, keyword])

  React.useEffect(() => {
    if (isAuthenticated) {
      fetchRoles()
    }
  }, [isAuthenticated, fetchRoles])

  const handlePageChange = (page: number) => {
    setPageIndex(page)
  }

  const handlePageSizeChange = (size: number) => {
    setPageSize(size)
    setPageIndex(0)
  }

  const handleSearch = () => {
    setKeyword(searchKeyword)
    setPageIndex(0)
  }

  const handleAddRole = () => {
    setEditingRole(null)
    setDialogOpen(true)
  }

  const handleEditRole = (role: RoleData) => {
    setEditingRole(role)
    setDialogOpen(true)
  }

  const handleDeleteRole = async (role: RoleData) => {
    if (!confirm(`Are you sure you want to delete role "${role.name}"?`)) {
      return
    }

    setApiUrl(BASE_URL)
    try {
      const response = await apiRequest<ApiResponse>(
        `/api/role/${role.id}`,
        { method: "DELETE", token: getAccessToken() }
      )
      if (response.code !== "success") {
        throw new Error(response.message || "Failed to delete role")
      }
      toast.success("Role deleted successfully")
      fetchRoles()
    } catch (error) {
      toast.error(error instanceof Error ? error.message : "Failed to delete role")
    }
  }

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault()
    const formData = new FormData(e.currentTarget)
    const roleData = {
      name: formData.get("name") as string,
      description: formData.get("description") as string || undefined,
    }

    setApiUrl(BASE_URL)
    try {
      const endpoint = editingRole ? `/api/role/${editingRole.id}` : "/api/role"
      const method = editingRole ? "PUT" : "POST"

      const response = await apiRequest<ApiResponse>(endpoint, {
        method,
        body: JSON.stringify(roleData),
        token: getAccessToken(),
      })

      if (response.code !== "success") {
        throw new Error(response.message || `Failed to ${editingRole ? "update" : "create"} role`)
      }

      toast.success(`Role ${editingRole ? "updated" : "created"} successfully`)
      setDialogOpen(false)
      fetchRoles()
    } catch (error) {
      toast.error(error instanceof Error ? error.message : "Failed to save role")
    }
  }

  const columns: ColumnDef<RoleData>[] = [
    {
      id: "select",
      header: ({ table }) => (
        <Checkbox
          checked={table.getIsAllPageRowsSelected()}
          onCheckedChange={(value) => table.toggleAllPageRowsSelected(!!value)}
          aria-label="Select all"
        />
      ),
      cell: ({ row }) => (
        <Checkbox
          checked={row.getIsSelected()}
          onCheckedChange={(value) => row.toggleSelected(!!value)}
          aria-label="Select row"
        />
      ),
      enableSorting: false,
    },
    {
      accessorKey: "name",
      header: "Name",
      cell: ({ row }) => (
        <div className="flex items-center gap-2">
          <IconShield className="h-4 w-4" />
          <span className="font-medium">{row.original.name}</span>
        </div>
      ),
    },
    {
      accessorKey: "description",
      header: "Description",
      cell: ({ row }) => row.original.description || "-",
    },
    {
      accessorKey: "permissions",
      header: "Permissions",
      cell: ({ row }) => (
        <Badge variant="outline">{row.original.permissions?.length || 0}</Badge>
      ),
    },
    {
accessorKey: "is_active",
        cell: ({ row }) => (
          <Badge variant={row.original.is_active ? "default" : "secondary"}>
            {row.original.is_active ? "Active" : "Disabled"}
          </Badge>
        ),
    },
    {
accessorKey: "creation_time",
      cell: ({ row }) => {
        const date = new Date(row.original.creation_time)
        return date.toLocaleDateString()
      },
    },
    {
      id: "actions",
      cell: ({ row }) => {
        const role = row.original
        return (
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button variant="ghost" size="icon" className="h-8 w-8">
                <IconDotsVertical className="h-4 w-4" />
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end">
              <DropdownMenuItem onClick={() => handleEditRole(role)}>Edit</DropdownMenuItem>
              <DropdownMenuSeparator />
              <DropdownMenuItem
                onClick={() => handleDeleteRole(role)}
                className="text-destructive focus:text-destructive"
              >
                Delete
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
        )
      },
    },
  ]

  const table = useReactTable({
    data,
    columns,
    getCoreRowModel: getCoreRowModel(),
    getPaginationRowModel: getPaginationRowModel(),
    manualPagination: true,
    pageCount: Math.ceil(totalCount / pageSize),
  })

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
        <h1 className="text-3xl font-bold">Role Management</h1>
        <p className="text-muted-foreground">Manage system roles and permissions</p>
      </div>

      <div className="flex items-center gap-4 mb-4">
        <div className="relative flex-1 max-w-sm">
          <IconSearch className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
          <Input
            placeholder="Search roles..."
            className="pl-9"
            value={searchKeyword}
            onChange={(e) => setSearchKeyword(e.target.value)}
            onKeyDown={(e) => e.key === "Enter" && handleSearch()}
          />
        </div>
        <Button onClick={handleSearch}>Search</Button>
        <Button onClick={handleAddRole}>
          <IconPlus className="mr-2 h-4 w-4" />
          Add Role
        </Button>
      </div>

      <div className="rounded-md border">
        <Table>
          <TableHeader>
            {table.getHeaderGroups().map((headerGroup) => (
              <TableRow key={headerGroup.id}>
                {headerGroup.headers.map((header) => (
                  <TableHead key={header.id}>
                    {header.isPlaceholder
                      ? null
                      : typeof header.column.columnDef.header === "function"
                        ? header.column.columnDef.header(header.getContext())
                        : header.column.columnDef.header}
                  </TableHead>
                ))}
              </TableRow>
            ))}
          </TableHeader>
          <TableBody>
            {table.getRowModel().rows?.length ? (
              table.getRowModel().rows.map((row) => (
                <TableRow key={row.id}>
                  {row.getVisibleCells().map((cell) => (
                    <TableCell key={cell.id}>
                      {typeof cell.column.columnDef.cell === "function"
                        ? cell.column.columnDef.cell(cell.getContext())
                        : String(cell.getValue())}
                    </TableCell>
                  ))}
                </TableRow>
              ))
            ) : (
              <TableRow>
                <TableCell colSpan={columns.length} className="h-24 text-center">
                  No results.
                </TableCell>
              </TableRow>
            )}
          </TableBody>
        </Table>
      </div>

      <div className="flex items-center justify-between mt-4">
        <div className="text-sm text-muted-foreground">
          {table.getFilteredSelectedRowModel().rows.length} of {totalCount} row(s) selected.
        </div>
        <div className="flex items-center gap-4">
          <div className="flex items-center gap-2">
            <span className="text-sm font-medium">Rows per page</span>
            <Select value={`${pageSize}`} onValueChange={(v) => handlePageSizeChange(Number(v))}>
              <SelectTrigger className="w-20">
                <SelectValue />
              </SelectTrigger>
              <SelectContent side="top">
                {[10, 20, 30, 50].map((size) => (
                  <SelectItem key={size} value={`${size}`}>{size}</SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
          <div className="text-sm font-medium">
            Page {pageIndex + 1} of {Math.ceil(totalCount / pageSize)}
          </div>
          <div className="flex items-center gap-2">
            <Button variant="outline" size="icon" onClick={() => handlePageChange(0)} disabled={pageIndex === 0}>
              <IconChevronLeft className="h-4 w-4" />
            </Button>
            <Button variant="outline" size="icon" onClick={() => handlePageChange(pageIndex - 1)} disabled={pageIndex === 0}>
              <IconChevronLeft className="h-4 w-4" />
            </Button>
            <Button variant="outline" size="icon" onClick={() => handlePageChange(pageIndex + 1)} disabled={pageIndex >= Math.ceil(totalCount / pageSize) - 1}>
              <IconChevronRight className="h-4 w-4" />
            </Button>
          </div>
        </div>
      </div>

      <Sheet open={dialogOpen} onOpenChange={setDialogOpen}>
        <SheetContent side="right" className="sm:max-w-[425px]">
          <SheetHeader>
            <SheetTitle>{editingRole ? "Edit Role" : "Create Role"}</SheetTitle>
            <SheetDescription>
              {editingRole ? "Update role information below." : "Fill in the information to create a new role."}
            </SheetDescription>
          </SheetHeader>
          <form onSubmit={handleSubmit}>
            <div className="grid gap-4 py-4">
              <div className="grid gap-2">
                <label htmlFor="name">Name *</label>
                <Input id="name" name="name" defaultValue={editingRole?.name} required />
              </div>
              <div className="grid gap-2">
                <label htmlFor="description">Description</label>
                <Input id="description" name="description" defaultValue={editingRole?.description} />
              </div>
            </div>
            <SheetFooter>
              <Button type="button" variant="outline" onClick={() => setDialogOpen(false)}>
                Cancel
              </Button>
              <Button type="submit">
                {editingRole ? "Save Changes" : "Create"}
              </Button>
            </SheetFooter>
          </form>
        </SheetContent>
      </Sheet>
    </div>
  )
}