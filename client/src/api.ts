import axios from "axios";

const API_BASE = import.meta.env.VITE_API_BASE ?? "http://localhost:5080";

export const api = axios.create({ baseURL: API_BASE });

const TOKEN_KEY = "fym.token";
const USER_KEY = "fym.user";

export function getToken(): string | null {
  return localStorage.getItem(TOKEN_KEY);
}

export function setSession(token: string, user: AppUser) {
  localStorage.setItem(TOKEN_KEY, token);
  localStorage.setItem(USER_KEY, JSON.stringify(user));
}

export function clearSession() {
  localStorage.removeItem(TOKEN_KEY);
  localStorage.removeItem(USER_KEY);
}

export function getStoredUser(): AppUser | null {
  const raw = localStorage.getItem(USER_KEY);
  return raw ? (JSON.parse(raw) as AppUser) : null;
}

api.interceptors.request.use((cfg) => {
  const t = getToken();
  if (t) cfg.headers.Authorization = `Bearer ${t}`;
  return cfg;
});

api.interceptors.response.use(
  (r) => r,
  (err) => {
    if (err.response?.status === 401) {
      clearSession();
      if (location.pathname !== "/login") location.href = "/login";
    }
    return Promise.reject(err);
  }
);

export interface Role {
  id: string;
  name: string;
  description?: string | null;
}

export interface AppUser {
  id: string;
  userName: string;
  email: string;
  isActive: boolean;
  createdAt: string;
  roles: Role[];
}

export interface LoginResponse {
  accessToken: string;
  expiresAtUtc: string;
  user: AppUser;
}

export const authApi = {
  login: (userName: string, password: string) =>
    api.post<LoginResponse>("/api/auth/login", { userName, password }).then((r) => r.data),
  register: (userName: string, email: string, password: string) =>
    api.post<LoginResponse>("/api/auth/register", { userName, email, password }).then((r) => r.data),
};

export const usersApi = {
  list: () => api.get<AppUser[]>("/api/users").then((r) => r.data),
  me: () => api.get<AppUser>("/api/users/me").then((r) => r.data),
  create: (body: { userName: string; email: string; password: string; roleIds: string[] }) =>
    api.post<AppUser>("/api/users", body).then((r) => r.data),
  assignRoles: (userId: string, roleIds: string[]) =>
    api.post<AppUser>(`/api/users/${userId}/roles`, { roleIds }).then((r) => r.data),
  removeRole: (userId: string, roleId: string) =>
    api.delete<AppUser>(`/api/users/${userId}/roles/${roleId}`).then((r) => r.data),
};

export const rolesApi = {
  list: () => api.get<Role[]>("/api/roles").then((r) => r.data),
};

export function isSuperAdmin(user: AppUser | null): boolean {
  return !!user?.roles.some((r) => r.name === "SuperAdmin");
}

export function isAdminOrAbove(user: AppUser | null): boolean {
  return !!user?.roles.some((r) => r.name === "Admin" || r.name === "SuperAdmin");
}

export function extractError(err: unknown): string {
  if (axios.isAxiosError(err)) {
    const data = err.response?.data as { detail?: string; title?: string; errors?: Record<string, string[]> } | undefined;
    if (data?.detail) return data.detail;
    if (data?.errors) return Object.values(data.errors).flat().join("; ");
    if (data?.title) return data.title;
    return err.message;
  }
  return (err as Error).message ?? "Unknown error";
}
