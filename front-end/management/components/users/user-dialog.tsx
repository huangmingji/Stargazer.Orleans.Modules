"use client"

import * as React from "react"

import { Button } from "@/components/ui/button"
import {
  Sheet,
  SheetContent,
  SheetDescription,
  SheetFooter,
  SheetHeader,
  SheetTitle,
} from "@/components/ui/sheet"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import type { UserData, CreateOrUpdateUserInput } from "@/lib/api"

interface UserDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  user?: UserData | null
  onSubmit: (data: CreateOrUpdateUserInput) => Promise<void>
}

export function UserDialog({
  open,
  onOpenChange,
  user,
  onSubmit,
}: UserDialogProps) {
  const [isLoading, setIsLoading] = React.useState(false)
  const [formData, setFormData] = React.useState<CreateOrUpdateUserInput>({
account: "",
    password: "",
    name: "",
    email: "",
    phone_number: "",
  })

  React.useEffect(() => {
    if (user) {
      setFormData({
        account: user.account,
        password: "",
        name: user.name || "",
        email: user.email || "",
        phone_number: user.phone_number || "",
      })
    } else {
      setFormData({
        account: "",
        password: "",
        name: "",
        email: "",
        phone_number: "",
      })
    }
  }, [user, open])

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setIsLoading(true)
    try {
      await onSubmit(formData)
      onOpenChange(false)
    } finally {
      setIsLoading(false)
    }
  }

  const handleChange = (field: keyof CreateOrUpdateUserInput) => (
    e: React.ChangeEvent<HTMLInputElement>
  ) => {
    setFormData((prev) => ({ ...prev, [field]: e.target.value }))
  }

  return (
    <Sheet open={open} onOpenChange={onOpenChange}>
      <SheetContent side="right" className="sm:max-w-[425px]">
        <SheetHeader>
          <SheetTitle>{user ? "Edit User" : "Create User"}</SheetTitle>
          <SheetDescription>
            {user
              ? "Update user information below."
              : "Fill in the information to create a new user."}
          </SheetDescription>
        </SheetHeader>
        <form onSubmit={handleSubmit} className="grid gap-4 py-4">
          <div className="grid gap-2">
            <Label htmlFor="account">Account *</Label>
            <Input
              id="account"
              value={formData.account}
              onChange={handleChange("account")}
              disabled={!!user || isLoading}
              required
            />
          </div>
          <div className="grid gap-2">
            <Label htmlFor="password">
              Password {user ? "(leave blank to keep)" : "*"}
            </Label>
            <Input
              id="password"
              type="password"
              value={formData.password}
              onChange={handleChange("password")}
              disabled={isLoading}
              required={!user}
            />
          </div>
          <div className="grid gap-2">
            <Label htmlFor="name">Name</Label>
            <Input
              id="name"
              value={formData.name}
              onChange={handleChange("name")}
              disabled={isLoading}
            />
          </div>
          <div className="grid gap-2">
            <Label htmlFor="email">Email</Label>
            <Input
              id="email"
              type="email"
              value={formData.email}
              onChange={handleChange("email")}
              disabled={isLoading}
            />
          </div>
          <div className="grid gap-2">
            <Label htmlFor="phone_number">Phone Number</Label>
            <Input
              id="phone_number"
              type="tel"
              value={formData.phone_number}
              onChange={handleChange("phone_number")}
              disabled={isLoading}
            />
          </div>
          <SheetFooter>
            <Button
              type="button"
              variant="outline"
              onClick={() => onOpenChange(false)}
              disabled={isLoading}
            >
              Cancel
            </Button>
            <Button type="submit" disabled={isLoading}>
              {isLoading ? "Saving..." : user ? "Save Changes" : "Create"}
            </Button>
          </SheetFooter>
        </form>
      </SheetContent>
    </Sheet>
  )
}