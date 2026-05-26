import { Link, Navigate, Route, Routes, useNavigate } from "react-router-dom";
import Login from "./pages/Login";
import Users from "./pages/Users";
import Roles from "./pages/Roles";
import Me from "./pages/Me";
import { AuthProvider, RequireAuth, useAuth } from "./auth";
import { isAdminOrAbove } from "./api";
import "./App.css";

function RequireAdminOrAbove({ children }: { children: React.ReactNode }) {
  const { user } = useAuth();
  if (!isAdminOrAbove(user)) return <Navigate to="/me" replace />;
  return <>{children}</>;
}

function DefaultRedirect() {
  const { user } = useAuth();
  return <Navigate to={isAdminOrAbove(user) ? "/users" : "/me"} replace />;
}

function Layout({ children }: { children: React.ReactNode }) {
  const { user, logout } = useAuth();
  const nav = useNavigate();
  const admin = isAdminOrAbove(user);
  return (
    <div className="app">
      <header>
        <strong>FymUsers</strong>
        {user && (
          <nav>
            {admin ? (
              <>
                <Link to="/users">Users</Link>
                <Link to="/roles">Roles</Link>
              </>
            ) : (
              <Link to="/me">Me</Link>
            )}
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
            path="/me"
            element={
              <RequireAuth>
                <Me />
              </RequireAuth>
            }
          />
          <Route
            path="/users"
            element={
              <RequireAuth>
                <RequireAdminOrAbove>
                  <Users />
                </RequireAdminOrAbove>
              </RequireAuth>
            }
          />
          <Route
            path="/roles"
            element={
              <RequireAuth>
                <RequireAdminOrAbove>
                  <Roles />
                </RequireAdminOrAbove>
              </RequireAuth>
            }
          />
          <Route
            path="*"
            element={
              <RequireAuth>
                <DefaultRedirect />
              </RequireAuth>
            }
          />
        </Routes>
      </Layout>
    </AuthProvider>
  );
}
