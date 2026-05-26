import { Link, Navigate, Route, Routes, useNavigate } from "react-router-dom";
import Login from "./pages/Login";
import Users from "./pages/Users";
import Roles from "./pages/Roles";
import { AuthProvider, RequireAuth, useAuth } from "./auth";
import "./App.css";

function Layout({ children }: { children: React.ReactNode }) {
  const { user, logout } = useAuth();
  const nav = useNavigate();
  return (
    <div className="app">
      <header>
        <strong>FymUsers</strong>
        {user && (
          <nav>
            <Link to="/users">Users</Link>
            <Link to="/roles">Roles</Link>
          </nav>
        )}
        <span className="spacer" />
        {user ? (
          <span>
            <span className="me">
              {user.userName} ({user.roles.map((r) => r.name).join(", ") || "no roles"})
            </span>
            <button
              onClick={() => {
                logout();
                nav("/login");
              }}
            >
              Sign out
            </button>
          </span>
        ) : null}
      </header>
      <main>{children}</main>
    </div>
  );
}

export default function App() {
  return (
    <AuthProvider>
      <Layout>
        <Routes>
          <Route path="/login" element={<Login />} />
          <Route
            path="/users"
            element={
              <RequireAuth>
                <Users />
              </RequireAuth>
            }
          />
          <Route
            path="/roles"
            element={
              <RequireAuth>
                <Roles />
              </RequireAuth>
            }
          />
          <Route path="*" element={<Navigate to="/users" replace />} />
        </Routes>
      </Layout>
    </AuthProvider>
  );
}
