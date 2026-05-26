import { useEffect, useState } from "react";
import { usersApi, extractError, type AppUser } from "../api";

export default function Me() {
  const [me, setMe] = useState<AppUser | null>(null);
  const [err, setErr] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    usersApi
      .me()
      .then(setMe)
      .catch((e) => setErr(extractError(e)))
      .finally(() => setLoading(false));
  }, []);

  if (loading) return <p>Loading…</p>;
  if (err) return <p className="error">{err}</p>;
  if (!me) return null;

  return (
    <div className="card">
      <h2>My Profile</h2>
      <table>
        <tbody>
          <tr>
            <th>Username</th>
            <td>{me.userName}</td>
          </tr>
          <tr>
            <th>Email</th>
            <td>{me.email}</td>
          </tr>
          <tr>
            <th>Roles</th>
            <td>{me.roles.map((r) => r.name).join(", ") || "—"}</td>
          </tr>
          <tr>
            <th>Status</th>
            <td>{me.isActive ? "Active" : "Inactive"}</td>
          </tr>
          <tr>
            <th>Member since</th>
            <td>{new Date(me.createdAt).toLocaleString()}</td>
          </tr>
        </tbody>
      </table>
    </div>
  );
}
