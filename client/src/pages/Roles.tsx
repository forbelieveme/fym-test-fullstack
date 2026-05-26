import { useEffect, useState } from "react";
import { rolesApi, extractError, type Role } from "../api";

export default function Roles() {
  const [roles, setRoles] = useState<Role[]>([]);
  const [err, setErr] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    rolesApi
      .list()
      .then(setRoles)
      .catch((e) => setErr(extractError(e)))
      .finally(() => setLoading(false));
  }, []);

  return (
    <div>
      <h2>Roles</h2>
      {err && <p className="error">{err}</p>}
      {loading ? (
        <p>Loading…</p>
      ) : (
        <table>
          <thead>
            <tr>
              <th>Name</th>
              <th>Description</th>
            </tr>
          </thead>
          <tbody>
            {roles.map((r) => (
              <tr key={r.id}>
                <td>{r.name}</td>
                <td>{r.description ?? "—"}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}
