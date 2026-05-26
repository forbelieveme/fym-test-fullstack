import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../auth";
import { authApi, extractError, getStoredUser, isAdminOrAbove, setSession } from "../api";

type Mode = "login" | "register";

export default function Login() {
  const { login } = useAuth();
  const nav = useNavigate();

  const [mode, setMode] = useState<Mode>("login");
  const [userName, setUserName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [err, setErr] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  function switchMode(next: Mode) {
    setMode(next);
    setErr(null);
    if (next === "login") {
      setUserName("");
      setPassword("");
      setEmail("");
    } else {
      setUserName("");
      setPassword("");
      setEmail("");
    }
  }

  async function submit(e: React.FormEvent) {
    e.preventDefault();
    setErr(null);
    setBusy(true);
    try {
      if (mode === "login") {
        await login(userName, password);
        nav(isAdminOrAbove(getStoredUser()) ? "/users" : "/me", { replace: true });
        return;
      } else {
        const res = await authApi.register(userName, email, password);
        setSession(res.accessToken, res.user);
        window.location.replace(isAdminOrAbove(res.user) ? "/users" : "/me");
        return;
      }
    } catch (e) {
      setErr(extractError(e));
    } finally {
      setBusy(false);
    }
  }

  return (
    <div className="card">
      <h1>FymUsers — {mode === "login" ? "Sign in" : "Create account"}</h1>

      <div style={{ display: "flex", gap: "0.5rem", marginBottom: "1rem" }}>
        <button
          type="button"
          onClick={() => switchMode("login")}
          style={{ opacity: mode === "login" ? 1 : 0.5, flex: 1 }}
        >
          Sign in
        </button>
        <button
          type="button"
          onClick={() => switchMode("register")}
          style={{ opacity: mode === "register" ? 1 : 0.5, flex: 1 }}
        >
          Register
        </button>
      </div>

      <form onSubmit={submit}>
        <label>
          Username
          <input
            value={userName}
            onChange={(e) => setUserName(e.target.value)}
            autoFocus
            required
            minLength={3}
            maxLength={64}
          />
        </label>

        {mode === "register" && (
          <label>
            Email
            <input
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
              maxLength={256}
            />
          </label>
        )}

        <label>
          Password
          <input
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
            minLength={mode === "register" ? 8 : undefined}
          />
        </label>

        {mode === "register" && (
          <p style={{ fontSize: "0.85rem", opacity: 0.7, margin: "0.25rem 0 0.75rem" }}>
            New accounts are created with the <strong>User</strong> role.
          </p>
        )}

        {err && <p className="error">{err}</p>}

        <button disabled={busy}>
          {busy
            ? mode === "login" ? "Signing in…" : "Creating account…"
            : mode === "login" ? "Log in" : "Create account"}
        </button>
      </form>
    </div>
  );
}
