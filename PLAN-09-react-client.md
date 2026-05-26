# Step 9 — React Client

## Goal
Build a web app that consumes all the API endpoints and enforces the same role-based rules in the UI.

## Stack

- **Vite 6** — build tool and dev server (Vite 7+ requires Node 22; Vite 6 works with Node 20)
- **React 19 + TypeScript** — UI framework
- **react-router-dom** — client-side routing
- **axios** — HTTP client with interceptors

## Pages

| Route | Who sees it | What it does |
|-------|------------|--------------|
| `/login` | Anyone | Username + password form, stores JWT on success |
| `/users` | Logged-in users | Table of all users + roles. SuperAdmin also sees create form and assign-role button |
| `/roles` | Logged-in users | Table of all roles |

## JWT lifecycle

1. Login → API returns `accessToken`.
2. Token stored in `localStorage`.
3. `axios` request interceptor reads token from storage and attaches it to every request: `Authorization: Bearer <token>`.
4. `axios` response interceptor catches 401 responses → clears storage → redirects to `/login`.
5. `RequireAuth` component wraps protected routes → redirects to `/login` if no token in storage.

## Role-based UI

The "Create user" form and "+ role" button are only rendered when the logged-in user holds the `SuperAdmin` role. They are not just hidden — they are not present in the DOM at all for other roles.

The API still enforces the same restriction server-side independently. The UI check is convenience; the API check is the actual security boundary.

## API type definitions (api.ts)

```ts
interface Role {
  id: number
  name: string
  description?: string | null
}

interface AppUser {
  id: number
  userName: string
  email: string
  isActive: boolean
  createdAt: string
  roles: Role[]
}
```

## CORS

The API allows requests only from `http://localhost:5173` (the Vite dev server origin). Any other origin gets blocked at the browser level.

## Running

```bash
cd /Users/mbp/Documents/TEST/client
npm install
npm run dev
```

Opens at http://localhost:5173. Pre-filled login form uses seed credentials.

## Key files

- [client/src/api.ts](client/src/api.ts) — axios instance, interfaces, all API calls
- [client/src/auth.tsx](client/src/auth.tsx) — AuthProvider, useAuth hook, RequireAuth wrapper
- [client/src/pages/Login.tsx](client/src/pages/Login.tsx)
- [client/src/pages/Users.tsx](client/src/pages/Users.tsx)
- [client/src/pages/Roles.tsx](client/src/pages/Roles.tsx)
- [client/src/App.tsx](client/src/App.tsx) — router + layout
