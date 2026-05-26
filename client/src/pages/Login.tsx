import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../auth";
import { extractError } from "../api";

export default function Login() {
  const { login } = useAuth();
  const nav = useNavigate();
  const [userName, setUserName] = useState("superadmin");
  const [password, setPassword] = useState("SuperAdmin123!");
  const [err, setErr] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  async function submit(e: React.FormEvent) {
    e.preventDefault();
    setErr(null);
    setBusy(true);
    try {
      await login(userName, password);
      nav("/users", { replace: true });
    } catch (e) {
      setErr(extractError(e));
    } finally {
      setBusy(false);
    }
  }

  return (
    <div className="card">
      <h1>FymUsers — Sign in</h1>
      <form onSubmit={submit}>
        <label>
          Username
          <input value={userName} onChange={(e) => setUserName(e.target.value)} autoFocus required />
        </label>
        <label>
          Password
          <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} required />
        </label>
        {err && <p className="error">{err}</p>}
        <button disabled={busy}>{busy ? "Signing in…" : "Sign in"}</button>
      </form>
      <p className="hint">
        Seed credentials: <code>superadmin / SuperAdmin123!</code>
      </p>
    </div>
  );
}
